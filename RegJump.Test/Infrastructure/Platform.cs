namespace RegJumpTest.Infrastructure;

public static class Platform
{
    public static bool IsWindows => OperatingSystem.IsWindows();
}
