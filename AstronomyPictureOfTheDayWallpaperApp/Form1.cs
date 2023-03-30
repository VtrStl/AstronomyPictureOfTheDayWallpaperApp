using System.Net;

namespace AstronomyPictureOfTheDayWallpaperApp
{
    public partial class Form1 : Form
    {
        private WallpaperAPODloader wpAPODloader;
        private WallpaperAPODruntime wallpaperAPODruntime;
        private bool configExists;
        public Form1()
        {
            InitializeComponent();
            wpAPODloader = new WallpaperAPODloader(this);
            wallpaperAPODruntime = new WallpaperAPODruntime(wpAPODloader, this);            
            configExists =  WallpaperAPODruntime.ConfigExists();
            UpdateTrayIcon(configExists);
        }
        // Set up the notification icon and start the wallpaper APOD manager if a configuration file exists.
        private async void Form1_Load(object sender, EventArgs e)
        {
            NotificationIcon.BalloonTipTitle = "APOD Wallpaper Manager";
            NotificationIcon.Visible = true;
            if (configExists)
            {
                Visible = false;
                ShowInTaskbar = false;
                UpdateStatusLabel(true);
                await wallpaperAPODruntime.StartTimers();
            }            
        }
        // This method restores the window state and sets the form as top-level when the notification icon is double-clicked. Commend: "Restores window state and sets as top-level on notification icon double-click."
        private void NotificationIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            WindowState = FormWindowState.Normal;
            if (WindowState == FormWindowState.Normal)
            {
                ShowInTaskbar = true;
                TopLevel = true;
            }
        }
        // Hide form from taskbar when minimized if mouse not on taskbar.
        public void Form1_SizeChanged(object sender, EventArgs e)
        {
            bool MousePointerNotOnTaskBar = Screen.GetWorkingArea(this).Contains(Cursor.Position);
            if (WindowState == FormWindowState.Minimized && MousePointerNotOnTaskBar)
            {
                ShowInTaskbar = false;
                TopLevel = false;
            }
        }
        // Updates the status label based on the isActive parameter.
        private void UpdateStatusLabel(bool isActive)
        {
            StatusLabel.Text = isActive ? "The application is active" : "The application is not active";
            StatusLabel.ForeColor = isActive ? Color.Green : Color.Red;
        }
        // Updates the tray icon based on the isActive parameter.
        private void UpdateTrayIcon(bool isActive)
        {
            string iconFolder = Path.Combine(Application.StartupPath,"..", "..", "..", "Icons"); // Need change path before build to single exe
            string iconName = isActive ? "APODiconGreen.ico" : "APODicon.ico";
            NotificationIcon.Icon = new Icon(Path.Combine(iconFolder, iconName));
        }

        public void ShowBaloonTipRetry()
        {
            NotificationIcon.ShowBalloonTip(10000, "Warning", "it seems there is no connection to internet, I will retry after 15 minutes", ToolTipIcon.Warning);
        }

        public void ShowBaloonTipVideo()
        {
            NotificationIcon.ShowBalloonTip(10000, "Information", "Today APOD is video format, Wallpaper will not change today and checker timer stopped", ToolTipIcon.Info);
        }
        
        public void ShowErrorMessageBox(Exception ex)
        {
            MessageBox.Show("Unhandled error accure: " + ex.Message + $" \nRestart the app and check {Application.LocalUserAppDataPath} folder for tracetrack. Please restart the app",
                    "error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        // Activate the app and even if media type is video, it will create configFile and lnk shortcun, but after when its done, its needed restart the app 
        private async void ActivateBT_Click(object sender, EventArgs e)
        {
            try
            {
                await wpAPODloader.LoadPicture();
                StatusLabel.Text = "Restart this app for the changes to take effect";
                StatusLabel.ForeColor = Color.Black;
                MessageBox.Show("Activation was successful, please restart the app for proper function");
            }
            catch (Exception ex) 
            { 
                if (ex is WebException webEx)
                {
                    if (webEx.Status == WebExceptionStatus.NameResolutionFailure)
                    {
                        MessageBox.Show("There is no internet connection", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }
                WallpaperAPODloader.CreateExceptionLog(ex);
                MessageBox.Show("Unhandled error accure: " + ex.Message + " \nRestart the app and check your applocal folder for tracetrack",
                    "error", MessageBoxButtons.OK, MessageBoxIcon.Error); 
            }
        }
        // Deactivate the app and remove all traces from app local folder and remove startup lnk file.
        private async void DeactivateBT_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Do you really want to deactivate this app and all processes and clear the cache?", "Warning",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                WallpaperAPODloader.ClearCache();
                UpdateStatusLabel(false);
                UpdateTrayIcon(false);
                await Task.Run(wallpaperAPODruntime.StopTimers);
            }
        }
    }
}