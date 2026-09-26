# BUG-003: `SerialPortDeviceAdapter.TryOpen` crashes the app when the port is busy

- **Component**: `BinaryDataDecoders.IO.Ports`
- **File**: `src/BinaryDataDecoders.IO.Ports/SerialPortDeviceAdapter.cs`
- **Severity**: Medium-High (unhandled exception crashes the whole CLI instead of a graceful "device unavailable")
- **Status**: Reproduced live (see stack trace below)

## Summary

`TryOpen` is written to be a non-throwing "try" method — it's meant to
return `false` if the device can't be opened. It only catches `IOException`,
but opening a `SerialPort` that is already in use by another process throws
`UnauthorizedAccessException`, not `IOException`. That exception is not
caught, so it propagates all the way out of `async Task Main`, printing an
"Unhandled exception" dump and terminating the process instead of the
intended "port not found / not available" handling.

## Where

```csharp
public bool TryOpen(out Stream? stream)
{
    if (device.IsOpen)
    {
        stream = device.BaseStream;
        return true;
    }

    try
    {
        device.Open();
        stream = device.BaseStream;
        return true;
    }
    catch (IOException)          // <-- misses UnauthorizedAccessException
    {
        stream = null;
        return false;
    }
}
```

## Repro / observed stack trace

Running the CLI against a COM port already opened by another (zombie, see
BUG-002) instance of the same app:

```
Unhandled exception. System.UnauthorizedAccessException: Access to the path 'COM8' is denied.
   at System.IO.Ports.SerialStream..ctor(...)
   at System.IO.Ports.SerialPort.Open()
   at BinaryDataDecoders.IO.Ports.SerialPortDeviceAdapter.TryOpen(Stream& stream) in .../SerialPortDeviceAdapter.cs:line 29
   at BinaryDataDecoders.IO.Controller.Cli.DeviceConsole.<>c__DisplayClass11_0`1.<<Execute>g__HandleStream|0>d.MoveNext() in .../DeviceConsole.cs:line 62
   at BinaryDataDecoders.IO.Controller.Cli.DeviceConsole.Execute[TMessage](...) in .../DeviceConsole.cs:line 106
   at BinaryDataDecoders.IO.Controller.Cli.Program.Main(String[] args) in .../Program.cs:line 60
```

## Root cause

`SerialPort.Open()` can throw several exception types depending on why the
port could not be opened, notably:

- `UnauthorizedAccessException` — port already open elsewhere, or access
  denied
- `IOException` — port doesn't exist / device error
- `ArgumentException` / `InvalidOperationException` — bad configuration

`TryOpen` only guards against `IOException`, so the other realistic failure
modes (in particular "port already in use", which is a common, expected
condition — e.g. a previous run left the port open, per BUG-002) crash the
whole application instead of being reported as "couldn't open the device".

## Suggested fix

Widen the catch to cover the realistic set of `SerialPort.Open()` failure
exceptions, e.g.:

```csharp
catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidOperationException)
{
    stream = null;
    return false;
}
```

Consider also surfacing `ex.Message` (e.g. via a `TryOpen(out stream, out string? error)` overload, or a logged event) so the caller can tell the user *why* the port couldn't be opened (e.g. "already in use") rather than just failing silently or generically.
