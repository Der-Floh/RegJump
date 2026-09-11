namespace RegJumpTest.Infrastructure;

/// <summary>
/// Spelled out independently of the library's own constants, so a test can catch the library
/// writing to the wrong key rather than agreeing with it.
/// </summary>
internal static class RegistryPaths
{
    public const string RegeditKey = @"Software\Microsoft\Windows\CurrentVersion\Applets\Regedit";

    public const string LastKeyValue = "LastKey";
}
