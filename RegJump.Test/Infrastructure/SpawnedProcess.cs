using System.ComponentModel;
using System.Diagnostics;

namespace RegJumpTest.Infrastructure;

/// <summary>
/// Owns a process a test started and kills it on disposal. Only ever the handle RegJump returned,
/// never a process looked up by name, which would take down a Registry Editor the developer opened.
/// </summary>
internal sealed class SpawnedProcess(Process process) : IDisposable
{
    private const int ExitTimeoutMilliseconds = 10_000;

    public Process Process { get; } = process;

    public void Dispose()
    {
        try
        {
            if (!Process.HasExited)
                Process.Kill();

            Process.WaitForExit(ExitTimeoutMilliseconds);
        }
        catch (InvalidOperationException)
        {
        }
        catch (Win32Exception)
        {
        }
        finally
        {
            Process.Dispose();
        }
    }
}
