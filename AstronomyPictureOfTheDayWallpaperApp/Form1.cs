using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace AstronomyPictureOfTheDayWallpaperApp
{
    public partial class Form1 : Form
    {
        protected readonly WallpaperAPODloader wallpaperAPODloader = new();
        protected readonly WallpaperAPODruntime wallpaperAPODruntime = new();

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            NotificationIcon.BalloonTipTitle = "APOD Wallpaper Manager";
            NotificationIcon.Visible = true;
            if (WallpaperAPODruntime.ConfigExists() == true)
            {
                Visible = false;
                ShowInTaskbar = false;
            }
        }

        private void NotificationIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            WindowState = FormWindowState.Normal;
            if (WindowState == FormWindowState.Normal)
            {
                ShowInTaskbar = true;
            }
        }

        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            bool MousePointerNotOnTaskBar = Screen.GetWorkingArea(this).Contains(Cursor.Position);
            if (WindowState == FormWindowState.Minimized && MousePointerNotOnTaskBar)
            {
                ShowInTaskbar = false;
            }
        }

        private async void ActivateBT_Click(object sender, EventArgs e)
        {
            try
            {
                await Task.Run((wallpaperAPODloader.LoadPicture));
                await Task.Run((wallpaperAPODloader.SetWallpaper));
                MessageBox.Show(WallpaperAPODruntime.ConfigExists().ToString());
            }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
        }

        private void DeactivateBT_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Do you really want to deactivate this app and all processes and clear the cache?", "Warning",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                WallpaperAPODloader.ClearCache();
            }
        }
    }
}