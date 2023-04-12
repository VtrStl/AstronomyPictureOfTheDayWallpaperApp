using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

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
                WallpaperAPODloader.CreateExceptionLog(ex);
                ShowErrorMessageBox(ex);
            }
        }

        public static bool ConfigExists()
        {
            return File.Exists(WallpaperAPODloader.configPath);
        }

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
            string iconFolder = Path.Combine(Application.StartupPath, "..", "..", "..", "Icons"); // Need change path before release
            string iconName = isActive ? "APODiconGreen.ico" : "APODicon.ico";
            notificationIcon.Icon = new Icon(Path.Combine(iconFolder, iconName));
        }

        public void ShowBaloonTipWait()
        {
            notificationIcon.ShowBalloonTip(10000, "Please wait", "The download and desktop setup is in progress until the Message Box appears. It depends on image size and internet speed.", ToolTipIcon.None);
        }

        public void ShowBaloonTipRetry()
        {
            notificationIcon.ShowBalloonTip(10000, "Warning", "it seems there is no connection to internet, I will retry after 10 minutes", ToolTipIcon.Warning);
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