using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceProcess;
using System.Threading.Tasks;
using System.Windows;
using System.Net;

namespace AstronomyPictureOfTheDayWallpaperApp
{
    public class WallpaperAPODruntime
    {
        private WallpaperAPODloader wpAPODloader;
        private Form1 form;
        private System.Timers.Timer? _dailytimer;
        private System.Timers.Timer? _checkTimer;
        private System.Timers.Timer? _oneTimeTimer;
        private TimeSpan TimeToUtc { get; set; }
               
        public WallpaperAPODruntime(WallpaperAPODloader _wpAPODloader, Form1 _form)
        {
            wpAPODloader = _wpAPODloader;
            form = _form;
        }
        // Check if the config files exists before run the other methods in WallpaperAPODruntime and return True/False.
        public static bool ConfigExists()
        {
            if (File.Exists(WallpaperAPODloader.configPath))
            {
                return true;                
            }
            return false;
        }
        // Start the timer when config files exists and app is activated state
        public async Task StartTimers()
        {
            DateTime nextUtcFourPm = GetNextUtcFiveAm(); // Get the next 5am am in UTC time 
            TimeToUtc = nextUtcFourPm.Subtract(DateTime.UtcNow);
            _dailytimer = new System.Timers.Timer();
            if (TimeToUtc < TimeSpan.Zero) // If the next 4am am in UTC time has already passed, update the wallpaper now
            {
                await UpdateWallpaperAndRestartDailyTimer(_dailytimer);
                TimeToUtc = GetNextUtcFiveAm().Subtract(DateTime.UtcNow);
            }
            // Set up the timer to update the wallpaper when the time elapses            
            _dailytimer.Interval = Math.Max(TimeToUtc.TotalMilliseconds, 1);
            _dailytimer.Elapsed += async (sender, e) => await UpdateWallpaperAndRestartDailyTimer(_dailytimer);

            _checkTimer = new System.Timers.Timer();
            _checkTimer.Interval = 60 * 60 * 1000;
            _checkTimer.Elapsed += async (sender, e) => await CheckForNewPhoto();
            _checkTimer.Start(); // wait indefinitely to prevent the task from completing

            // Set up one-time timer to check for new photo after 1 minute after program started on background
            _oneTimeTimer = new System.Timers.Timer();
            _oneTimeTimer.AutoReset = false;
            _oneTimeTimer.Interval = 1 * 60 * 1000;
            _oneTimeTimer.Elapsed += async (sender, e) => await CheckForNewPhoto();
            _oneTimeTimer.Start();
            _dailytimer.Start();
            await Task.Delay(TimeToUtc);  // Wait for the timer to elapse           
        }
        // Code to check for new photo in APOD goes here. If there is a new photo, update the wallpaper
        private async Task CheckForNewPhoto()
        {
            int retryCount = 0;
            bool retry = true;

            while (retry && retryCount < 3) // Loop for trying LoadPicture if there is no internet connection, it will try 3 times after 15 minutes elapsed
            {
                try
                {
                    await wpAPODloader.LoadPicture();
                    retry = false;
                }
                catch (WebException webEx)
                {
                    if (webEx.Status == WebExceptionStatus.NameResolutionFailure)
                    {
                        retryCount++;
                        form.ShowBaloonTipRetry();
                        await Task.Delay(10 * 60 * 1000);
                    }
                }
                catch (Exception ex)
                {
                    WallpaperAPODloader.CreateExceptionLog(ex);
                    form.ShowErrorMessageBox(ex);
                    retry = false;
                }
            }
        }
        // Change wallpaper at 5:15AM UTC time 
        private async Task UpdateWallpaperAndRestartDailyTimer(System.Timers.Timer _dailytimer)
        {
            await CheckForNewPhoto(); // Update the wallpaper
            DateTime nextUTCtime = GetNextUtcFiveAm(); // Restart the timer
            TimeToUtc = nextUTCtime.Subtract(DateTime.UtcNow);
            _dailytimer.Interval = Math.Max(TimeToUtc.TotalMilliseconds, 1);           
        }
        // Get and set the UTC time and set another day
        private static DateTime GetNextUtcFiveAm()
        {
            DateTime utcTime = DateTime.UtcNow;
            DateTime nextUtcFourPm = new(utcTime.Year, utcTime.Month, utcTime.Day, 5, 15, 0, 0, DateTimeKind.Utc);
            if (utcTime >= nextUtcFourPm) // If the current time is greater than or equal to the next 5:15 AM UTC time, add 1 day
            {
                nextUtcFourPm = nextUtcFourPm.AddDays(1);
            }
            return nextUtcFourPm;
        }
        // Stop the timers when user click on "Deactive"       
        public void StopTimers()
        {
            if (_dailytimer != null && _checkTimer != null || _oneTimeTimer != null)
            {
                _dailytimer.Stop();
                _dailytimer.Dispose();
                _checkTimer.Stop();
                _checkTimer.Dispose();
                _oneTimeTimer.Stop();
                _oneTimeTimer.Dispose();
            }
        }
        // Stop the timers when current media type on APOD web is video.
        public void StopCheckTimerForToday()
        {
            if (_checkTimer != null)
            {
                _checkTimer.Stop();
            }
        }
    }
}