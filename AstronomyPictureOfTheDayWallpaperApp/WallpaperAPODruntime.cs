using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceProcess;
using System.Timers;
using System.Threading.Tasks;
using System.Windows;

namespace AstronomyPictureOfTheDayWallpaperApp
{
    public class WallpaperAPODruntime
    {
        private WallpaperAPODloader wpAPODloader;
        private System.Timers.Timer? _timer;
        private TimeSpan TimeToUtc { get; set; }

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
            DateTime nextUtcFourPm = GetNextUtcFourPm(); // Get the next 16:10 am in UTC time 
            TimeToUtc = nextUtcFourPm.Subtract(DateTime.UtcNow);
            _timer = new System.Timers.Timer();
            _timer.AutoReset = false;
            if (TimeToUtc < TimeSpan.Zero) // If the next 16:10 am in UTC time has already passed, update the wallpaper now
            {
                await Task.Run(UpdateWallpaperAndRestartTimer);
                TimeToUtc = GetNextUtcFourPm().Subtract(DateTime.UtcNow);
            }
            // Set up the timer to update the wallpaper when the time elapses            
            _timer.Interval = Math.Max(TimeToUtc.TotalMilliseconds, 1);
            _timer.Elapsed += async (sender, e) => await UpdateWallpaperAndRestartTimer();
            _timer.Start();
            await Task.Delay(TimeToUtc);  // Wait for the timer to elapse
        }

        private async Task UpdateWallpaperAndRestartTimer()
        {            
            await wpAPODloader.LoadPicture(); // Update the wallpaper
            DateTime nextUTCtime = GetNextUtcFourPm(); // Restart the timer
            TimeToUtc = nextUTCtime.Subtract(DateTime.UtcNow);
            _timer.Interval = Math.Max(TimeToUtc.TotalMilliseconds, 1);
        }
        // Get and set the UTC time and set another day
        private static DateTime GetNextUtcFourPm()
        {
            DateTime utcTime = DateTime.UtcNow;
            DateTime nextUtcFourPm = new(utcTime.Year, utcTime.Month, utcTime.Day, 16, 10, 0, 0, DateTimeKind.Utc);
            // If the current time is greater than or equal to the next 4:00 PM UTC time, add 1 day
            if (utcTime >= nextUtcFourPm)
            {
                nextUtcFourPm = nextUtcFourPm.AddMinutes(1);
            }
            return nextUtcFourPm;
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