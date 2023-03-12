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

namespace AstronomyPictureOfTheDayWallpaperApp
{
    public class WallpaperAPODloader
    {
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern Int32 SystemParametersInfo(UInt32 action, UInt32 uParam, String vParam, UInt32 winIni);

        private readonly string url = "https://api.nasa.gov/planetary/apod?api_key=3mCppAwIDyPLdiEMfBFovfWjpwmrp3KIgGTFRCKO";
        public readonly string configPath = Path.Combine(Application.LocalUserAppDataPath, "activated");
        private string json = string.Empty;
        private string pictureURL = string.Empty;
        private string title = string.Empty;
        private string description = string.Empty;
        private dynamic? results;
        private Image? pictureModified;
        
        // Load the current picture and title from the site https://apod.nasa.gov/apod/ and all the important things that will be used in another methods
        public void LoadPicture()
        {
            using (WebClient client = new())
            {
                json = client.DownloadString(url);
            }
            
            results = JsonConvert.DeserializeObject<dynamic>(json);
            pictureURL = results.hdurl;
            title = results.title;
            description = results.explanation;
        }
        
        // Download and set the current picture as wallpaper
        public void SetWallpaper()
        {
            string pictureFolder = Path.Combine(Application.LocalUserAppDataPath, "img");
            string picturePathDefault = Path.Combine(pictureFolder, "APODclear.jpg");
            string picturePathModified = Path.Combine(pictureFolder, "APODmodified.png");
            
            if (!Directory.Exists(pictureFolder)) { Directory.CreateDirectory(pictureFolder); } //Check if the folder already exists, otherwise will create a folder in the user's Local                       
            using (WebClient client = new())
            {
                client.DownloadFile(pictureURL, picturePathDefault);
            }
            
            var createConfig = File.Create(configPath); //For now, it creates blank file "activated" and on startup if it exists, it will start program silently on background
            pictureModified = Image.FromFile(picturePathDefault);            
            using (Graphics graphic = Graphics.FromImage(pictureModified))
            {
                // Set the Title of the image in the bottom right with an offset
                Font titleFont = new("Arial", 13, FontStyle.Bold);
                Brush brush = new SolidBrush(Color.White);
                string titleText = title;
                int titlePadding = 10;
                SizeF titleTextSize = TextRenderer.MeasureText(titleText, titleFont);
                graphic.DrawString(titleText, titleFont, brush, pictureModified.Width - titleTextSize.Width - titlePadding, pictureModified.Height - titleTextSize.Height - 180);
                // Set the Description of the image in the bottom right with offset and below the Title
                Font descriptionFont = new("Arial", 11, FontStyle.Regular);
                Brush descriptionBrush = new SolidBrush(Color.White);
                int descriptionPadding = 10;
                int descriptionHeight = 160;
                RectangleF descriptionRect = new RectangleF(
                    descriptionPadding,
                    pictureModified.Height - descriptionHeight - 2 * descriptionPadding,
                    pictureModified.Width - 2 * descriptionPadding,
                    descriptionHeight
                );
                graphic.DrawString(description, descriptionFont, descriptionBrush, descriptionRect);
                pictureModified.Save(picturePathModified, ImageFormat.Png); //Create new modified picture with a title, its PNG because of the lossless format
            }           
            
            pictureModified?.Dispose();
            createConfig?.Dispose();            
            _ = SystemParametersInfo(0x14, 0, picturePathModified, 0x01 | 0x02);
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