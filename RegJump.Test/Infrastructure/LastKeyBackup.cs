using System.Runtime.Versioning;

using Microsoft.Win32;

namespace RegJumpTest.Infrastructure;

/// <summary>
/// Captures <c>LastKey</c> on construction and restores it on disposal, so a test that really writes
/// the registry leaves the Registry Editor where it was even when the test fails part way through.
/// Only that one value is touched; the same key also holds window placement and favourites.
/// </summary>
[SupportedOSPlatform("windows")]
internal sealed class LastKeyBackup : IDisposable
{
    private readonly bool _keyExisted;
    private readonly object? _value;
    private readonly RegistryValueKind _kind;

    public LastKeyBackup()
    {
        using var key = Registry.CurrentUser.OpenSubKey(RegistryPaths.RegeditKey);
        if (key is null)
            return;

        _keyExisted = true;
        _value = key.GetValue(RegistryPaths.LastKeyValue);
        if (_value is not null)
            _kind = key.GetValueKind(RegistryPaths.LastKeyValue);
    }

    public static string? Read()
    {
        using var key = Registry.CurrentUser.OpenSubKey(RegistryPaths.RegeditKey);
        return key?.GetValue(RegistryPaths.LastKeyValue) as string;
    }

    public static RegistryValueKind ReadKind()
    {
        using var key = Registry.CurrentUser.OpenSubKey(RegistryPaths.RegeditKey);
        if (key?.GetValue(RegistryPaths.LastKeyValue) is null)
            return RegistryValueKind.Unknown;

        return key.GetValueKind(RegistryPaths.LastKeyValue);
    }

    public static void Write(string value)
    {
        using var key = Registry.CurrentUser.CreateSubKey(RegistryPaths.RegeditKey);
        key?.SetValue(RegistryPaths.LastKeyValue, value, RegistryValueKind.String);
    }

    public void Dispose()
    {
        if (!_keyExisted)
        {
            Registry.CurrentUser.DeleteSubKeyTree(RegistryPaths.RegeditKey, throwOnMissingSubKey: false);
            return;
        }

        using var key = Registry.CurrentUser.CreateSubKey(RegistryPaths.RegeditKey);
        if (key is null)
            return;

        if (_value is null)
            key.DeleteValue(RegistryPaths.LastKeyValue, throwOnMissingValue: false);
        else
            key.SetValue(RegistryPaths.LastKeyValue, _value, _kind);
    }
}
