using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Newtonsoft.Json;
using System.Drawing.Imaging;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace AstronomyPictureOfTheDayWallpaperApp
{
    public class WallpaperAPODloader
    {
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern Int32 SystemParametersInfo(UInt32 action, UInt32 uParam, String vParam, UInt32 winIni);
        // String variables
        private readonly string url = "https://api.nasa.gov/planetary/apod?api_key=";
        private readonly string api = "3mCppAwIDyPLdiEMfBFovfWjpwmrp3KIgGTFRCKO";
        public static string configPath = Path.Combine(Application.LocalUserAppDataPath, "config.txt");
        public static string ShortcutPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Startup), "WallpaperAPOD.lnk");
        private string json = string.Empty;
        private string pictureURL = string.Empty;
        private string title = string.Empty;
        private string description = string.Empty;
        private string pictureFolder = string.Empty;
        private string picturePathDefault = string.Empty;
        private string picturePathModified = string.Empty;
        // Dynamic variable
        private dynamic? results;
        // Image related variables   
        private Image? pictureModified;
        // Class variable
        private Form1 form;
        
        // Loading methods from Form class      
        public WallpaperAPODloader(Form1 _form)
        {
            form = _form;
        }
                
        // Validation for WallpaperAPODruntime for stopping _checkTimer
        public bool IsMediaTypeVideo() 
        {
            return results.media_type == "video";            
        }
        
        // Load the current picture and title from the site https://apod.nasa.gov/apod/ and all the important things that will be used in another methods
        public async Task LoadPicture()
        {
            using (WebClient client = new())
            {
                json = await client.DownloadStringTaskAsync(url + api);
            }
            results = JsonConvert.DeserializeObject<dynamic>(json);
            title = results.title;
            string mediaType = results.media_type;
            if ((!File.Exists(configPath) && mediaType != "video" ||                                       // Check if config file doesnt exist
                (File.Exists(configPath) && File.ReadAllText(configPath) != title && !IsMediaTypeVideo()))) // OR it exists with different title AND if the mediatype is not "video"
            {
                await DownloadPicture(results);
            }
            if (IsMediaTypeVideo()) 
            { 
            //    await Task.Run(wpAPODruntime.StopCheckTimerForToday); 
                form.ShowBaloonTipVideo();
                await CreateConfig();
                await CreateOnStartupShortcut();
            }
            await Task.CompletedTask;
        }
        // Download the Picture when every condition is correct
        private async Task DownloadPicture(dynamic results)
        {            
            description = results.explanation;
            pictureFolder = Path.Combine(Application.LocalUserAppDataPath, "img");
            picturePathDefault = Path.Combine(pictureFolder, "APODclear.jpg");
            if (!Directory.Exists(pictureFolder)) { Directory.CreateDirectory(pictureFolder); } // Check if the folder already exists, otherwise will create a folder in the user's Local                       
            using (WebClient client = new())
            {
                pictureURL = results.hdurl;
                client.DownloadFile(pictureURL, picturePathDefault);
            }
            await CreateConfig(); // It create config txt file with of title name that was get from API for checking if the wallpaper is already downloaded                        
            await SetWallpaper();
            await CreateOnStartupShortcut();             
            await Task.CompletedTask;
        }
        // Download and set the current picture as wallpaper
        private async Task SetWallpaper()
        {
            WallpaperAPODdraw wpAPODdraw = new();
            picturePathModified = Path.Combine(pictureFolder, "APODmodified.png");           
            pictureModified = Image.FromFile(picturePathDefault);            
            using (Graphics graphic = Graphics.FromImage(pictureModified))
            {
                RectangleF descriptionRect = await wpAPODdraw.SetDescription(graphic, pictureModified, description);
                await wpAPODdraw.SetTitle(graphic, pictureModified, descriptionRect, title, description);
                pictureModified.Save(picturePathModified, ImageFormat.Png);
            }            
            pictureModified.Dispose(); // Dispose the pictureModified object
            _ = SystemParametersInfo(0x14, 0, picturePathModified, 0x01 | 0x02);
            await Task.CompletedTask;
        }
        // Create config file with title of the picture on txt file for checking, if the current wallpaper is the same on the web before downloading. This method will also create lnk shortcut on the local Startup folder
        private async Task CreateConfig()
        {
            using (var fileStream = new FileStream(configPath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true))
            {
                await fileStream.WriteAsync(Encoding.UTF8.GetBytes(title));
                await fileStream.FlushAsync();
            }
        }
        // This method asynchronously creates a shortcut of the WallpaperAPOD application in the Windows Startup folder by invoking the CreateShortcut method of the WallpaperAPODshortcut class.
        private async Task CreateOnStartupShortcut()
        {
             await Task.Run(WallpaperAPODshortcut.CreateShortcut);
        }
        // Clear caches in the local app folder
        public static void ClearCache()
        {
            string cacheFolder = Application.LocalUserAppDataPath;
            if (Directory.Exists(cacheFolder))
            {
                Directory.Delete(cacheFolder, true);
            }
            if (File.Exists(ShortcutPath))
            {
                WallpaperAPODshortcut.DeleteShortcut();
            }

        }
        // If something goes wrong and it's not handled properly, this gonna create traceback txt file on app local folder.
        public static void CreateExceptionLog(Exception ex)
        {
            string timeStamp = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss-fff");
            string filePath = Path.Combine(Application.LocalUserAppDataPath, $"Tracetrack_{timeStamp}.txt");
            using (StreamWriter sw = new(filePath))
            {
                sw.WriteLine($"Error occurred at {DateTime.Now}:\n{ex.Message}\n{ex.StackTrace}");
            }
        }
    }
}