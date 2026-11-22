using System;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media.Imaging;

namespace GameStore.App.Helpers
{
    // ממירים (Converters) משנים ערך לפני שהוא מוצג במסך ב-Binding.
    // למשל: Price = 89.9  ->  "₪89.90"

    /// <summary>מספר -> מחיר בשקלים</summary>
    public class PriceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            decimal price = value is decimal d ? d : 0;
            return price == 0 ? "חינם" : "₪" + price.ToString("0.00");
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
            => throw new NotImplementedException();
    }

    /// <summary>כתובת תמונה -> תמונה. אם אין כתובת או שהיא לא תקינה, מציגים תמונת ברירת מחדל</summary>
    public class ImageUrlConverter : IValueConverter
    {
        private const string Placeholder = "ms-appx:///Assets/GamePlaceholder.png";

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            string url = value as string;
            if (string.IsNullOrWhiteSpace(url) || !Uri.TryCreate(url, UriKind.Absolute, out Uri uri))
                uri = new Uri(Placeholder);
            return new BitmapImage(uri);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
            => throw new NotImplementedException();
    }

    /// <summary>תאריך -> טקסט בפורמט ישראלי</summary>
    public class DateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return value is DateTime date ? date.ToString("dd/MM/yyyy") : "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
            => throw new NotImplementedException();
    }

    /// <summary>IsAdmin -> "מנהל" / "משתמש"</summary>
    public class BoolToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return value is bool b && b ? "מנהל" : "משתמש";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
            => throw new NotImplementedException();
    }
}
