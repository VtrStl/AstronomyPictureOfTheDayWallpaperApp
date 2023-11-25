using System.Text;
using System.Runtime.InteropServices;
using Newtonsoft.Json;
using System.Drawing.Imaging;
using Microsoft.Win32;
using System.Drawing.Text;

namespace AstronomyPictureOfTheDayWallpaperApp
{
    public class WallpaperAPODloader : IDisposable
    {
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern Int32 SystemParametersInfo(UInt32 action, UInt32 uParam, String vParam, UInt32 winIni);
        // Define named constants for SystemParametersInfo flags
        private const int SPI_SETDESKWALLPAPER = 0x14;
        private const int SPIF_UPDATEINIFILE = 0x01;
        private const int SPIF_SENDWININICHANGE = 0x02;
        // Define named constants for Registry to set Wallpaper Style
        private const string NO_TILE = "0";
        private const string STYLE_FIT = "6";
        // String variables
        public static string configPath = Path.Combine(Application.LocalUserAppDataPath, "config.txt");
        private string pictureFolder = string.Empty;
        private string picturePathDefault = string.Empty;
        private string picturePathModified = string.Empty;
        private string json = string.Empty;
        // Image related variables
        private Image? pictureModified;
        // Bool variable
        private bool disposedValue;
        // Class variable
        private WallpaperAPODmanager wpAPODmanager;
        private ApodData? results;
        

        // Loading methods from Form class      
        public WallpaperAPODloader(WallpaperAPODmanager wpAPODmanager)
        {
            this.wpAPODmanager = wpAPODmanager;
        }

        // Validation for WallpaperAPODruntime for stopping _checkTimer
        public bool IsMediaTypeVideo()
        {
            return results is not null && results.media_type == "video";
        }

        // Load the current picture and title from the site https://apod.nasa.gov/apod/ and all the important things that will be used in another methods
        public async Task LoadPicture()
        {
            string url = "https://api.nasa.gov/planetary/apod?api_key=";
            string apiPath = Path.Combine(Application.StartupPath, "..", "..", "..", "apikey.txt"); // Need change path before release
            string api = File.ReadAllText(apiPath);
            using (HttpClient client = new())
            {
                json = await client.GetStringAsync(url + api);
                results = JsonConvert.DeserializeObject<ApodData>(json);
            }
            if (!File.Exists(configPath) && results?.media_type != "video" ||                                       // Check if config file doesnt exist
                (File.Exists(configPath) && File.ReadAllText(configPath) != results?.title && !IsMediaTypeVideo())) // OR it exists with different title AND if the mediatype is not "video"
            {
                if (results is not null)
                    await DownloadPicture(results); 
            }
            if (IsMediaTypeVideo())
            {
                wpAPODmanager.ShowBaloonTipVideo();
                CreateOnStartupShortcut();
                if (results is not null)
                    await CreateConfig(results);
            }
        }
        
        // Download the Picture when every condition is correct
        private async Task DownloadPicture(ApodData results)
        {
            pictureFolder = Path.Combine(Application.LocalUserAppDataPath, "img");
            picturePathDefault = Path.Combine(pictureFolder, "APODclear.jpg");
            if (!Directory.Exists(pictureFolder)) { Directory.CreateDirectory(pictureFolder); } // Check if the folder already exists, otherwise will create a folder in the user's Local                       
            using (HttpClient client = new())
            {
                using HttpResponseMessage response = await client.GetAsync(results.hdurl, HttpCompletionOption.ResponseHeadersRead);
                using Stream streamToReadFrom = await response.Content.ReadAsStreamAsync();
                using Stream streamToWriteTo = File.Open(picturePathDefault, FileMode.Create);
                await streamToReadFrom.CopyToAsync(streamToWriteTo, 48 * 1024); // Maybe need modify buffer size later, now this is ideal size of 48kb
            }
            await CreateConfig(results); // It create config txt file with of title name that was get from API for checking if the wallpaper is already downloaded                        
            await SetWallpaper(results);
            CreateOnStartupShortcut();
        }
        
        // Download and set the current picture as wallpaper
        private async Task SetWallpaper(ApodData results)
        {
            picturePathModified = Path.Combine(pictureFolder, "APODmodified.jpg");
            WallpaperAPODdraw wpAPODdraw = new();
            using (Graphics graphic = Graphics.FromImage(pictureModified = Image.FromFile(picturePathDefault)))
            {
                string fontPath = Path.Combine(Application.StartupPath, "..", "..", "..", "Fonts"); // Need change path before release
                PrivateFontCollection fontCollection = new();
                DirectoryInfo fontsDir = new DirectoryInfo(fontPath);
                foreach (FileInfo fontFile in fontsDir.GetFiles("*.ttf"))
                {
                    fontCollection.AddFontFile(fontFile.FullName);
                }
                RectangleF descriptionRect = await wpAPODdraw.SetDescription(graphic, pictureModified, results.explanation, fontCollection);
                await Task.Run(() => wpAPODdraw.SetTitle(graphic, pictureModified, descriptionRect, results.title, results.explanation, fontCollection));
                pictureModified.Save(picturePathModified, ImageFormat.Jpeg);
            }
            pictureModified.Dispose(); // Dispose the pictureModified object            
            wpAPODdraw.Dispose(); // Dispose the WallpaperAPODdraw object
            RegistryKey? key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", true);
            key?.SetValue(@"WallpaperStyle", STYLE_FIT);
            key?.SetValue(@"TileWallpaper", NO_TILE);
            SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, picturePathModified, SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE); // Set the modified picture as wallpaper
        }
        
        // Create config file with title of the picture on txt file for checking, if the current wallpaper is the same on the web before downloading. This method will also create lnk shortcut on the local Startup folder
        private async Task CreateConfig(ApodData results)
        {
            using (var fileStream = new FileStream(configPath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true))
            {
                await fileStream.WriteAsync(Encoding.UTF8.GetBytes(results.title));
                await fileStream.FlushAsync();
            }
        }
        
        // This method creates a shortcut of the WallpaperAPOD application in the Windows Startup folder by invoking the CreateShortcut method of the WallpaperAPODshortcut class
        private static void CreateOnStartupShortcut()
        {
            WallpaperAPODshortcut.CreateShortcut();
        }
        
        // Clear caches in the local app folder and removes shortcut from Startup folder
        public static void ClearCache()
        {
            string cacheFolder = Application.LocalUserAppDataPath;
            if (Directory.Exists(cacheFolder))
            {
                Directory.Delete(cacheFolder, true);
            }
            WallpaperAPODshortcut.DeleteShortcut();
        }
        
        // If something goes wrong and it's not handled properly, this gonna create StackTrace txt file on app local folder
        public static void CreateExceptionLog(Exception ex)
        {
            string timeStamp = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss-fff");
            string filePath = Path.Combine(Application.LocalUserAppDataPath, $"StackTrace_{timeStamp}.txt");
            using (StreamWriter sw = new(filePath, true))
            {
                sw.WriteLine($"Error occurred at {DateTime.Now}:\n{ex.Message}\n{ex.StackTrace}");
            }
        }
        
        // Dispose method with optional disposing parameter.
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    results = null;
                    json = string.Empty;
                    pictureFolder = string.Empty;
                    picturePathDefault = string.Empty;
                    picturePathModified = string.Empty;
                }
                disposedValue = true;
            }
        }
        
        // Public Dispose method that calls the protected Dispose method with disposing set to true, and suppresses finalization.
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }    
    
    // Class for binding data from API, dynamic variable causes RuntimeBinderException
    public class ApodData
    {
        public string title { get; set; } = "";
        public string explanation { get; set; } = "";
        public string hdurl { get; set; } = "";
        public string media_type { get; set; } = "";        
    }
}