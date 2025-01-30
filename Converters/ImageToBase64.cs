using CrosshairWindow.Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace CrosshairWindow.Converters
{
    public static class ImageToBase64Converter
    {
        public static string BitmapToBase64String(Bitmap? bitmap)
        {
            if (bitmap == null)
                return "";

            using (MemoryStream ms = new MemoryStream())
            {
                bitmap.Save(ms, ImageFormat.Png); // Save as PNG format
                byte[] imageBytes = ms.ToArray(); // Convert to byte array
                return Convert.ToBase64String(imageBytes); // Convert to Base64 string
            }

        }
    }
}
