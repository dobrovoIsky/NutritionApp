using System.Collections.Concurrent;
using System.Globalization;

namespace NutritionApp.Converters;

public class Base64ToImageSourceConverter : IValueConverter
{
    // Кеш декодованих байтів по хешу перших 64 символів base64 рядка
    private static readonly ConcurrentDictionary<int, byte[]> _cache = new();
    private const int MaxCacheSize = 60;

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string base64 && !string.IsNullOrWhiteSpace(base64))
        {
            try
            {
                // Remove potential data URI prefix if it exists
                int commaIndex = base64.IndexOf(',');
                if (commaIndex >= 0)
                {
                    base64 = base64.Substring(commaIndex + 1);
                }

                // Використовуємо хеш для кешування
                int cacheKey = base64.Length > 64
                    ? base64.Substring(0, 64).GetHashCode() ^ base64.Length
                    : base64.GetHashCode();

                if (!_cache.TryGetValue(cacheKey, out byte[] imageBytes))
                {
                    imageBytes = System.Convert.FromBase64String(base64);

                    // Обмежуємо розмір кешу
                    if (_cache.Count >= MaxCacheSize)
                    {
                        _cache.Clear();
                    }
                    _cache[cacheKey] = imageBytes;
                }

                return ImageSource.FromStream(() => new MemoryStream(imageBytes));
            }
            catch
            {
                return null;
            }
        }
        return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
