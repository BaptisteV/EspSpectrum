namespace AndroidMic;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

    }

    private async Task RequestMicrophonePermission()
    {
        var status = await Permissions.RequestAsync<Permissions.Microphone>();
        if (status != PermissionStatus.Granted)
        {
            await DisplayAlertAsync("Permission Denied", "Microphone permission is required", "OK");
        }
    }

    private async void Shell_Loaded(object sender, EventArgs e)
    {
        await RequestMicrophonePermission();
    }
}
