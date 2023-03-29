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
            // Set up endless timer to check for new photo every 60 minutes
            _checkTimer = new System.Timers.Timer();
            _checkTimer.Interval = 60 * 60 * 1000;
            _checkTimer.Elapsed += async (sender, e) => await CheckForNewPhoto(_checkTimer);
            _checkTimer.Start();
            
            // Get the next 5am am in UTC time 
            DateTime nextUtcFourPm = GetNextUtcFiveAm(); 
            TimeToUtc = nextUtcFourPm.Subtract(DateTime.UtcNow);
            _dailytimer = new System.Timers.Timer();
            if (TimeToUtc < TimeSpan.Zero) // If the next 4am am in UTC time has already passed, update the wallpaper now
            {
                await UpdateWallpaperAndRestartDailyTimer(_dailytimer, _checkTimer);
                TimeToUtc = GetNextUtcFiveAm().Subtract(DateTime.UtcNow);
            }                                            
            _dailytimer.Interval = Math.Max(TimeToUtc.TotalMilliseconds, 1); // Set up the timer to update the wallpaper when the time elapses   
            _dailytimer.Elapsed += async (sender, e) => await UpdateWallpaperAndRestartDailyTimer(_dailytimer, _checkTimer);            

            // Set up one-time timer to check for new photo after 1 minute after program started on background
            _oneTimeTimer = new System.Timers.Timer();
            _oneTimeTimer.AutoReset = false;
            _oneTimeTimer.Interval = 1 * 60 * 1000;
            _oneTimeTimer.Elapsed += async (sender, e) => await CheckForNewPhoto(_checkTimer);
            _oneTimeTimer.Start();
            _dailytimer.Start();
            await Task.Delay(TimeToUtc);  // Wait for the timer to elapse           
        }
        // Code to check for new photo in APOD goes here. If there is a new photo, update the wallpaper
        private async Task CheckForNewPhoto(System.Timers.Timer _checkTimer)
        {
            int retryCount = 0;
            bool retry = true;
            while (retry && retryCount < 3) // Loop for trying LoadPicture if there is no internet connection, it will try 3 times after 15 minutes elapsed
            {
                try
                {
                    await wpAPODloader.LoadPicture();
                    if (wpAPODloader.IsMediaTypeVideo())
                    {
                        _checkTimer.Stop(); // Stop the timer if the media type is video
                    }
                    else if (IsTimeToStopCheckTimer())
                    {
                        _checkTimer.Stop();
                    }
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
        private async Task UpdateWallpaperAndRestartDailyTimer(System.Timers.Timer _dailytimer, System.Timers.Timer _checkTimer)
        {
            await CheckForNewPhoto(_checkTimer); // Update the wallpaper
            if (!_checkTimer.Enabled && !wpAPODloader.IsMediaTypeVideo() && !IsTimeToStopCheckTimer())
            {
                _checkTimer.Start(); // Start the timer if it was previously stopped and the media type is not video
            }
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
        // Checks if the current UTC time is before 4:15 AM, and returns true if so. Used to deactivate the '_checkertimer'.
        private bool IsTimeToStopCheckTimer()
        {
            DateTime utcTime = DateTime.UtcNow;
            DateTime stopCheckTime = new DateTime(utcTime.Year, utcTime.Month, utcTime.Day, 4, 15, 0, 0, DateTimeKind.Utc); // 1 hour before 5:15 am UTC
            return utcTime <= stopCheckTime; // Return true if its less 4:15 AM UTC time and deactivate the _checkertimer         
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
            _checkTimer?.Stop();
        }
    }
}