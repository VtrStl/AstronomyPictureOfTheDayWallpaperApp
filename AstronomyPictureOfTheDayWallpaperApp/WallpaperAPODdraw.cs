using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing.Imaging;
using System.Drawing;

namespace AstronomyPictureOfTheDayWallpaperApp
{
    public class WallpaperAPODdraw
    {
        private Brush? textColor;
        const float minTitleHeightRatio = 0.13f;
        const float maxTitleHeightRatio = 0.24f;
        const float minDescriptionHeightRatio = 0.05f;
        const float maxDescriptionHeightRatio = 0.13f;
        
        // Set title in image and size is by width and heigh of the image and add shadow
        public async Task SetTitle(Graphics graphic, Image pictureModified, RectangleF descriptionRect, string title, string description)
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
            RectangleF shadowRect = new(titleRect.X + shadowOffset, titleRect.Y + shadowOffset, titleRect.Width, titleRect.Height);
            graphic.DrawString(title, titleFont, shadowBrush, shadowRect, titleFormat); // draw title shadow
            graphic.DrawString(title, titleFont, textColor, titleRect, titleFormat); // draw title
            await Task.CompletedTask;            
        }
        // Set description in image and size is by width and heigh of the image
        public async Task<RectangleF> SetDescription(Graphics graphic, Image pictureModified, string description)
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
    }
}