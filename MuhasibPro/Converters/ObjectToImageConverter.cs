
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using System.IO;
using Windows.Storage.Streams;

namespace MuhasibPro.Converters;

public sealed class ObjectToImageConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is ImageSource imageSource)
        {
            return imageSource;
        }
        if (value is String url)
        {
            return url;
        }
        if (value is byte[] bytes && bytes.Length > 0)
        {
            return Decode(bytes);
        }
        return null;
    }

    /// <summary>Entity Resim alanları (byte[]) PersonPicture/Image'e bağlansın diye;
    /// bozuk veride null → varsayılan avatar (baş-harf) gösterilir.
    /// NOT: Converter UI thread'de çalışır — WriteAsync().GetAwaiter().GetResult()
    /// deadlock yapar (HATALAR: UI yolunda bloklayan çağrı yasak); senkron yazılır.</summary>
    private static object Decode(byte[] bytes)
    {
        try
        {
            var image = new BitmapImage();
            using var stream = new InMemoryRandomAccessStream();
            using (var writer = stream.AsStreamForWrite())
            {
                writer.Write(bytes, 0, bytes.Length);
                writer.Flush();
            }
            stream.Seek(0);
            image.SetSource(stream);
            return image;
        }
        catch
        {
            return null;
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
