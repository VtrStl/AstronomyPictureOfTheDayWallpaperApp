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
        private System.Timers.Timer? _dailytimer;
        private System.Timers.Timer? _checkTimer;
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
            DateTime nextUtcFourPm = GetNextUtcFourAm(); // Get the next 4am am in UTC time 
            TimeToUtc = nextUtcFourPm.Subtract(DateTime.UtcNow);
            _dailytimer = new System.Timers.Timer();
            if (TimeToUtc < TimeSpan.Zero) // If the next 4am am in UTC time has already passed, update the wallpaper now
            {
                await UpdateWallpaperAndRestartDailyTimer(_dailytimer);
                TimeToUtc = GetNextUtcFourAm().Subtract(DateTime.UtcNow);
            }            
            // Set up the timer to update the wallpaper when the time elapses            
            _dailytimer.Interval = Math.Max(TimeToUtc.TotalMilliseconds, 1);
            _dailytimer.Elapsed += async (sender, e) => await UpdateWallpaperAndRestartDailyTimer(_dailytimer);
            var checkTask = Task.Run(async () =>
            {
                _checkTimer = new System.Timers.Timer();
                _checkTimer.Interval = 1 * 60 * 1000;
                _checkTimer.Elapsed += async (sender, e) => await CheckForNewPhoto();
                _checkTimer.Start();
                await Task.Delay(-1); // wait indefinitely to prevent the task from completing
            });
            _dailytimer.Start();
            await Task.Delay(TimeToUtc);  // Wait for the timer to elapse           
        }

        private async Task CheckForNewPhoto()
        {
            // Code to check for new photo in APOD goes here
            // If there is a new photo, update the wallpaper
            await wpAPODloader.LoadPicture();
        }

        private async Task UpdateWallpaperAndRestartDailyTimer(System.Timers.Timer _dailytimer)
        {            
            await wpAPODloader.LoadPicture(); // Update the wallpaper
            DateTime nextUTCtime = GetNextUtcFourAm(); // Restart the timer
            TimeToUtc = nextUTCtime.Subtract(DateTime.UtcNow);
            _dailytimer.Interval = Math.Max(TimeToUtc.TotalMilliseconds, 1);
        }
        // Get and set the UTC time and set another day
        private static DateTime GetNextUtcFourAm()
        {
            DateTime utcTime = DateTime.UtcNow;
            DateTime nextUtcFourPm = new(utcTime.Year, utcTime.Month, utcTime.Day, 4, 15, 0, 0, DateTimeKind.Unspecified);
            // If the current time is greater than or equal to the next 4:15 AM UTC time, add 1 day
            if (utcTime >= nextUtcFourPm)
            {
                nextUtcFourPm = nextUtcFourPm.AddDays(1);
            }
            return nextUtcFourPm;
        }
        // Stop the timers when user click on "Deactive"
        public void StopTimers()
        {
            if (_dailytimer != null && _checkTimer != null)
            {
                _dailytimer.Stop();
                _dailytimer.Dispose();
                _checkTimer.Stop();
                _checkTimer.Dispose();
            }
        }
    }
}