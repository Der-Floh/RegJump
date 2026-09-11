using System.Reflection;

namespace RegJumpTest.Unit;

/// <summary>
/// Open normalizes the whole path before it writes anything, so a rejected path never reaches the
/// registry and never starts a process. That ordering is what lets these run on any platform.
/// </summary>
public sealed class OpenArgumentTests
{
    [Fact]
    public void Open_RejectsANullPath()
        => Assert.Throws<ArgumentNullException>("path", () => RegJump.Open(null!));

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(@"\")]
    public void Open_RejectsAnEmptyPath(string path)
        => Assert.Throws<ArgumentException>("path", () => RegJump.Open(path));

    [Theory]
    [InlineData("Computer")]
    [InlineData(@"HKXX\Software")]
    [InlineData(@"C:\Windows")]
    public void Open_RejectsAnUnknownHive(string path)
        => Assert.Throws<ArgumentException>("path", () => RegJump.Open(path));

    [Fact]
    public void OpenAt_IsStillMarkedObsolete()
        => Assert.NotNull(typeof(RegJump).GetMethod("OpenAt")?.GetCustomAttribute<ObsoleteAttribute>());

    [Fact]
    public void OpenAt_ValidatesExactlyLikeOpen()
    {
#pragma warning disable CS0618
        var exception = Assert.Throws<ArgumentException>("path", () => RegJump.OpenAt("Computer"));
#pragma warning restore CS0618

        Assert.StartsWith("Unknown registry hive 'Computer'.", exception.Message, StringComparison.Ordinal);
    }
}
