# Full-stack bug review — status (paused)

Paused on 2026-09-26 at the user's request ("stop the review for now"). This
file is the resume point.

## Done

Deep-dive review of the serial/IO device stack (found while testing the
Radex One CLI on COM8). Filed and still valid:

- `BUG-001-deviceconsole-always-throws-after-session.md` — `DeviceConsole.HandleStream` always throws after a successful session (missing `else`).
- `BUG-002-serial-session-never-exits-after-cancel.md` — serial receive loop hangs forever after cancel; COM port never released.
- `BUG-003-serialportdeviceadapter-tryopen-unhandled-exception.md` — `TryOpen` only catches `IOException`, so a busy port crashes the app via `UnauthorizedAccessException`.
- `BUG-004-deviceconsole-getserialport-error-message.md` — wrong exception type + typo in `GetSerialPort`/`GetHidDevice` error messages.

Covered projects (no further review needed): `BinaryDataDecoders.IO.Pipelines`,
`BinaryDataDecoders.IO.Ports`, `BinaryDataDecoders.IO.Controller.Cli`,
`BinaryDataDecoders.Quarta.RadexOne`.

## Not started (stopped before producing any output)

Four parallel review passes were launched to cover the rest of the ~39
main projects, but were killed by user request before any of them wrote a
single report file. Nothing from this batch exists yet — resuming means
re-running these from scratch, not picking up partial work.

Planned split (still valid, re-issue as-is when resuming):

1. **Hardware/device-IO** (prefix would've been `BUG-IOA-*`):
   `BinaryDataDecoders.IO.Abstractions`, `BinaryDataDecoders.IO`,
   `BinaryDataDecoders.IO.UsbHids` (+.Tests), `BinaryDataDecoders.Velleman.K8055`,
   `BinaryDataDecoders.Kuando.Busylight` (+.Tests),
   `BinaryDataDecoders.EByteElectronicTechnology` (+.Tests),
   `BinaryDataDecoders.Zoom.H4n`, `BinaryDataDecoders.Rigol` (+.Tests),
   `BinaryDataDecoders.ElectronicScoringMachines.Fencing` (+.Tests),
   `BinaryDataDecoders.Nmea` (+.Tests), `BinaryDataDecoders.LanC`.

2. **Text/data processing & core toolkit** (prefix `BUG-TXT-*`):
   `BinaryDataDecoders.ToolKit` (+.Tests), `BinaryDataDecoders.ToolKit.Abstractions`,
   `BinaryDataDecoders.Cryptography` (+.Tests), `BinaryDataDecoders.ExpressionCalculator` (+.Tests),
   `BinaryDataDecoders.Text.Markdown` (+.Tests), `BinaryDataDecoders.Text.Json` (+.Tests),
   `BinaryDataDecoders.Yaml` (+.Tests), `BinaryDataDecoders.Templating.Abstractions`,
   `BinaryDataDecoders.Templating.Html` (+.Tests).
   (Note: `ToolKit/ReadOnlySpanEx.cs:32` ambiguous-overload bug already fixed this session — don't re-report.)

3. **Code analysis, archives, filesystem, networking** (prefix `BUG-SYS-*`):
   `BinaryDataDecoders.CodeAnalysis` (+.Tests), `BinaryDataDecoders.CodeAnalysis.StructuredLog` (+.Tests),
   `BinaryDataDecoders.CodeAnalysis.DacFx` (+.Tests), `BinaryDataDecoders.Archives` (+.Tests),
   `BinaryDataDecoders.FileSystems` (+.Tests), `BinaryDataDecoders.Net` (+.Tests),
   `BinaryDataDecoders.Net.ServiceHost.Cli`, `BinaryDataDecoders.Apple2` (+.Tests).

4. **Apps/UI/misc tooling** (prefix `BUG-APP-*`):
   `BinaryDataDecoders.Drawing` (+.Tests), `BinaryDataDecoders.Windows.Forms`,
   `BinaryDataDecoders.WinForms.Samples`, `BinaryDataDecoders.PackMan.Cli`,
   `BinaryDataDecoders.Xslt.Cli`, `BinaryDataDecoders.Notes`,
   `BinaryDataDecoders.TestUtilities` (MSTest 4.4.1 migration already done this
   session — don't re-report `ContextualTestMethodAttribute.cs`/`ContextualTestClassBase.cs`),
   plus a quick low-priority pass on `BinaryDataDecoders.SqlServer.Samples`.

## How to resume

Say "resume the full-stack review" (or similar). Next step is to re-launch
groups 1-4 above as parallel fork agents (or work through them serially),
same bar as BUG-001..004: confirmed bugs only (exact file/line + concrete
failure scenario), reports written into `docs/bug-reports/` with the prefix
shown per group, then renumber everything into one consistent `BUG-0xx`
sequence once all groups are back.
