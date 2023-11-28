using System.Net;

namespace AstronomyPictureOfTheDayWallpaperApp
{
    public class WallpaperAPODmanager : IDisposable
    {
        private WallpaperAPODloader wpAPODloader;
        private WallpaperAPODruntime wallpaperAPODruntime;
        private MainForm? form;
        private bool configExists;
        private NotifyIcon notificationIcon;
        private bool disposedValue;

        // Constructor that creates most objects and here is where the app starts
        public WallpaperAPODmanager(NotifyIcon notificationIcon)
        {
            // Load all classes and items
            wpAPODloader = new WallpaperAPODloader(this);
            wallpaperAPODruntime = new WallpaperAPODruntime(wpAPODloader, this);
            configExists = ConfigExists();
            this.notificationIcon = notificationIcon;
            // Mouse EventArgs and set Tray Icon by status
            notificationIcon.MouseDoubleClick += NotificationIcon_MouseDoubleClick;
            UpdateTrayIcon(configExists);
        }

        // Checks if config file exists in AppLocal
        public static bool ConfigExists()
        {
            return File.Exists(WallpaperAPODloader.configPath);
        }

        // Verifies if the API key is correct and lets the user know if the API key is valid
        public async Task<HttpStatusCode> CheckAPIStatus(string apikey)
        {
            using (HttpClient client = new())
            {
                HttpResponseMessage response = await client.GetAsync("https://api.nasa.gov/planetary/apod?api_key=" + apikey);
                HttpStatusCode statusCode = response.StatusCode;
                switch (statusCode)
                {
                    case HttpStatusCode.OK:
                        MessageBox.Show($"This API key is valid and server returns: {statusCode}", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        break;

                    case HttpStatusCode.Forbidden:
                        MessageBox.Show($"This API key is not valid and server returns: {statusCode}", "Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        break;

                    case var code when (int)code >= 500 && (int)code <= 504:
                        MessageBox.Show($"This API is maybe valid, but there is a problem on the server side and server returns: {statusCode}",
                            "Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        break;

                    default:
                        MessageBox.Show($"Unhandled status code: {statusCode}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        break;
                }
                return statusCode;
            }
        }

        // Writes the API key from the textbox from MainForm to apikey.txt or creates new textfile with an API key
        public void SetupAPIKey(string apikey)
        {
            string apikeypath = Path.Combine(Application.StartupPath, "..", "..", "..", "apikey.txt"); // Change path before release
            using (StreamWriter sw = new(apikeypath))
            {
                sw.WriteAsync(apikey);
                sw.Flush();
            }
        }

        // This event reacts to double-clicking on the icon, and checking if the form is initialised or not, if not, it creates a new one.
        private void NotificationIcon_MouseDoubleClick(object? sender, MouseEventArgs e)
        {
            try
            {
                if (form is null || form.IsDisposed)
                {
                    form = new MainForm(this, ConfigExists());
                }
                else
                {
                    form.WindowState = FormWindowState.Normal;
                }
                form.ShowInTaskbar = true;
                form.Show();
                form.Focus();
            }
            catch (Exception ex)
            {
                ShowBaloonTipError(ex);
                WallpaperAPODloader.CreateExceptionLog(ex);
                ShowErrorMessageBox(ex);
            }
        }

        // Starts the timers in WallpaperAPODruntime class
        public async Task Start()
        {            
            if (configExists)
            {
                await wallpaperAPODruntime.StartTimers();
            }
        }
        
        // Updates the status label based on the isActive parameter
        public void UpdateTrayIcon(bool isActive)
        {
            string iconPathDev = Path.Combine(Application.StartupPath, "..", "..", "..", "Icons");
            string iconPathSwitcher = Directory.Exists(iconPathDev) ? iconPathDev : Path.Combine(Application.StartupPath, "Icons");
            string iconName = isActive ? "APODiconGreen.ico" : "APODicon.ico";
            notificationIcon.Icon = new Icon(Path.Combine(iconPathSwitcher, iconName));
        }

        public void ShowBaloonTipWait()
        {
            notificationIcon.ShowBalloonTip(10000, "Please wait", "The download and desktop setup is in progress until the Message Box appears. It depends on image size and internet speed.", ToolTipIcon.None);
        }

        public void ShowBaloonTipRetry()
        {
            notificationIcon.ShowBalloonTip(10000, "Warning", "it seems there is no connection to internet or apod site is not avaible, I will retry after 10 minutes", ToolTipIcon.Warning);
        }

        public void ShowBaloonTipError(Exception ex)
        {
            notificationIcon.ShowBalloonTip(10000, "Error", $"Application has an exception : '{ex.Message}.' Restart the app for proper function", ToolTipIcon.Error);
        }

        public void ShowBaloonTipVideo()
        {
            notificationIcon.ShowBalloonTip(10000, "Information", "Today APOD is video format, Wallpaper will not change today and checker timer stopped", ToolTipIcon.None);
        }

        public static void ShowErrorMessageBox(Exception ex)
        {
            MessageBox.Show("Unhandled error occurred: " + ex.Message + $" \nRestart the app and check {Application.LocalUserAppDataPath} folder for tracetrack. Please restart the app",
            "error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        
        // Dispose method with optional disposing parameter.
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    notificationIcon.MouseDoubleClick -= NotificationIcon_MouseDoubleClick;
                }
                wpAPODloader?.Dispose();
                disposedValue = true;
            }
        }
        // Public Dispose method that calls the protected Dispose method with disposing set to true, and suppresses finalization.
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}