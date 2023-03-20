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
        private readonly string url = "https://api.nasa.gov/planetary/apod?api_key=3mCppAwIDyPLdiEMfBFovfWjpwmrp3KIgGTFRCKO";
        private readonly string api = "3mCppAwIDyPLdiEMfBFovfWjpwmrp3KIgGTFRCKO";
        public static string configPath = Path.Combine(Application.LocalUserAppDataPath, "activated");
        private string json = string.Empty;
        private string pictureURL = string.Empty;
        private string title = string.Empty;
        private string description = string.Empty;
        private string pictureFolder = string.Empty;
        private string picturePathDefault = string.Empty;
        private string picturePathModified = string.Empty;
        // Dynamic variable
        private dynamic? results;
        private SemaphoreSlim _fileSemaphore = new SemaphoreSlim(1);
        private bool _disposed = false;
        // Image related variables   
        private Image? pictureModified;
        private Brush? textColor;
        private readonly Brush? shadowBrush;
        // float variables with const values for adapt text to image, need more optimization
        const float minTitleHeightRatio = 0.13f; 
        const float maxTitleHeightRatio = 0.24f;
        const float minTitlePaddingRatio = 0.05f;
        const float maxTitlePaddingRatio = 0.08f;
        const float minDescriptionHeightRatio = 0.05f;
        const float maxDescriptionHeightRatio = 0.13f;

        // Load the current picture and title from the site https://apod.nasa.gov/apod/ and all the important things that will be used in another methods
        public async Task LoadPicture()
        {
            using (WebClient client = new())
            {
                json = client.DownloadString(url);
            }
            results = JsonConvert.DeserializeObject<dynamic>(json);
            pictureURL = results.hdurl;
            title = results.title;
            description = results.explanation;
            pictureFolder = Path.Combine(Application.LocalUserAppDataPath, "img");
            picturePathDefault = Path.Combine(pictureFolder, "APODclear.jpg");
            if (!Directory.Exists(pictureFolder)) { Directory.CreateDirectory(pictureFolder); } // Check if the folder already exists, otherwise will create a folder in the user's Local                       
            using (var httpClient = new HttpClient())
            {
                using (var response = await httpClient.GetAsync(pictureURL))
                {
                    using (var stream = await response.Content.ReadAsStreamAsync())
                    {
                        using (var fileStream = new FileStream(picturePathDefault, FileMode.Create, FileAccess.Write, FileShare.None))
                        {
                            await stream.CopyToAsync(fileStream);
                        }
                    }
                }
            }
            var createConfig = File.Create(configPath); // For now, it creates blank file "activated" and on startup if it exists, it will start program silently on background
            createConfig?.Dispose();
            await Task.CompletedTask;
        }
        // Download and set the current picture as wallpaper
        public async Task SetWallpaper()
        {            
            picturePathModified = Path.Combine(pictureFolder, "APODmodified.png");
            using (var fileStream = new FileStream(picturePathDefault, FileMode.Open, FileAccess.Read))
            {
                pictureModified = Image.FromStream(fileStream);
            }
            using (Graphics graphic = Graphics.FromImage(pictureModified))
            {
                RectangleF descriptionRect = await SetDescription(graphic, pictureModified); // Needed variable for parameter to Title to adapt title to description
                await SetTitle(graphic, pictureModified, descriptionRect);
                pictureModified.Save(picturePathModified, ImageFormat.Png); // Create new modified picture with a title, its PNG because of the lossless format
            }                       
            pictureModified?.Dispose();                     
            _ = SystemParametersInfo(0x14, 0, picturePathModified, 0x01 | 0x02);
        }
        // Set title in image and size is by width and heigh of the image and add shadow
        private async Task SetTitle(Graphics graphic, Image pictureModified, RectangleF descriptionRect)
        {
            int titlePadding = (int)(0.03 * pictureModified.Width);
            float titleHeightRatio = Math.Max(Math.Min(pictureModified.Height * 0.005f, maxTitleHeightRatio), minTitleHeightRatio);
            int titleHeight = (int)((pictureModified.Height) * titleHeightRatio);
            int offset = (int)(pictureModified.Height * -0.035); // Set height above description
            RectangleF titleRect = new(
                    titlePadding,
                    pictureModified.Height - titleHeight - 1.5f * titlePadding - offset,
                    pictureModified.Width - 2 * titlePadding,
                    titleHeight
            );
            float maxFontSize = titleRect.Height;
            float titleFontSize = maxFontSize;
            Font titleFont = new("Gill Sans Nova", titleFontSize, FontStyle.Bold);
            SizeF textSize = graphic.MeasureString(title, titleFont, (int)descriptionRect.Width);
            while (textSize.Height > titleRect.Height && titleFontSize > 1) 
            {
                titleFontSize--;
                titleFont = new Font("Gill Sans Nova", titleFontSize, FontStyle.Bold);
                textSize = graphic.MeasureString(description, titleFont, (int)titleRect.Width);
            }
            SolidBrush shadowBrush = new(Color.FromArgb(128, Color.Black));
            SolidBrush textColor = new(Color.White);
            StringFormat titleFormat = new() { Alignment = StringAlignment.Far };
            float shadowOffset = pictureModified.Height * 0.002f;
            RectangleF shadowRect = new RectangleF(titleRect.X + shadowOffset, titleRect.Y + shadowOffset, titleRect.Width, titleRect.Height);
            graphic.DrawString(title, titleFont, shadowBrush, shadowRect, titleFormat); // draw title shadow
            graphic.DrawString(title, titleFont, textColor, titleRect, titleFormat); // draw title
            await Task.CompletedTask;
        }
        // Set description in image and size is by width and heigh of the image
        private async Task<RectangleF> SetDescription(Graphics graphic, Image pictureModified)
        {
            int descriptionPadding = (int)(0.03 * pictureModified.Width);
            float descriptionHeightRatio = Math.Max(Math.Min(pictureModified.Height * 0.0005f, maxDescriptionHeightRatio), minDescriptionHeightRatio);
            int descriptionHeight = (int)((pictureModified.Height - 30) * descriptionHeightRatio);
            int offset = (int)(pictureModified.Height * 0.0005);
            RectangleF descriptionRect = new(
                    descriptionPadding,
                    pictureModified.Height - descriptionHeight - 2 * descriptionPadding - 10 - offset,
                    pictureModified.Width - 2 * descriptionPadding,
                    descriptionHeight
            );
            float maxFontSize = descriptionRect.Height;
            float descriptionFontSize = maxFontSize;
            Font descriptionFont = new("Gill Sans Nova", descriptionFontSize, FontStyle.Regular);
            SizeF textSize = graphic.MeasureString(description, descriptionFont, (int)descriptionRect.Width);
            while (textSize.Height > descriptionRect.Height && descriptionFontSize > 1)
            {
                descriptionFontSize--;
                descriptionFont = new Font("Gill Sans Nova", descriptionFontSize, FontStyle.Regular);
                textSize = graphic.MeasureString(description, descriptionFont, (int)descriptionRect.Width);
            }
            textColor = new SolidBrush(Color.White);
            StringFormat descriptionFormat = new() { Alignment = StringAlignment.Far };
            graphic.DrawString(description, descriptionFont, textColor, descriptionRect, descriptionFormat); // Draw the text using the calculated font size
            await Task.CompletedTask;
            return descriptionRect;
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