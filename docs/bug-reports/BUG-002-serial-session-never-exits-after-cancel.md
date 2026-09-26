# BUG-002: Serial device session hangs forever after "Enter to exit", leaking the COM port

- **Component**: `BinaryDataDecoders.IO.Pipelines`, `BinaryDataDecoders.IO.Ports`, `BinaryDataDecoders.IO.Controller.Cli`
- **Files**:
  - `src/BinaryDataDecoders.IO.Pipelines/StreamDevice.cs`
  - `src/BinaryDataDecoders.IO.Pipelines/Factories/StreamPipelineFactory.cs`
  - `src/BinaryDataDecoders.IO.Abstractions/Ports/SerialPortAttribute.cs`
  - `src/BinaryDataDecoders.IO.Controller.Cli/DeviceConsole.cs`
- **Severity**: High (process never terminates; requires manual `kill`; blocks the COM port for any other process/run)
- **Status**: Reproduced live against a Radex One on COM8

## Summary

After pressing Enter at the "Enter to exit" prompt, the CLI prints `Done!`
but the process does not actually exit. The underlying `dotnet.exe` /
`*.exe` process keeps running indefinitely and keeps the serial port open,
so a second run against the same port fails with
`System.UnauthorizedAccessException: Access to the path 'COM8' is denied.`
(see BUG-003). The only way to free the port is to kill the process
manually.

## Repro

1. Run `BinaryDataDecoders.IO.Controller.Cli` with the Radex One definition
   against a real device on a COM port.
2. Let it poll for any length of time, then press Enter.
3. Console prints `Enter to exit` → `Done!`.
4. `Get-Process`/`Get-CimInstance Win32_Process` still shows the app's
   `dotnet.exe` and `BinaryDataDecoders.IO.Controller.Cli.exe` processes
   alive minutes later.
5. A subsequent run against the same COM port fails to open it.

## Root cause

`DeviceConsole.Execute` awaits:

```csharp
await Task.WhenAll(
    Task.Run(() => { Console.ReadLine(); _tokenSource.Cancel(); Console.WriteLine("Done!"); }, _tokenSource.Token),
    uiTasks ?? Task.FromResult(0),
    streamDevice.Runner
);
```

`_tokenSource.Cancel()` cancels the token that `StreamDevice<TMessage>`
links into (`StreamDevice.cs`, constructor: `CancellationTokenSource.CreateLinkedTokenSource(token)`).
`StreamDevice.Runner` is `Task.WhenAll(deviceInitializer, messageReceiver, messageTransmitter)`,
and `messageReceiver` (`Receiver`) loops:

```csharp
while (!_token.IsCancellationRequested)
{
    await mre.WaitAsync();
    ...
    await _stream.Follow()
                 .With(_segmentDefintion.ThenAs(_decoder, OnMessageReceived))
                 .RunAsync(_token)
                 .ConfigureAwait(false);
    ...
}
```

`RunAsync` ultimately calls `StreamPipelineFactory.CreateWriter`, which does:

```csharp
var read = await context.owner.ReadAsync(memory, context.cancellationToken);
```

where `context.owner` is `device.BaseStream` from a `System.IO.Ports.SerialPort`
(`SerialPortDeviceAdapter.Stream`). `SerialPortAttribute.ReadTimeout` defaults
to `-1` (infinite) and is never overridden for the Radex One definition, and
`SerialPortFactory.GetDevice` applies that timeout directly to the
`SerialPort`. When no further bytes arrive on the wire (which is exactly
what happens once the CLI stops sending requests), the pending
`ReadAsync(memory, cancellationToken)` call on the serial stream does not
observe/honor the passed `CancellationToken` and never completes or throws
`OperationCanceledException`. Because nothing else in the receive path has a
timeout or a way to force the read to abort (e.g. closing/disposing the
underlying `SerialPort`/stream), the `await` blocks forever, so:

- `messageReceiver` never finishes →
- `streamDevice.Runner` never finishes →
- the outer `Task.WhenAll` in `DeviceConsole.Execute` never finishes →
- `HandleStream` never returns, and the process never exits.

This is consistent with what was observed: no error/exception message is
ever printed by `MessageReceivedError` (which the CLI wires up to log to
`Console.Error`), meaning the pending read never faults — it simply never
completes.

## Suggested fix

Cancellation of a pending blocking/serial read generally has to be forced
from the outside, not merely requested via a `CancellationToken`. Options:

1. On cancellation, actively close/dispose the `SerialPort`/`Stream` (e.g.
   register `_token.Register(() => device.Close())` or similar) so the
   pending `ReadAsync` faults immediately with an `IOException`/
   `ObjectDisposedException` that the existing catch/`ErrorHandling.Ignore`
   path already knows how to handle.
2. Alternatively, give the serial port a finite `ReadTimeout` (e.g. a few
   hundred ms) so the read loop periodically wakes up and re-checks
   `_token.IsCancellationRequested` instead of blocking indefinitely.
3. Add a hard timeout/fallback around `streamDevice.Runner` in
   `DeviceConsole.Execute` (e.g. `Task.WhenAny` with a grace-period timeout)
   so the CLI can still exit and report a warning even if a lower layer
   fails to unwind promptly.

Any of these should be paired with verifying, on the target OS, that
`System.IO.Ports.SerialStream.ReadAsync` actually honors cancellation for a
pending read with no incoming data — historically this has been unreliable
across `System.IO.Ports` versions/platforms.
