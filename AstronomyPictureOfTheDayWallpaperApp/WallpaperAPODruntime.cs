using System.Net.Sockets;
using System.Net;

namespace AstronomyPictureOfTheDayWallpaperApp
{
    public class WallpaperAPODruntime
    {
        private WallpaperAPODloader wpAPODloader;
        private WallpaperAPODmanager form;
        private System.Timers.Timer? _dailytimer;
        private System.Timers.Timer? _checkTimer;
        private System.Timers.Timer? _oneTimeTimer;
        private TimeSpan TimeToUtc { get; set; }
        
        public WallpaperAPODruntime(WallpaperAPODloader wpAPODloader, WallpaperAPODmanager _form)
        {
            this.wpAPODloader = wpAPODloader;
            form = _form;
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
            if (TimeToUtc < TimeSpan.Zero) // If the next 5:15AM in UTC time has already passed, update the wallpaper now
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
            while (retry && retryCount < 3) // Loop for trying LoadPicture if there is no internet connection, it will try 3 times after 10 minutes elapsed
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
                    _oneTimeTimer?.Stop();
                    _oneTimeTimer?.Dispose();
                    wpAPODloader.Dispose();                    
                    retry = false;
                }
                catch (HttpRequestException httpEx) when ((httpEx.InnerException is SocketException)
                || (httpEx.StatusCode >= HttpStatusCode.InternalServerError && httpEx.StatusCode <= HttpStatusCode.GatewayTimeout))
                {
                    retryCount++;
                    form.ShowBaloonTipRetry();
                    await Task.Delay(10 * 60 * 1000);
                }
				catch (HttpRequestException httpEx) when (httpEx.Message.Contains("Forbidden"))
                {
                    form.ShowBaloonTipError(httpEx);
                    WallpaperAPODloader.CreateExceptionLog(httpEx);
                    MessageBox.Show($"Error: \n{httpEx.Message}\nInvalid API key probably.\nRestart the app and check {Application.LocalUserAppDataPath} folder for StackTrace", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    retry = false;
                }
                catch (HttpRequestException httpEx)
                {
                    form.ShowBaloonTipError(httpEx);
                    WallpaperAPODloader.CreateExceptionLog(httpEx);
                    WallpaperAPODmanager.ShowErrorMessageBox(httpEx);
                    retry = false;
                }
                catch (Exception ex)
                {
                    form.ShowBaloonTipError(ex);
                    WallpaperAPODloader.CreateExceptionLog(ex);                    
                    WallpaperAPODmanager.ShowErrorMessageBox(ex);
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
            DateTime nextUtcFiveAm = new(utcTime.Year, utcTime.Month, utcTime.Day, 5, 15, 0, 0, DateTimeKind.Utc);
            if (utcTime >= nextUtcFiveAm) // If the current time is greater than or equal to the next 5:15 AM UTC time, add 1 day
            {
                nextUtcFiveAm = nextUtcFiveAm.AddDays(1);
            }
            return nextUtcFiveAm;
        }
        
        // Checks if the current UTC time is hour before 5:15 AM, and returns true if so. Used to deactivate the '_checkertimer'
        private static bool IsTimeToStopCheckTimer()
        {
            DateTime utcTime = DateTime.UtcNow;
            DateTime nextUtcFiveAm = GetNextUtcFiveAm();                            
            return nextUtcFiveAm.Subtract(utcTime) <= TimeSpan.FromHours(1);  // If the time until the next 5:15 AM UTC is less than an hour, stop checking   
        }
        
        // Stop the timers when user click on "Deactive"       
        public void StopTimers()
        {
            _dailytimer?.Stop();
            _dailytimer?.Dispose();
            _checkTimer?.Stop();
            _checkTimer?.Dispose();
            _oneTimeTimer?.Stop();
            _oneTimeTimer?.Dispose();
        }
        
        // Stop the timers when current media type on APOD web is video
        public void StopCheckTimerForToday()
        {
            _checkTimer?.Stop();
        }
    }
}