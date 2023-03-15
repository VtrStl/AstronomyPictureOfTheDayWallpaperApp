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

namespace AstronomyPictureOfTheDayWallpaperApp
{
    public class WallpaperAPODloader
    {
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern Int32 SystemParametersInfo(UInt32 action, UInt32 uParam, String vParam, UInt32 winIni);

        private readonly string url = "https://api.nasa.gov/planetary/apod?api_key=3mCppAwIDyPLdiEMfBFovfWjpwmrp3KIgGTFRCKO";
        public static string configPath = Path.Combine(Application.LocalUserAppDataPath, "activated");
        private string json = string.Empty;
        private string pictureURL = string.Empty;
        private string title = string.Empty;
        private string description = string.Empty;
        private string pictureFolder = string.Empty;
        private string picturePathDefault = string.Empty;
        private string picturePathModified = string.Empty;

        private dynamic? results;
           
        private Image? pictureModified;
        private Brush? textColor;
        private readonly Brush? shadowBrush;
        // Important values for adapt text to image, need more optimization
        const float minTitleHeightRatio = 0.15f; 
        const float maxTitleHeightRatio = 0.24f;
        const float minTitlePaddingRatio = 0.05f;
        const float maxTitlePaddingRatio = 0.08f;
        const float minDescriptionHeightRatio = 0.05f;
        const float maxDescriptionHeightRatio = 0.13f;

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
            pictureFolder = Path.Combine(Application.LocalUserAppDataPath, "img");
            picturePathDefault = Path.Combine(pictureFolder, "APODclear.jpg");            
            if (!Directory.Exists(pictureFolder)) { Directory.CreateDirectory(pictureFolder); } //Check if the folder already exists, otherwise will create a folder in the user's Local                       
            using (WebClient client = new())
            {
                client.DownloadFile(pictureURL, picturePathDefault);
            }
            var createConfig = File.Create(configPath); //For now, it creates blank file "activated" and on startup if it exists, it will start program silently on background
            createConfig?.Dispose();         
        }        
        // Download and set the current picture as wallpaper
        public async void SetWallpaper()
        {
            picturePathModified = Path.Combine(pictureFolder, "APODmodified.png");
            pictureModified = Image.FromFile(picturePathDefault);            
            using (Graphics graphic = Graphics.FromImage(pictureModified))
            {
                await SetTitle(graphic, pictureModified);
                await SetDescription(graphic, pictureModified);   
                pictureModified.Save(picturePathModified, ImageFormat.Png); //Create new modified picture with a title, its PNG because of the lossless format
            }                       
            pictureModified?.Dispose();                     
            _ = SystemParametersInfo(0x14, 0, picturePathModified, 0x01 | 0x02);
        }

        private async Task SetTitle(Graphics graphic, Image pictureModified)
        {
            int titlePadding = 15;
            float titleHeightRatio = Math.Max(Math.Min(pictureModified.Height * 0.001f, maxTitleHeightRatio), minTitleHeightRatio);
            int titleHeight = (int)((pictureModified.Height) * titleHeightRatio);
            float titleFontSize = pictureModified.Width * 0.02f;
            while (true) // adjust font size for smaller resolutions
            {
                Font titleFont = new("Gill Sans Nova", titleFontSize, FontStyle.Bold);
                SizeF titleTextSize = graphic.MeasureString(title, titleFont);
                if (titleTextSize.Width <= pictureModified.Width - titlePadding && titleTextSize.Height <= titleHeight) break;  // title fits, break out of loop
                if (titleFontSize <= 1) break; // font size too small, break out of loop
                titleFontSize -= 1; // decrease font size and try again
            }
            Font finalTitleFont = new("Gill Sans Nova", titleFontSize, FontStyle.Bold);
            SizeF finalTitleTextSize = graphic.MeasureString(title, finalTitleFont);
            float titleX = pictureModified.Width - finalTitleTextSize.Width - titlePadding;
            float titleY = pictureModified.Height - finalTitleTextSize.Height - titleHeight;            
            SolidBrush shadowBrush = new(Color.FromArgb(128, Color.Black));
            graphic.DrawString(title, finalTitleFont, shadowBrush, titleX + 3, titleY + 3); // draw title shadow            
            SolidBrush textColor = new(Color.White);
            graphic.DrawString(title, finalTitleFont, textColor, titleX, titleY); // draw title
            await Task.CompletedTask;
        }
        
        private async Task SetDescription(Graphics graphic, Image pictureModified)
        {
            int descriptionPadding = (int)(0.03 * pictureModified.Width);
            float descriptionHeightRatio = Math.Max(Math.Min(pictureModified.Height * 0.0005f, maxDescriptionHeightRatio), minDescriptionHeightRatio);
            int descriptionHeight = (int)((pictureModified.Height - 30) * descriptionHeightRatio);
            int offset = (int)(pictureModified.Height * 0.0005); // adjust the offset as needed
            RectangleF descriptionRect = new(
                    descriptionPadding,
                    pictureModified.Height - descriptionHeight - 2 * descriptionPadding - 20 - offset,
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
            StringFormat formatDescription = new()
            {
                Alignment = StringAlignment.Far
            };
            graphic.DrawString(description, descriptionFont, textColor, descriptionRect, formatDescription); // Draw the text using the calculated font size
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