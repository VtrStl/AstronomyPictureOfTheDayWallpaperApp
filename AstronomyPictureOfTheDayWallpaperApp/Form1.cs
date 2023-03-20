using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace AstronomyPictureOfTheDayWallpaperApp
{
    public partial class Form1 : Form
    {
        private readonly WallpaperAPODloader wpAPODloader = new();
        protected readonly WallpaperAPODruntime wallpaperAPODruntime;
        private bool configExists = WallpaperAPODruntime.ConfigExists();
        public Form1()
        {
            InitializeComponent();
            wallpaperAPODruntime = new WallpaperAPODruntime(wpAPODloader);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            NotificationIcon.BalloonTipTitle = "APOD Wallpaper Manager";
            NotificationIcon.Visible = true;
            if (configExists)
            {
                Visible = false;
                ShowInTaskbar = false;
                UpdateStatusLabel(true);
                wallpaperAPODruntime.StartTimer();
            }
            UpdateTrayIcon(configExists);
        }

        private void NotificationIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            WindowState = FormWindowState.Normal;
            if (WindowState == FormWindowState.Normal)
            {
                ShowInTaskbar = true;
                TopLevel = true;
            }
        }

        public void Form1_SizeChanged(object sender, EventArgs e)
        {
            bool MousePointerNotOnTaskBar = Screen.GetWorkingArea(this).Contains(Cursor.Position);
            if (WindowState == FormWindowState.Minimized && MousePointerNotOnTaskBar)
            {
                ShowInTaskbar = false;
                TopLevel = false;
            }
        }

        private async void ActivateBT_Click(object sender, EventArgs e)
        {
            try
            {
                await Task.Run(wpAPODloader.LoadPicture);
                await Task.Run(wpAPODloader.SetWallpaper);
                MessageBox.Show(WallpaperAPODruntime.ConfigExists().ToString());
                UpdateStatusLabel(true);
                UpdateTrayIcon(true);
            }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
        }

        private void DeactivateBT_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Do you really want to deactivate this app and all processes and clear the cache?", "Warning",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                WallpaperAPODloader.ClearCache();
                UpdateStatusLabel(false);
                UpdateTrayIcon(false);
            }
        }

        private void UpdateStatusLabel(bool isActive)
        {
            StatusLabel.Text = isActive ? "The application is active" : "The application is not active";
            StatusLabel.ForeColor = isActive ? Color.Green : Color.Red;
        }

        private void UpdateTrayIcon(bool configExists)
        {
            string iconFolder = Path.Combine(Application.StartupPath, "..", "..", "..", "Icons");
            string iconName = configExists ? "APODiconGreen.ico" : "APODicon.ico";
            NotificationIcon.Icon = new Icon(Path.Combine(iconFolder, iconName));
        }

        private async void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            wallpaperAPODruntime.StopTimer();
        }
    }
}