namespace AstronomyPictureOfTheDayWallpaperApp
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        private static readonly string MutexName = "AstronomyPictureOfTheDayWallpaperApp";
        [STAThread]
        static void Main()
        {            
            bool createdNew;
            using (var mutex = new Mutex(true, MutexName, out createdNew))
            {
                if (createdNew)
                {
                    ApplicationConfiguration.Initialize();
                    Application.Run(new Form1());
                }
                else
                {
                    MessageBox.Show("Another instance of the application is already running", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }
    }
}