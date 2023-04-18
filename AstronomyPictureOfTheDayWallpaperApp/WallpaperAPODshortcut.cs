using IWshRuntimeLibrary;

namespace AstronomyPictureOfTheDayWallpaperApp
{
    public static class WallpaperAPODshortcut
    {
        public static void CreateShortcut()
        {
            string startupPath = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
            string shortcutPath = Path.Combine(startupPath, "WallpaperAPOD.lnk");
            if (!System.IO.File.Exists(shortcutPath))
            {
                WshShellClass shell = new();
                IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutPath);
                shortcut.TargetPath = Application.ExecutablePath;
                shortcut.Save();
            }            
        }

        public static void DeleteShortcut()
        {
            string startupPath = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
            string shortcutPath = Path.Combine(startupPath, "WallpaperAPOD.lnk");
            if (System.IO.File.Exists(shortcutPath))
            {
                System.IO.File.Delete(shortcutPath);
            }
        }
    }
}
