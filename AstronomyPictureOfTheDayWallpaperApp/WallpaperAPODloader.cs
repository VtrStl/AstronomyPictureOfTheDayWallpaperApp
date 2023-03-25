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
using System.Drawing;
using Microsoft.VisualBasic.Devices;
using System.Runtime.CompilerServices;

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
            if (!File.Exists(configPath) || File.ReadAllText(configPath) != title && mediaType != "video") //Check if config file doesnt exist OR it exists with different title AND if the mediatype is not "video"
            {
                await DownloadPicture(results);
            }
            await Task.CompletedTask;
        }

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
            var createConfig = File.WriteAllTextAsync(configPath, title); // It create config txt file with of title name that was get from API for checking if the wallpaper is already downloaded            
            await SetWallpaper();
            await Task.CompletedTask;
            createConfig?.Dispose();
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
        // Clear caches in the local app folder
        public static void ClearCache()
        {
            string cacheFolder = Application.LocalUserAppDataPath;
            if (Directory.Exists(cacheFolder))
            {
                Directory.Delete(cacheFolder, true);
            }
        }
    }
}