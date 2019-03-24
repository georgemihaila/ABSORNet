using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABSORNet.Core.Imaging
{
    /// <summary>
    /// Provides extension methods
    /// </summary>
    internal static class Extensions
    {
        public static Image Scale(this Image sourceImage, int destWidth, int destHeight)
        {
            Bitmap toReturn = new Bitmap(sourceImage, destWidth, destHeight);
            toReturn.SetResolution(sourceImage.HorizontalResolution, sourceImage.VerticalResolution);
            using (Graphics graphics = Graphics.FromImage(toReturn))
            {
                graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.DrawImage(sourceImage, 0, 0, destWidth, destHeight);
            }
            return toReturn;
        }
    }
}
