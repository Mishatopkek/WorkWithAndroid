using System.Diagnostics;
using System.Xml.Linq;
using System.Xml.XPath;

namespace WorkWithAndroid.Helper;

public static class AdbCommands
{
    public static async Task<string> ExecuteShellCommand(string command)
    {
        ProcessStartInfo startInfo = new()
        {
            FileName = "adb",
            Arguments = command,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };
        Process? proc = Process.Start(startInfo);
        ArgumentNullException.ThrowIfNull(proc);

        string output = await proc.StandardOutput.ReadToEndAsync();
        output = output.Replace("UI hierchary dumped to: /sdcard/window_dump.xml\r\n", ""); //Allows to get only XML

        await proc.WaitForExitAsync();

        return output;
    }

    public static Task<string> GetDump() =>
        ExecuteShellCommand("shell \"uiautomator dump && cat /sdcard/window_dump.xml\"");

    public static Task OpenRecentApps() => ExecuteShellCommand("shell input keyevent KEYCODE_APP_SWITCH");
    public static Task SelectScreen() => ExecuteShellCommand("shell input keyevent DPAD_DOWN");
    public static Task KillSnapshot() => ExecuteShellCommand("shell input keyevent DEL");
    public static Task PressEnter() => ExecuteShellCommand("shell input keyevent KEYCODE_ENTER");

    public static Task RunChrome() =>
        ExecuteShellCommand("shell am start -n com.android.chrome/com.google.android.apps.chrome.Main");

    public static Task TypeInInput(string text)
    {
        text = text.Replace(" ", "%s");
        return ExecuteShellCommand($"shell input text {text}");
    }
}