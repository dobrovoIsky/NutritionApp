using System.Globalization;

namespace NutritionApp.Converters;

public class Base64ToImageSourceConverter : IValueConverter
{
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

                byte[] imageBytes = System.Convert.FromBase64String(base64);
                return ImageSource.FromStream(() => new MemoryStream(imageBytes));
            }
            catch
            {
                // If it fails, don't return null if MAUI crashes on null ImageSource. But null is standard.
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
