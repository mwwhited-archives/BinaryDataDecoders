# BUG-001: `DeviceConsole.Execute` always throws after a successful session

- **Component**: `BinaryDataDecoders.IO.Controller.Cli`
- **File**: `src/BinaryDataDecoders.IO.Controller.Cli/DeviceConsole.cs`
- **Severity**: High (every successful run ends in an unhandled exception)
- **Status**: Confirmed by code inspection

## Summary

`DeviceConsole.Execute<TMessage>`'s local `HandleStream` function throws
`ApplicationException($"{device} Not found")` unconditionally, even when the
device was opened and the session ran to completion normally.

## Where

```csharp
async Task HandleStream(IDeviceAdapter device)
{
    if (device.TryOpen(out var stream))
        using (stream ?? throw new ApplicationException())
        {
            // ... run the session ...
        }
    throw new ApplicationException($"{device} Not found");   // <-- always executes
}
```

(`DeviceConsole.cs`, `Execute` → `HandleStream`, roughly lines 60-96.)

## Root cause

The `if (device.TryOpen(...))` has no braces, so only the `using` statement is
the body of the `if`. The `throw` on the last line is **not** part of the
`if`/`using` — it is the next statement in the method, so control flow always
reaches it once the `using` block finishes, regardless of whether `TryOpen`
returned `true` or `false`. The `throw` was clearly intended to run only in
the `TryOpen == false` case (an `else` branch), but the `else` is missing.

## Impact

Once a device session completes (user presses Enter, `Task.WhenAll`
resolves), the method throws `ApplicationException`, which is unhandled in
`Program.Main` (an `async Task Main`). Every otherwise-successful CLI run
ends as a crash. This has not been observed directly in manual testing only
because a separate hang bug (see BUG-002) currently prevents the `using`
block from ever completing.

## Suggested fix

Add an `else` (or restructure with a `return`) so the "not found" throw only
happens when `TryOpen` fails:

```csharp
async Task HandleStream(IDeviceAdapter device)
{
    if (!device.TryOpen(out var stream))
        throw new ApplicationException($"{device} Not found");

    using (stream ?? throw new ApplicationException())
    {
        // ... run the session ...
    }
}
```
