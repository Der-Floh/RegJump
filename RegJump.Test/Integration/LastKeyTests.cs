using System.Runtime.Versioning;

using Microsoft.Win32;

namespace RegJumpTest.Integration;

/// <summary>
/// These really write the registry and really start regedit, because that pair is the entire
/// mechanism and a mock of it would assert nothing. Elevated launches are never exercised: a UAC
/// prompt cannot be answered unattended.
/// </summary>
[Collection<RegistryCollection>]
public sealed class LastKeyTests
{
    private const string Sentinel = @"HKEY_CURRENT_USER\Environment";

    [Fact(Skip = "Writes the Windows registry.", SkipUnless = nameof(Platform.IsWindows), SkipType = typeof(Platform))]
    [SupportedOSPlatform("windows")]
    public void Open_WritesTheNormalizedPathToLastKey()
    {
        using var backup = new LastKeyBackup();

        using var regedit = new SpawnedProcess(RegJump.Open(@"Computer\hkcu/Control Panel\Cursors\"));

        Assert.Equal(@"HKEY_CURRENT_USER\Control Panel\Cursors", LastKeyBackup.Read());
        Assert.Equal(RegistryValueKind.String, LastKeyBackup.ReadKind());
    }

    [Fact(Skip = "Writes the Windows registry.", SkipUnless = nameof(Platform.IsWindows), SkipType = typeof(Platform))]
    [SupportedOSPlatform("windows")]
    public void Open_LeavesLastKeyUntouchedWhenThePathIsRejected()
    {
        using var backup = new LastKeyBackup();
        LastKeyBackup.Write(Sentinel);

        Assert.Throws<ArgumentException>("path", () => RegJump.Open(@"HKXX\Nope"));

        Assert.Equal(Sentinel, LastKeyBackup.Read());
    }

    [Fact(Skip = "Starts regedit.exe.", SkipUnless = nameof(Platform.IsWindows), SkipType = typeof(Platform))]
    [SupportedOSPlatform("windows")]
    public void Open_WithoutAPathLeavesLastKeyUntouched()
    {
        using var backup = new LastKeyBackup();
        LastKeyBackup.Write(Sentinel);

        using var regedit = new SpawnedProcess(RegJump.Open());

        Assert.Equal(Sentinel, LastKeyBackup.Read());
    }
}
