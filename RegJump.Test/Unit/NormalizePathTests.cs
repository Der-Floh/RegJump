namespace RegJumpTest.Unit;

public sealed class NormalizePathTests
{
    [Theory]
    [InlineData("HKCR", "HKEY_CLASSES_ROOT")]
    [InlineData("HKCU", "HKEY_CURRENT_USER")]
    [InlineData("HKLM", "HKEY_LOCAL_MACHINE")]
    [InlineData("HKU", "HKEY_USERS")]
    [InlineData("HKCC", "HKEY_CURRENT_CONFIG")]
    [InlineData("HKPD", "HKEY_PERFORMANCE_DATA")]
    [InlineData("HKEY_LOCAL_MACHINE", "HKEY_LOCAL_MACHINE")]
    [InlineData("hkcu", "HKEY_CURRENT_USER")]
    [InlineData("hKeY_cUrReNt_UsEr", "HKEY_CURRENT_USER")]
    public void NormalizePath_ExpandsHivesCaseInsensitively(string path, string expected)
        => Assert.Equal(expected, RegJump.NormalizePath(path));

    [Theory]
    [InlineData(@"HKCU\Software", @"HKEY_CURRENT_USER\Software")]
    [InlineData(@"HKLM\SYSTEM\CurrentControlSet\Services", @"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services")]
    [InlineData(@"HKCU\Control Panel\Cursors", @"HKEY_CURRENT_USER\Control Panel\Cursors")]
    [InlineData(@"hklm\software", @"HKEY_LOCAL_MACHINE\software")]
    public void NormalizePath_LeavesTheSubKeyAlone(string path, string expected)
        => Assert.Equal(expected, RegJump.NormalizePath(path));

    [Theory]
    [InlineData("HKCU/Software/Microsoft", @"HKEY_CURRENT_USER\Software\Microsoft")]
    [InlineData(@"HKCU/Software\Microsoft", @"HKEY_CURRENT_USER\Software\Microsoft")]
    [InlineData("\"HKCU\\Software\"", @"HKEY_CURRENT_USER\Software")]
    [InlineData(@"   HKCU\Software   ", @"HKEY_CURRENT_USER\Software")]
    [InlineData(@"HKCU\Software\", @"HKEY_CURRENT_USER\Software")]
    [InlineData(@"\HKCU\Software", @"HKEY_CURRENT_USER\Software")]
    [InlineData("HKCU/Software/", @"HKEY_CURRENT_USER\Software")]
    [InlineData(@"HKCU\", "HKEY_CURRENT_USER")]
    [InlineData(@"HKCU\\Software", @"HKEY_CURRENT_USER\Software")]
    public void NormalizePath_CanonicalizesSeparatorsAndTrimsEdges(string path, string expected)
        => Assert.Equal(expected, RegJump.NormalizePath(path));

    [Theory]
    [InlineData(@"Computer\HKCU\Software", @"HKEY_CURRENT_USER\Software")]
    [InlineData(@"Computer\HKEY_CURRENT_USER", "HKEY_CURRENT_USER")]
    [InlineData(@"computer\hkcu\Software", @"HKEY_CURRENT_USER\Software")]
    [InlineData(@"Computer\HKEY_CLASSES_ROOT\.txt", @"HKEY_CLASSES_ROOT\.txt")]
    public void NormalizePath_StripsTheAddressBarPrefix(string path, string expected)
        => Assert.Equal(expected, RegJump.NormalizePath(path));

    [Theory]
    [InlineData(@"HKCU\Software\Foo*Bar?Baz|Qux", @"HKEY_CURRENT_USER\Software\Foo*Bar?Baz|Qux")]
    [InlineData(@"HKCU\Software\\Microsoft", @"HKEY_CURRENT_USER\Software\\Microsoft")]
    public void NormalizePath_PassesThroughCharactersLegalInKeyNames(string path, string expected)
        => Assert.Equal(expected, RegJump.NormalizePath(path));

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t\r\n")]
    [InlineData(@"\")]
    [InlineData(@"\\")]
    [InlineData("/")]
    [InlineData("///")]
    [InlineData("\"\"")]
    public void NormalizePath_RejectsAnEmptyPath(string path)
    {
        var exception = Assert.Throws<ArgumentException>("path", () => RegJump.NormalizePath(path));

        Assert.StartsWith("Registry path must not be empty.", exception.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("Computer", "Computer")]
    [InlineData(@"Computer\", "Computer")]
    [InlineData(@"Computer\Computer\HKCU", "Computer")]
    [InlineData(@"HKXX\Software", "HKXX")]
    [InlineData("HKEY_DYN_DATA", "HKEY_DYN_DATA")]
    [InlineData(@"HKEY_CURRENT_USER_X\Foo", "HKEY_CURRENT_USER_X")]
    [InlineData(@"C:\Windows", "C:")]
    [InlineData(@"HKCU \Software", "HKCU ")]
    [InlineData("  \" HKCU\\Foo \"  ", " HKCU")]
    [InlineData(@"Software\Microsoft", "Software")]
    public void NormalizePath_RejectsAnUnknownHive(string path, string reportedHive)
    {
        var exception = Assert.Throws<ArgumentException>("path", () => RegJump.NormalizePath(path));

        Assert.StartsWith($"Unknown registry hive '{reportedHive}'.", exception.Message, StringComparison.Ordinal);
    }
}
