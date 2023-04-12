using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Sockets;

namespace AstronomyPictureOfTheDayWallpaperApp
{
    public partial class MainForm : Form, IDisposable
    {
        private WallpaperAPODruntime? wpAPODruntime;
        private WallpaperAPODmanager wpAPODmanager;
        WallpaperAPODloader? wpAPODloader;
        private bool configExists;
        public MainForm(WallpaperAPODmanager wpAPODmanager, bool configExists)
        {
            InitializeComponent();
            this.wpAPODmanager = wpAPODmanager;
            this.configExists = configExists;
        }
        
        // Set up the notification icon and start the wallpaper APOD manager if a configuration file exists.
        private void Form1_Load(object sender, EventArgs e)
        {
            if (configExists)
            {
                UpdateStatusLabel(true);
                wpAPODmanager.UpdateTrayIcon(true);
            }
        }
        
        // Hide form from taskbar when minimized if mouse not on taskbar
        public void Form1_SizeChanged(object sender, EventArgs e)
        {
            bool MousePointerNotOnTaskBar = Screen.GetWorkingArea(this).Contains(Cursor.Position);
            if (WindowState == FormWindowState.Minimized && MousePointerNotOnTaskBar)
            {
                ShowInTaskbar = false;
                Hide();
            }            
        }
        
        // Before the closing, it will ask user if he really want close the app
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (configExists)
            {
                if (MessageBox.Show("Are you sure you want to close the application and all instances?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    e.Cancel = true;
                    return;
                }                
            }
            Application.ExitThread();
        }
        
        // Updates the status label based on the isActive parameter
        private void UpdateStatusLabel(bool isActive)
        {
            StatusLabel.Text = isActive ? "The application is active" : "The application is not active";
            StatusLabel.ForeColor = isActive ? Color.Green : Color.Red;
        }

        // Activate the app and even if media type is video, it will create configFile and lnk shortcun, but after when its done, its needed restart the app 
        private async void ActivateBT_Click(object sender, EventArgs e)
        {
            try
            {
                wpAPODmanager.ShowBaloonTipWait();
                wpAPODloader = new(wpAPODmanager);
                await wpAPODloader.LoadPicture();
                StatusLabel.Text = "Restart this app for the changes to take effect.";
                StatusLabel.ForeColor = Color.Black;
                MessageBox.Show("Activation was successful, please restart the app for proper function.");
            }
            catch (HttpRequestException httpEx) when (httpEx.InnerException is SocketException) // If there is no connection
            {
                MessageBox.Show("There is no internet connection.\nTry again until you have a stable connection for activation.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (HttpRequestException httpEx) when (httpEx.Message.Contains("Forbidden")) // If API is not valid
            {
                MessageBox.Show("Invalid API key probably.\nCheck you apikey.txt and set the valid API key.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                WallpaperAPODloader.CreateExceptionLog(ex);
                MessageBox.Show("Unhandled error accure: " + ex.Message + " \nRestart the app and check your applocal folder for tracetrack.",
                    "error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        // Deactivate the app and remove all traces from app local folder and remove startup lnk file
        private void DeactivateBT_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Do you really want to deactivate this app and all processes and clear the cache?", "Warning",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                WallpaperAPODloader.ClearCache();
                UpdateStatusLabel(false);
                wpAPODmanager.UpdateTrayIcon(false);
                wpAPODruntime = new(wpAPODloader, wpAPODmanager);
                wpAPODruntime.StopTimers();
            }
        }
    }
}