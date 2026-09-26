# BUG-004: `DeviceConsole.GetSerialPort` throws the wrong exception type with a typo'd message

- **Component**: `BinaryDataDecoders.IO.Controller.Cli`
- **File**: `src/BinaryDataDecoders.IO.Controller.Cli/DeviceConsole.cs`
- **Severity**: Low (cosmetic / diagnostics quality)
- **Status**: Confirmed by code inspection

## Summary

When a serial device can't be configured, `GetSerialPort` throws a bare
`NullReferenceException` with a message that contains a typo:

```csharp
var serialPort = serial.GetDevice(portName, definition: definition) ??
     throw new NullReferenceException($"Enable to configure \"{portName}\" for \"{definition}\"");
```

- "Enable to configure" should read "**Unable** to configure".
- `NullReferenceException` is reserved by convention for genuine null-
  dereference bugs; throwing it deliberately here is misleading to anyone
  debugging a real NRE elsewhere, and gives callers no clean way to
  distinguish "port failed to open" from an actual null-ref bug.

The sibling method `GetHidDevice` has the exact same pattern:

```csharp
private IDeviceAdapter GetHidDevice(object definition) =>
    usbHid.GetDevice(definition: definition) ??
         throw new NullReferenceException($"Enable to configure \"{definition}\"");
```

## Suggested fix

Use a more appropriate exception type (e.g. `InvalidOperationException` or
a custom `DeviceNotAvailableException`) and fix the typo, in both
`GetSerialPort` and `GetHidDevice`:

```csharp
throw new InvalidOperationException($"Unable to configure \"{portName}\" for \"{definition}\"");
```
