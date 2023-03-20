using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceProcess;
using System.Timers;
using System.Threading.Tasks;

namespace AstronomyPictureOfTheDayWallpaperApp
{
    public class WallpaperAPODruntime
    {
        private WallpaperAPODloader wpAPODloader;
        private System.Timers.Timer _timer;
        
        public WallpaperAPODruntime(WallpaperAPODloader wallpaperAPODloader)
        {
            wpAPODloader = wallpaperAPODloader;
        }

        public static bool ConfigExists()
        {
            if (File.Exists(WallpaperAPODloader.configPath))
            {
                return true;                
            }
            return false;
        }

        public async Task StartTimer()
        {
            // Get the next 6:00 am in Washington time
            DateTime nextWashingtonSixAm = GetNextWashingtonSixAm();
            TimeSpan timeToWashingtonSixAm = nextWashingtonSixAm.Subtract(DateTime.UtcNow);
            _timer = new System.Timers.Timer();
            // If the next 6:00 am in Washington time has already passed, update the wallpaper now
            if (timeToWashingtonSixAm < TimeSpan.Zero)
            {
                await UpdateWallpaperAndRestartTimer();
                return;
            }
            // Set up the timer to update the wallpaper when the time elapses            
            _timer.Interval = Math.Max(timeToWashingtonSixAm.TotalMilliseconds, 1);
            _timer.Elapsed += async (sender, e) => await UpdateWallpaperAndRestartTimer();
            _timer.Start();

            // Wait for the timer to elapse
            await Task.Delay(timeToWashingtonSixAm);
        }

        private async Task UpdateWallpaperAndRestartTimer()
        {
            // Update the wallpaper
            await Task.Run(() => wpAPODloader.LoadPicture());
            await Task.Run(() => wpAPODloader.SetWallpaper());
            // Restart the timer
            DateTime nextWashingtonSixAm = GetNextWashingtonSixAm();
            TimeSpan timeToWashingtonSixAm = nextWashingtonSixAm.Subtract(DateTime.UtcNow);
            _timer.Interval = Math.Max(timeToWashingtonSixAm.TotalMilliseconds, 1);
        }

        public DateTime GetNextWashingtonSixAm()
        {
            TimeZoneInfo washingtonTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
            DateTime washingtonTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, washingtonTimeZone);
            DateTime nextWashingtonSixAm = new DateTime(washingtonTime.Year, washingtonTime.Month, washingtonTime.Day, 21, 23, 50, 0);
            if (washingtonTime >= nextWashingtonSixAm)
            {
                nextWashingtonSixAm = nextWashingtonSixAm.AddDays(1);
            }
            return nextWashingtonSixAm;
        }

        public void StopTimer()
        {
            if (_timer != null)
            {
                _timer.Stop();
                _timer.Dispose();
            }
        }
    }
}