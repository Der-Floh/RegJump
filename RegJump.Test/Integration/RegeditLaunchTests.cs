using System.Runtime.Versioning;

namespace RegJumpTest.Integration;

/// <summary>
/// The assertion stays at the handle level on purpose: regedit is single-instance, so when one is
/// already open the newly started process hands off and exits immediately.
/// </summary>
[Collection<RegistryCollection>]
public sealed class RegeditLaunchTests
{
    [Fact(Skip = "Starts regedit.exe.", SkipUnless = nameof(Platform.IsWindows), SkipType = typeof(Platform))]
    [SupportedOSPlatform("windows")]
    public void Open_ReturnsAHandleToAStartedRegistryEditor()
    {
        using var backup = new LastKeyBackup();

        using var regedit = new SpawnedProcess(RegJump.Open(@"HKCU\Environment"));

        Assert.True(regedit.Process.Id > 0);
    }
}
