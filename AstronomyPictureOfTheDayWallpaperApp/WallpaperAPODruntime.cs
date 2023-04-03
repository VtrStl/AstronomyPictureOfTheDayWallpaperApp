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
            return File.Exists(WallpaperAPODloader.configPath);
        }
        // Start the timer when config files exists and app is activated state
        public async Task StartTimers()
        {
            // Set up endless timer to check for new photo every 60 minutes
            _checkTimer = new System.Timers.Timer();
            _checkTimer.Interval = 60 * 60 * 1000;
            _checkTimer.Elapsed += async (sender, e) => await CheckForNewPhoto(_checkTimer).ConfigureAwait(false);
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
            _dailytimer.Elapsed += async (sender, e) => await UpdateWallpaperAndRestartDailyTimer(_dailytimer, _checkTimer).ConfigureAwait(false);
            _dailytimer.Start();

            // Set up one-time timer to check for new photo after 1 minute after program started on background
            _oneTimeTimer = new System.Timers.Timer();
            _oneTimeTimer.AutoReset = false;
            _oneTimeTimer.Interval = 1 * 60 * 1000;
            _oneTimeTimer.Elapsed += async (sender, e) => await CheckForNewPhoto(_checkTimer).ConfigureAwait(false);
            _oneTimeTimer.Start();
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
                    await wpAPODloader.LoadPicture().ConfigureAwait(false);
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
                    else if (webEx.Response is HttpWebResponse response && response.StatusCode == HttpStatusCode.Forbidden)
                    {
                        WallpaperAPODloader.CreateExceptionLog(webEx);
                        MessageBox.Show($"Error: \n{webEx.Message}\nInvalid API key probably.\nRestart the app and check apikey.txt or traceback", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        retry = false;
                    }
                    else
                    {
                        WallpaperAPODloader.CreateExceptionLog(webEx);
                        form.ShowErrorMessageBox(webEx);
                        retry = false;
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
        private static bool IsTimeToStopCheckTimer()
        {
            DateTime utcTime = DateTime.UtcNow;
            DateTime nextUtcFiveAm = GetNextUtcFiveAm();                            
            return nextUtcFiveAm.Subtract(utcTime) <= TimeSpan.FromHours(1);  // If the time until the next 5:00 AM UTC is less than an hour, stop checking   
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
                if (_oneTimeTimer != null)
                {
                    _oneTimeTimer.Stop();
                    _oneTimeTimer.Dispose();
                }
            }
        }
        // Stop the timers when current media type on APOD web is video.
        public void StopCheckTimerForToday()
        {
            _checkTimer?.Stop();
        }
    }
}