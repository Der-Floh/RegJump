# ![RegJump icon](https://raw.githubusercontent.com/Der-Floh/RegJump/main/Assets/icon-x64.png) RegJump

[![NuGet Version](https://img.shields.io/nuget/vpre/RegJump)](https://www.nuget.org/packages/RegJump)
[![NuGet Downloads](https://img.shields.io/nuget/dt/RegJump)](https://www.nuget.org/packages/RegJump)
[![CI](https://github.com/Der-Floh/RegJump/actions/workflows/ci.yml/badge.svg)](https://github.com/Der-Floh/RegJump/actions/workflows/ci.yml)

A lightweight Windows library that opens the Windows Registry Editor directly at a specified registry key path.

## Features

- Open the registry at any path with a single method call
- Optional elevation via UAC (`elevated`) or suppressed UAC prompt
- Returns the `Process` handle of the launched Registry Editor
- Accepts hive abbreviations, full hive names, forward slashes, and paths copied straight from the Registry Editor address bar
- Validates the path up front and throws instead of silently opening the wrong location

## Requirements

- **Windows** (Windows-only library)
- **.NET Standard 2.0** or higher (compatible with .NET Framework 4.6.1+, .NET Core 2.0+, .NET 5+)

## Installation

Install via the .NET CLI:

```shell
dotnet add package RegJump
```

## Usage

### Open Registry Editor at a specific path

```csharp
// Open regedit at a specific key (no elevation)
RegJump.Open(@"HKCU\Software\Microsoft\Windows\CurrentVersion");

// Using full hive name
RegJump.Open(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services");
```

### Open Registry Editor with elevated privileges

```csharp
// Opens regedit via UAC prompt
RegJump.Open(@"HKLM\SYSTEM\CurrentControlSet\Control", elevated: true);
```

### Open Registry Editor without navigating to a path

```csharp
// Just opens regedit at its last visited location
RegJump.Open();

// With elevation
RegJump.Open(elevated: true);
```

### Using the returned Process handle

```csharp
using System.Diagnostics;

Process regedit = RegJump.Open(@"HKCU\Software\MyApp");
regedit.WaitForExit();
```

## API Reference

### `RegJump.Open(string path, bool elevated = false)`

Opens the Windows Registry Editor and navigates to `path`.

| Parameter | Type     | Description                                                                                            |
| --------- | -------- | ------------------------------------------------------------------------------------------------------ |
| `path`    | `string` | Registry path in the format `HIVE\SubKey\Path`                                                         |
| `elevated` | `bool`  | `true` to request elevation via UAC; `false` (default) to run as the current user without a UAC prompt |

**Returns:** `Process` — the started Registry Editor process.

**Throws:**

| Exception               | When                                                              |
| ----------------------- | ----------------------------------------------------------------- |
| `ArgumentNullException` | `path` is `null`                                                  |
| `ArgumentException`     | `path` is empty or does not start with a recognised registry hive |

---

### `RegJump.OpenAt(string path, bool elevated = false)`

Deprecated alias for `Open(string path, bool elevated)`. Identical behaviour; marked `[Obsolete]` and slated for
removal in a future major version. Use `Open` instead.

---

### `RegJump.Open(bool elevated = false)`

Opens the Windows Registry Editor at its last visited location.

| Parameter | Type   | Description                                                                                            |
| --------- | ------ | ------------------------------------------------------------------------------------------------------ |
| `elevated` | `bool` | `true` to request elevation via UAC; `false` (default) to run as the current user without a UAC prompt |

**Returns:** `Process` — the started Registry Editor process.

---

### Supported hive prefixes

| Abbreviation | Full name               |
| ------------ | ----------------------- |
| `HKLM`       | `HKEY_LOCAL_MACHINE`    |
| `HKCU`       | `HKEY_CURRENT_USER`     |
| `HKCR`       | `HKEY_CLASSES_ROOT`     |
| `HKU`        | `HKEY_USERS`            |
| `HKCC`       | `HKEY_CURRENT_CONFIG`   |
| `HKPD`       | `HKEY_PERFORMANCE_DATA` |

Hive names are matched case-insensitively. Forward slashes are accepted as separators, surrounding quotes and
trailing separators are stripped, and an optional leading `Computer\` is ignored — so a path copied from the
Registry Editor address bar can be passed through unchanged:

```csharp
RegJump.Open(@"Computer\HKEY_CURRENT_USER\Control Panel\Cursors");
```

A path that is empty or does not begin with one of the hives above throws `ArgumentException` rather than
launching the Registry Editor at an arbitrary location.

## How It Works

`RegJump` navigates the Registry Editor by writing the desired path to  
`HKCU\Software\Microsoft\Windows\CurrentVersion\Applets\Regedit\LastKey` before launching `regedit.exe` — the same mechanism used by Sysinternals RegJump.

When `elevated` is `false`, `regedit.exe` is started directly with `__COMPAT_LAYER=RUNASINVOKER` in its environment to
suppress the automatic UAC elevation prompt, ensuring the editor opens in the current user context regardless of
manifest settings.

## Notes

The Registry Editor is a single-instance application. If it is already running, launching it again simply focuses
the existing window, and that window does **not** re-read the stored path — so `Open(path)` will not re-navigate an
already open Registry Editor. Close it first if you need the jump to take effect.

## Contributing

Contributions are welcome. Please open an issue first to discuss what you would like to change, then submit a pull request.

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/my-feature`)
3. Commit your changes (`git commit -m 'Add my feature'`)
4. Push to the branch (`git push origin feature/my-feature`)
5. Open a Pull Request

## License

This project is licensed under the [MIT License](LICENSE).
