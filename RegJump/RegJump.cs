using System.Diagnostics;

using Microsoft.Win32;

/// <summary>
/// Opens the Windows Registry Editor, optionally navigating straight to a specific key.
/// </summary>
public static class RegJump
{
    private const string RegeditExecutable = "regedit.exe";
    private const string RegeditKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Applets\Regedit";
    private const string LastKeyValueName = "LastKey";
    private const string CompatLayerVariable = "__COMPAT_LAYER";
    private const string RunAsInvokerLayer = "RUNASINVOKER";
    private const string ElevationVerb = "runas";
    private const string ComputerPrefix = "Computer";

    private static readonly Dictionary<string, string> HiveNames = new(StringComparer.OrdinalIgnoreCase)
    {
        ["HKCR"] = "HKEY_CLASSES_ROOT",
        ["HKEY_CLASSES_ROOT"] = "HKEY_CLASSES_ROOT",
        ["HKCU"] = "HKEY_CURRENT_USER",
        ["HKEY_CURRENT_USER"] = "HKEY_CURRENT_USER",
        ["HKLM"] = "HKEY_LOCAL_MACHINE",
        ["HKEY_LOCAL_MACHINE"] = "HKEY_LOCAL_MACHINE",
        ["HKU"] = "HKEY_USERS",
        ["HKEY_USERS"] = "HKEY_USERS",
        ["HKCC"] = "HKEY_CURRENT_CONFIG",
        ["HKEY_CURRENT_CONFIG"] = "HKEY_CURRENT_CONFIG",
        ["HKPD"] = "HKEY_PERFORMANCE_DATA",
        ["HKEY_PERFORMANCE_DATA"] = "HKEY_PERFORMANCE_DATA",
    };

    /// <summary>
    /// Opens the Registry Editor at the specified registry path.
    /// When opened without admin privileges, only keys that do not require elevation can be edited; all other keys are read-only.
    /// </summary>
    /// <param name="path">The registry path to navigate to, in the format <c>HIVE\SubKey\Path</c>. The hive may be an
    /// abbreviation (<c>HKLM</c>) or a full name (<c>HKEY_LOCAL_MACHINE</c>), and an optional leading <c>Computer\</c>
    /// is accepted so paths copied from the Registry Editor address bar can be passed through unchanged.</param>
    /// <param name="elevated">Whether to open with elevated privileges.</param>
    /// <returns>The started Registry Editor process.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="path"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="path"/> is empty or does not start with a known registry hive.</exception>
    [Obsolete("Use Open(string, bool) instead. OpenAt will be removed in a future major version.")]
    public static Process OpenAt(string path, bool elevated = false)
        => Open(path, elevated);

    /// <summary>
    /// Opens the Registry Editor at the specified registry path.
    /// When opened without admin privileges, only keys that do not require elevation can be edited; all other keys are read-only.
    /// </summary>
    /// <param name="path">The registry path to navigate to, in the format <c>HIVE\SubKey\Path</c>. The hive may be an
    /// abbreviation (<c>HKLM</c>) or a full name (<c>HKEY_LOCAL_MACHINE</c>), and an optional leading <c>Computer\</c>
    /// is accepted so paths copied from the Registry Editor address bar can be passed through unchanged.</param>
    /// <param name="elevated">Whether to open with elevated privileges.</param>
    /// <returns>The started Registry Editor process.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="path"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="path"/> is empty or does not start with a known registry hive.</exception>
    public static Process Open(string path, bool elevated = false)
    {
        if (path is null)
            throw new ArgumentNullException(nameof(path));

        SetLastKey(NormalizePath(path));
        return Open(elevated);
    }

    /// <summary>
    /// Opens the Registry Editor at its last visited location.
    /// When opened without admin privileges, only keys that do not require elevation can be edited; all other keys are read-only.
    /// </summary>
    /// <param name="elevated">Whether to open with elevated privileges.</param>
    /// <returns>The started Registry Editor process.</returns>
    public static Process Open(bool elevated = false)
    {
        var startInfo = new ProcessStartInfo(RegeditExecutable);
        if (elevated)
        {
            startInfo.UseShellExecute = true;
            startInfo.Verb = ElevationVerb;
        }
        else
        {
            startInfo.UseShellExecute = false;
            startInfo.EnvironmentVariables[CompatLayerVariable] = RunAsInvokerLayer;
        }

        return Process.Start(startInfo) ?? throw new InvalidOperationException($"Failed to start {RegeditExecutable}.");
    }

    /// <summary>
    /// Expands a hive abbreviation to its full name and strips any leading <c>Computer\</c> segment.
    /// </summary>
    private static string NormalizePath(string path)
    {
        var cleaned = path.Trim().Trim('"').Replace('/', '\\').Trim('\\');
        if (cleaned.Length == 0)
            throw new ArgumentException("Registry path must not be empty.", nameof(path));

        var segments = cleaned.Split(['\\'], 2);
        if (segments.Length == 2 && segments[0].Equals(ComputerPrefix, StringComparison.OrdinalIgnoreCase))
            segments = segments[1].Split(['\\'], 2);

        if (!HiveNames.TryGetValue(segments[0], out var hive))
            throw new ArgumentException($"Unknown registry hive '{segments[0]}'. Expected HKLM, HKCU, HKCR, HKU, HKCC, HKPD or a full hive name.", nameof(path));

        var subKey = segments.Length == 2 ? segments[1].Trim('\\') : string.Empty;
        return subKey.Length == 0 ? hive : $@"{hive}\{subKey}";
    }

    private static void SetLastKey(string normalizedPath)
    {
        using var regeditKey = Registry.CurrentUser.CreateSubKey(RegeditKeyPath);
        if (regeditKey is null)
            throw new InvalidOperationException($@"Could not create or open HKEY_CURRENT_USER\{RegeditKeyPath}.");

        regeditKey.SetValue(LastKeyValueName, normalizedPath, RegistryValueKind.String);
    }
}
