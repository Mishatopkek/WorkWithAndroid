using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using WorkWithAndroid.Services;

namespace WorkWithAndroid;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }

    private async void OnKillProcess(object sender, EventArgs e)
    {
        KillBtn.Text = "Working";
        SemanticScreenReader.Announce(KillBtn.Text);

        await AdbService.KillAllBackgroundApplications();

        KillBtn.Text = "Kill 'em";
        SemanticScreenReader.Announce(KillBtn.Text);
    }

    private async void OnSearchQuery(object sender, EventArgs e)
    {
        const string ipQuery = "my ip address";
        GetIpBtn.Text = "Working";
        SemanticScreenReader.Announce(GetIpBtn.Text);

        await AdbService.InsertInChromeInput(SearchQuery.Text);

        if (SearchQuery.Text.Trim().ToLower() == ipQuery)
        {
            string? ip = await AdbService.GetAnIpFromChrome();
            IpLabel.IsVisible = true;
            IpLabel.Text = string.IsNullOrEmpty(ip) ?
                "IP was not found" :
                "Your IP address: " + ip;
            SemanticScreenReader.Announce(IpLabel.Text);
        }
        GetIpBtn.Text = "Get it";
        SemanticScreenReader.Announce(GetIpBtn.Text);
    }
}