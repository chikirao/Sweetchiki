using System.Diagnostics;
using Sweetch.Core;
namespace Sweetch.App.Shell;
/// <summary>No update server is configured for Sweetch 2.0. No background traffic or installers.</summary>
internal sealed class Updater
{
    public static Version Current => new(2, 0, 0);
    public static bool IsInstalled => false;
    public UpdateInfo? Available => null;
    public DateTime? LastChecked => null;
    public string? LastError => null;
    public bool Installing => false;
    public event Action? Changed { add { } remove { } }
    public Task<UpdateInfo?> CheckAsync() => Task.FromResult<UpdateInfo?>(null);
    public void StartBackgroundChecks(AppSettings settings, Action<UpdateInfo> found) { }
    public Task InstallAsync(UpdateInfo update) => Task.CompletedTask;
    public static void Open(string url) => Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
}
