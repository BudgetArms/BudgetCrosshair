using CrosshairWindow.Model;
using System;
using System.Collections.Generic;
using System.Drawing;
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
    public class Base64ToImageConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string? imageOrBase64String = value as string;

            if (string.IsNullOrEmpty(imageOrBase64String))
                return null;

            // if normal string
            if (imageOrBase64String.Contains(".png") || imageOrBase64String.Contains(".jpg")  )
                    return imageOrBase64String;
        
            try
            {
                // Remove the data:image/png;base64, part if present
                if (imageOrBase64String.Contains(','))
					imageOrBase64String = imageOrBase64String[(imageOrBase64String.IndexOf(',') + 1)..];

                byte[] imageBytes = System.Convert.FromBase64String(imageOrBase64String);
                var bitmap = new BitmapImage();

                using (var ms = new MemoryStream(imageBytes))
                {
                    bitmap.BeginInit();
                    bitmap.StreamSource = ms;
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                }

                return bitmap;
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.ToString());

                Task.Run(() =>
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show(ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    });
                });

                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }


    public static class StringImageConverter
    {
        public static Bitmap? Base64ToBitmap(string base64Image)
        {
            if (string.IsNullOrEmpty(base64Image))
                return null;

            // if normal string
            if( base64Image.Contains(".png") || base64Image.Contains(".jpg") || base64Image.Contains(".bmp") )
            {
                Console.WriteLine(Path.GetFullPath(base64Image));
                //Image imagetest = Image.FromFile(base64Image);

                if (!File.Exists(base64Image))
                {

                    string projectDir = Directory.GetCurrentDirectory();
                    Console.WriteLine(projectDir);

                    return null;
                }
                    
                return new Bitmap(base64Image);
            }

            // Remove the data:image/png;base64, part if present
            if (base64Image.Contains(','))
                base64Image = base64Image[(base64Image.IndexOf(',') + 1)..];

            byte[] imageBytes = Convert.FromBase64String(base64Image);

            // Create a MemoryStream from the byte array
            using (MemoryStream ms = new MemoryStream(imageBytes))
            {
                // Convert the MemoryStream to a Bitmap
                return new Bitmap(ms);
            }
        }

        public static WriteableBitmap? Base64ToWriteableBitmap(string base64String)
        {
            if (string.IsNullOrEmpty(base64String))
                return null;

            // Remove "data:image/png;base64," if present
            string base64Data = base64String.Contains(",") ? base64String.Split(',')[1] : base64String;

            // Convert Base64 string to byte array
            byte[] imageBytes = Convert.FromBase64String(base64Data);

            using (var ms = new MemoryStream(imageBytes))
            {
                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.StreamSource = ms;
                bitmapImage.EndInit();
                bitmapImage.Freeze(); // Make it thread-safe

                return new WriteableBitmap(bitmapImage);
            }
        }
    }


}
