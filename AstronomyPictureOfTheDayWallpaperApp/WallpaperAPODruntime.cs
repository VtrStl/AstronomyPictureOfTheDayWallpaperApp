using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AstronomyPictureOfTheDayWallpaperApp
{
    public class WallpaperAPODruntime
    {
        WallpaperAPODloader wpAPODloader = new();
        public static bool ConfigExists()
        {
            if (File.Exists(WallpaperAPODloader.configPath))
            {
                return true;
            }
            return false;
        }     
    }
}