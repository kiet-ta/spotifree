using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Spotifree.Converters
{
    public class TrackCoverConverter : IValueConverter
    {
        private static BitmapImage LoadDefault()
        {
            var img = new BitmapImage();
            img.BeginInit();
            img.UriSource = new Uri("pack://application:,,,/Assets/defaultImage.png");
            img.CacheOption = BitmapCacheOption.OnLoad;
            img.EndInit();
            img.Freeze();
            return img;
        }

        private static readonly BitmapImage DefaultImage = LoadDefault();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                // Đã là Bitmap (album / playlist cover)
                if (value is BitmapSource bmp)
                    return bmp;

                // TagLib picture (track.CoverArt = byte[])
                if (value is byte[] bytes && bytes.Length > 0)
                {
                    using var ms = new MemoryStream(bytes);
                    var img = new BitmapImage();
                    img.BeginInit();
                    img.CacheOption = BitmapCacheOption.OnLoad;
                    img.StreamSource = ms;
                    img.EndInit();
                    img.Freeze();
                    return img;
                }

                // Đường dẫn ảnh (nếu sau này có xài string)
                if (value is string path && !string.IsNullOrWhiteSpace(path))
                {
                    var img = new BitmapImage();
                    img.BeginInit();
                    img.UriSource = new Uri(path, UriKind.RelativeOrAbsolute);
                    img.CacheOption = BitmapCacheOption.OnLoad;
                    img.EndInit();
                    img.Freeze();
                    return img;
                }
            }
            catch
            {
            }

            return DefaultImage;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => Binding.DoNothing;
    }
}
