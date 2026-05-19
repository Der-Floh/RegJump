# ![RegJump icon](https://raw.githubusercontent.com/Der-Floh/RegJump/main/Assets/icon-x64.png) RegJump

[![NuGet Version](https://img.shields.io/nuget/vpre/RegJump)](https://www.nuget.org/packages/RegJump)
[![NuGet Downloads](https://img.shields.io/nuget/dt/RegJump)](https://www.nuget.org/packages/RegJump)

A lightweight Windows library that opens the Windows Registry Editor directly at a specified registry key path.

## Features

- Open the registry at any path with a single method call
- Optional elevation via UAC (`runas`) or suppressed UAC prompt
- Returns the `Process` handle of the launched Registry Editor

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

// Using the OpenAt alias
RegJump.OpenAt(@"HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion");
```

### Open Registry Editor with elevated privileges

```csharp
// Opens regedit via UAC prompt (runas)
RegJump.Open(@"HKLM\SYSTEM\CurrentControlSet\Control", runas: true);
```

### Open Registry Editor without navigating to a path

```csharp
// Just opens regedit at its last visited location
RegJump.Open();

// With elevation
RegJump.Open(runas: true);
```

### Using the returned Process handle

```csharp
using System.Diagnostics;

Process regedit = RegJump.Open(@"HKCU\Software\MyApp");
regedit.WaitForExit();
```

## API Reference

### `RegJump.Open(string path, bool runas = false)`

Opens the Windows Registry Editor and navigates to `path`.

| Parameter | Type     | Description                                                                                            |
| --------- | -------- | ------------------------------------------------------------------------------------------------------ |
| `path`    | `string` | Registry path in the format `HIVE\SubKey\Path`                                                         |
| `runas`   | `bool`   | `true` to request elevation via UAC; `false` (default) to run as the current user without a UAC prompt |

**Returns:** `Process` — the started Registry Editor process.

---

### `RegJump.OpenAt(string path, bool runas = false)`

Alias for `Open(string path, bool runas)`. Identical behaviour.

---

### `RegJump.Open(bool runas = false)`

Opens the Windows Registry Editor at its last visited location.

| Parameter | Type   | Description                                                                                            |
| --------- | ------ | ------------------------------------------------------------------------------------------------------ |
| `runas`   | `bool` | `true` to request elevation via UAC; `false` (default) to run as the current user without a UAC prompt |

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

## How It Works

`RegJump` navigates the Registry Editor by writing the desired path to  
`HKCU\Software\Microsoft\Windows\CurrentVersion\Applets\Regedit\LastKey` before launching `regedit.exe` — the same mechanism used by Sysinternals RegJump.

When `runas` is `false`, the process is started with `__COMPAT_LAYER=RUNASINVOKER` to suppress any automatic UAC elevation prompt, ensuring the editor opens in the current user context regardless of manifest settings.

## Contributing

Contributions are welcome. Please open an issue first to discuss what you would like to change, then submit a pull request.

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/my-feature`)
3. Commit your changes (`git commit -m 'Add my feature'`)
4. Push to the branch (`git push origin feature/my-feature`)
5. Open a Pull Request

## License

This project is licensed under the [MIT License](LICENSE).
