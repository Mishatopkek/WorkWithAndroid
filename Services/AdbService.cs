using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml.XPath;
using WorkWithAndroid.Helper;

namespace WorkWithAndroid.Services;

public static class AdbService
{
    public static async Task KillAllBackgroundApplications()
    {
        await AdbCommands.OpenRecentApps();

        await AdbCommands.SelectScreen();
        while (true)
        {
            if (await RemoveRecent())
                continue;
            break;
        }
    }

    public static async Task InsertInChromeInput(string query)
    {
        string dumpValue = await AdbCommands.GetDump();
        bool isChromeLaunchable = dumpValue.Contains("id/launcher") && dumpValue.Contains("text=\"Chrome\"");

        if (!isChromeLaunchable) return;
        
        await AdbCommands.RunChrome();

        await AcceptTerms();

        await ClickToChromeElement("search_box_text");
        await AdbCommands.TypeInInput(query);
        await AdbCommands.PressEnter();
    }

    public static async Task<string?> GetAnIpFromChrome()
    {
        await AdbCommands.GetDump();                    //I don't know how
        string dumpValue = await AdbCommands.GetDump(); //but it works if I call it twice
        List<string> ips = GetIps(dumpValue);
        IEnumerable<string> uniqueIps = GetUniqueIps(ips);
        XElement xml = await GetParsedDump(dumpValue);
        return uniqueIps.FirstOrDefault(ip => TryGetMyIp(xml, ip));
    }

    private static async Task<bool> RemoveRecent()
    {
        string output = await AdbCommands.GetDump();
        if (!output.Contains("id/snapshot")) return false;

        await AdbCommands.KillSnapshot();
        return true;
    }

    private static async Task AcceptTerms()
    {
        string dumpValue = await AdbCommands.GetDump();
        if (dumpValue.Contains("id/terms_accept"))
        {
            await ClickToChromeElement("terms_accept", dumpValue);
            await ClickToChromeElement("negative_button");
        }
    }

    private static async Task ClickToChromeElement(string elementId, string? dump = null)
    {
        XElement xml = await GetParsedDump(dump);
        XElement? selectedElement = xml.XPathSelectElement($"//*[@resource-id='com.android.chrome:id/{elementId}']");

        if (selectedElement != null)
        {
            await TapInput(selectedElement);
        }
    }

    private static bool TryGetMyIp(XNode xml, string ip)
    {
        XElement? selectedIpElement = xml.XPathSelectElement($"//*[@text='{ip}']");
        List<XElement>? ipParent = selectedIpElement?.Parent?.Elements().ToList();
        if (ipParent is not {Count: 4}) return false;
        bool areThereThreeTextLabel = ipParent.Take(3).All(x => x.Attribute("class")?.Value == "android.view.View");
        bool isThereAButton = ipParent[^1].Attribute("class")?.Value == "android.widget.Button";
        return areThereThreeTextLabel && isThereAButton;
    }

    private static IEnumerable<string> GetUniqueIps(List<string> ips)
    {
        Dictionary<string, int> frequencyMap = new();

        foreach (string element in ips)
        {
            if (frequencyMap.ContainsKey(element))
            {
                frequencyMap[element]++;
            }
            else
            {
                frequencyMap[element] = 1;
            }
        }

        List<string> result = ips.Where(element => frequencyMap[element] == 1).ToList();
        return result;
    }

    private static List<string> GetIps(string dumpValue)
    {
        const string ipAddressPattern = @"\b(?:\d{1,3}\.){3}\d{1,3}\b";

        MatchCollection matches = Regex.Matches(dumpValue, ipAddressPattern);

        List<string> ips = matches.Select(x => x.Value).ToList();
        return ips;
    }

    private static async Task<XElement> GetParsedDump(string? dump = null)
    {
        dump ??= await AdbCommands.GetDump();
        return XElement.Parse(dump);
    }

    private static async Task TapInput(XElement selectedElement)
    {
        XAttribute? coordinatesAttribute = selectedElement.Attribute("bounds");
        if (selectedElement.Attribute("bounds") == null)
        {
            throw new ArgumentNullException(nameof(selectedElement), "text input was not found");
        }

        int[] coordinates = coordinatesAttribute!.Value
            .Split(new[] {'[', ']', ','}, StringSplitOptions.RemoveEmptyEntries)
            .Select(int.Parse)
            .ToArray();

        int avgX = (int) ((coordinates[0] + coordinates[2]) / 2.0);
        int avgY = (int) ((coordinates[1] + coordinates[3]) / 2.0);

        await AdbCommands.ExecuteShellCommand($"shell input tap {avgX} {avgY}");
    }
}