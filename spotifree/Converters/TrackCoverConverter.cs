using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Spotifree.Converters
{
    public class TrackCoverConverter : IValueConverter
    {
        private static readonly BitmapImage DefaultImage = LoadDefault();

        private static BitmapImage LoadDefault()
        {
            var img = new BitmapImage();
            img.BeginInit();
            img.UriSource = new Uri("pack://application:,,,/Spotifree;component/Assets/defaultImage.png");
            img.CacheOption = BitmapCacheOption.OnLoad;
            img.EndInit();
            img.Freeze();
            return img;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string filePath && !string.IsNullOrWhiteSpace(filePath) && File.Exists(filePath))
            {
                try
                {
                    using var file = TagLib.File.Create(filePath);
                    if (file.Tag.Pictures.Length > 0)
                    {
                        var bin = file.Tag.Pictures[0].Data.Data;
                        if (bin != null && bin.Length > 0)
                        {
                            using var ms = new MemoryStream(bin);
                            var image = new BitmapImage();
                            image.BeginInit();
                            image.StreamSource = ms;

                            image.DecodePixelWidth = 200;

                            image.CacheOption = BitmapCacheOption.OnLoad;
                            image.EndInit();
                            image.Freeze();
                            return image;
                        }
                    }
                }
                catch { }
            }

            if (value is string path && File.Exists(path))
            {
                try
                {
                    var img = new BitmapImage();
                    img.BeginInit();
                    img.UriSource = new Uri(path);
                    img.DecodePixelWidth = 200; 
                    img.CacheOption = BitmapCacheOption.OnLoad;
                    img.EndInit();
                    img.Freeze();
                    return img;
                }
                catch { }
            }

            return DefaultImage;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => Binding.DoNothing;
    }
}