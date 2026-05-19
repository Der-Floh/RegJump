using System.Diagnostics;

using Microsoft.Win32;

public static class RegJump
{
    private const string LastKey = @"HKCU\Software\Microsoft\Windows\CurrentVersion\Applets\Regedit\LastKey";

    /// <summary>
    /// Opens a registry key at the specified path, supporting common registry hive abbreviations.
    /// </summary>
    /// <param name="path">The registry path in the format "HIVE\SubKey\Path", where HIVE can be HKLM, HKCU, HKU, HKCC, HKCR, HKPD or their
    /// full names.</param>
    /// <param name="writable">Indicates whether the registry key should be opened with write access.</param>
    /// <returns>The opened registry key, or <see langword="null"/> if the hive is not recognized or the key does not exist.</returns>
    private static RegistryKey? OpenPath(string path, bool writable = false)
    {
        path = path.Trim().Trim('\"').Replace("/", "\\");
        var paths = path.Split(['\\'], 2);
        var location = paths[0].ToUpper();
        path = paths[1];

        switch (location)
        {
            case "HKLM":
            case "HKEY_LOCAL_MACHINE":
                return Registry.LocalMachine.OpenSubKey(path, writable);
            case "HKCU":
            case "HKEY_CURRENT_USER":
                return Registry.CurrentUser.OpenSubKey(path, writable);
            case "HKU":
            case "HKEY_USERS":
                return Registry.Users.OpenSubKey(path, writable);
            case "HKCC":
            case "HKEY_CURRENT_CONFIG":
                return Registry.CurrentConfig.OpenSubKey(path, writable);
            case "HKCR":
            case "HKEY_CLASSES_ROOT":
                return Registry.ClassesRoot.OpenSubKey(path, writable);
            case "HKPD":
            case "HKEY_PERFORMANCE_DATA":
                return Registry.PerformanceData.OpenSubKey(path, writable);
            default:
                break;
        }
        return null;
    }

    /// <summary>
    /// Sets a registry value at the specified path with the given data and value kind.
    /// </summary>
    /// <param name="path">The full registry path including the value name.</param>
    /// <param name="value">The data to store in the registry value.</param>
    /// <param name="kind">The data type of the registry value. If <see cref="RegistryValueKind.None"/>, the type is determined automatically.</param>
    private static void SetKey(string path, object value, RegistryValueKind kind = RegistryValueKind.String)
    {
        var key = Path.GetFileName(path);
        path = Path.GetDirectoryName(path);

        var regKey = OpenPath(path, true);
        if (kind == RegistryValueKind.None)
            regKey?.SetValue(key, value);
        else
            regKey?.SetValue(key, value, kind);
    }

    /// <summary>
    /// Opens the registry editor at the specified registry path.
    /// When opened without admin privileges, only keys that do not require elevation can be edited; all other keys are read-only.
    /// </summary>
    /// <param name="path">The registry path where the registry should be opened.</param>
    /// <param name="runas">Whether to open with elevated privileges.</param>
    public static Process OpenAt(string path, bool runas = false)
        => Open(path, runas);

    /// <summary>
    /// Opens the registry editor at the specified registry path.
    /// When opened without admin privileges, only keys that do not require elevation can be edited; all other keys are read-only.
    /// </summary>
    /// <param name="path">The registry path where the registry should be opened.</param>
    /// <param name="runas">Whether to open with elevated privileges.</param>
    public static Process Open(string path, bool runas = false)
    {
        SetKey(LastKey, path);
        return Open(runas);
    }

    /// <summary>
    /// Opens the Windows Registry Editor.
    /// When opened without admin privileges, only keys that do not require elevation can be edited; all other keys are read-only.
    /// </summary>
    /// <param name="runas">Whether to open with elevated privileges.</param>
    public static Process Open(bool runas = false)
    {
        var psi = runas
            ? new ProcessStartInfo()
            {
                FileName = "regedit.exe",
                UseShellExecute = true,
                Verb = "runas",
            }
            : new ProcessStartInfo()
            {
                FileName = "cmd.exe",
                Arguments = "/c \"set __COMPAT_LAYER=RUNASINVOKER && start \"\" regedit.exe\"",
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
            };

        return Process.Start(psi);
    }
}
