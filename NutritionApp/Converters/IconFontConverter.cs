using System.Globalization;

namespace NutritionApp.Converters
{
    public class IconFontConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string iconCode && parameter is string fontFamily)
            {
                // Визначаємо колір іконки в залежності від теми
                Color iconColor = Application.Current.RequestedTheme == AppTheme.Dark ? Colors.White : Colors.Black;

                return new FontImageSource
                {
                    Glyph = iconCode,
                    FontFamily = fontFamily,
                    Color = iconColor
                };
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}