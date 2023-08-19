using System.Diagnostics;

namespace WorkWithAndroid;

public partial class MainPage : ContentPage
{
    // int count = 0;

    public MainPage()
    {
        InitializeComponent();
    }

    // private v/*oid OnCounterClicked(object sender, EventArgs e)
    // {
    //     count++;
    //
    //     if (count == 1)
    //         CounterBtn.Text = $"Clicked {count} time";
    //     else
    //         CounterBtn.Text = $"Clicked {count} times";
    //
    //     SemanticScreenReader.Announce(CounterBtn.Text);
    // }*/

    private async void OnKillProcess(object sender, EventArgs e)
    {
        ProcessStartInfo startInfo = new()
        {
            FileName = "adb",
            Arguments = "shell am kill-all",
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };
        Process proc = Process.Start(startInfo);
        ArgumentNullException.ThrowIfNull(proc);
        string output = await proc.StandardOutput.ReadToEndAsync();
        await proc.WaitForExitAsync();
    }
    private async void OnGetIpProcess(object sender, EventArgs e)
    {
        ProcessStartInfo startInfo = new()
        {
            FileName = "adb",
            Arguments = "shell am start -n com.android.chrome/com.google.android.apps.chrome.Main",
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };
        Process proc = Process.Start(startInfo);
        ArgumentNullException.ThrowIfNull(proc);
        // string output = await proc.StandardOutput.ReadToEndAsync();
        await proc.WaitForExitAsync();
    }
}