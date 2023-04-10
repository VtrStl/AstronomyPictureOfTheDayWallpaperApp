namespace AstronomyPictureOfTheDayWallpaperApp
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        private static WallpaperAPODmanager? wallpaperAPODmanager;
        private static NotifyIcon? notificationIcon;
        private static readonly string MutexName = "AstronomyPictureOfTheDayWallpaperApp";
        
        [STAThread]
        static void Main()
        {            
            bool createdNew;
            using (var mutex = new Mutex(true, MutexName, out createdNew))
            {                
                if (createdNew)
                {
                    Application.SetCompatibleTextRenderingDefault(false);
                    Application.EnableVisualStyles();                    
                    if (!WallpaperAPODmanager.ConfigExists())
                    {
                        ApplicationConfiguration.Initialize();
                        wallpaperAPODmanager = new WallpaperAPODmanager(SetNotificationIcon());
                        Application.Run(new MainForm(wallpaperAPODmanager, WallpaperAPODmanager.ConfigExists()));
                    }
                    else
                    {                        
                        wallpaperAPODmanager = new WallpaperAPODmanager(SetNotificationIcon());
                        wallpaperAPODmanager?.Start();
                        Application.Run();
                    }
                }
                else
                {
                    MessageBox.Show("Another instance of the application is already running", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }
        
        // Create and set Tray & Notification Icon
        private static NotifyIcon SetNotificationIcon()
        {
            notificationIcon = new NotifyIcon()
            {
                BalloonTipText = "APOD Wallpaper Manager",
                Visible = true,
            };            
            return notificationIcon;
        }
        
        private static void OnExit(object sender, EventArgs e)
        {
            notificationIcon?.Dispose();
            wallpaperAPODmanager?.Dispose();
            Application.Exit();
        }
    }
}