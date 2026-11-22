using System;
using System.Text.RegularExpressions;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace GameStore.App.Helpers
{
    /// <summary>
    /// בדיקות קלט (ולידציה). כל מתודה מחזירה:
    ///   null           - הקלט תקין
    ///   מחרוזת שגיאה  - הקלט לא תקין, וזו ההודעה שתוצג למשתמש
    ///
    /// כל הבדיקות נמצאות במקום אחד, וכך כל הטפסים (הרשמה, עריכת פרופיל, ניהול משתמשים)
    /// בודקים בדיוק לפי אותם כללים.
    /// </summary>
    public static class Validator
    {
        // ביטויים רגולריים (Regex) - תבנית שהטקסט חייב להתאים לה
        // ^ = תחילת הטקסט, $ = סוף הטקסט, {4,16} = בין 4 ל-16 פעמים
        private static readonly Regex UsernameRegex = new Regex(@"^[A-Za-z][A-Za-z0-9_]{3,15}$");
        private static readonly Regex NameRegex = new Regex(@"^[A-Za-zא-ת][A-Za-zא-ת '\-]{1,29}$");
        private static readonly Regex EmailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[A-Za-z]{2,}$");
        private static readonly Regex PhoneRegex = new Regex(@"^05\d-?\d{7}$");

        public static string Username(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return "יש להזין שם משתמש";
            if (!UsernameRegex.IsMatch(value))
                return "4-16 תווים באנגלית, ספרות או _ , ומתחיל באות";
            return null;
        }

        /// <summary>סיסמה: 8-20 תווים, לפחות אות גדולה, אות קטנה וספרה</summary>
        public static string Password(string value)
        {
            if (string.IsNullOrEmpty(value)) return "יש להזין סיסמה";
            if (value.Length < 8 || value.Length > 20) return "הסיסמה חייבת להכיל 8-20 תווים";
            if (!Regex.IsMatch(value, "[A-Z]")) return "הסיסמה חייבת להכיל לפחות אות גדולה אחת באנגלית";
            if (!Regex.IsMatch(value, "[a-z]")) return "הסיסמה חייבת להכיל לפחות אות קטנה אחת באנגלית";
            if (!Regex.IsMatch(value, "[0-9]")) return "הסיסמה חייבת להכיל לפחות ספרה אחת";
            if (value.Contains(" ")) return "הסיסמה לא יכולה להכיל רווחים";
            return null;
        }

        /// <summary>אימות סיסמה: שתי הסיסמאות חייבות להיות זהות</summary>
        public static string ConfirmPassword(string password, string confirm)
        {
            if (string.IsNullOrEmpty(confirm)) return "יש להזין את הסיסמה שוב";
            if (password != confirm) return "הסיסמאות אינן תואמות";
            return null;
        }

        public static string Name(string value, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(value)) return "יש להזין " + fieldName;
            if (!NameRegex.IsMatch(value.Trim())) return fieldName + " יכול להכיל אותיות בלבד (2-30 תווים)";
            return null;
        }

        public static string Email(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return "יש להזין כתובת אימייל";
            if (!EmailRegex.IsMatch(value.Trim())) return "כתובת אימייל לא תקינה (לדוגמה: name@mail.com)";
            return null;
        }

        public static string Phone(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return "יש להזין מספר טלפון";
            if (!PhoneRegex.IsMatch(value.Trim())) return "מספר נייד לא תקין (לדוגמה: 050-1234567)";
            return null;
        }

        /// <summary>תאריך לידה: חובה, לא בעתיד, גיל 8 עד 120</summary>
        public static string BirthDate(DateTimeOffset? value)
        {
            if (value == null) return "יש לבחור תאריך לידה";
            DateTime date = value.Value.Date;
            if (date > DateTime.Today) return "תאריך הלידה לא יכול להיות בעתיד";
            int age = DateTime.Today.Year - date.Year;
            if (date > DateTime.Today.AddYears(-age)) age--;   // עוד לא היה יום הולדת השנה
            if (age < 8) return "גיל מינימלי להרשמה הוא 8";
            if (age > 120) return "תאריך לידה לא הגיוני";
            return null;
        }

        /// <summary>בדיקה ששדה בחירה (ComboBox / RadioButton) נבחר</summary>
        public static string Selected(object selected, string fieldName)
        {
            return selected == null ? "יש לבחור " + fieldName : null;
        }

        public static string Required(string value, string fieldName)
        {
            return string.IsNullOrWhiteSpace(value) ? "יש להזין " + fieldName : null;
        }

        /// <summary>מחיר: מספר בין 0 ל-1000</summary>
        public static string Price(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return "יש להזין מחיר";
            if (!decimal.TryParse(value, out decimal price)) return "המחיר חייב להיות מספר";
            if (price < 0 || price > 1000) return "המחיר חייב להיות בין 0 ל-1000";
            return null;
        }

        public static string Year(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return "יש להזין שנת יציאה";
            if (!int.TryParse(value, out int year)) return "השנה חייבת להיות מספר שלם";
            if (year < 1970 || year > DateTime.Today.Year + 2) return "שנה לא הגיונית";
            return null;
        }

        /// <summary>כתובת תמונה: לא חובה, אבל אם הוזנה - חייבת להיות כתובת http/https</summary>
        public static string ImageUrl(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;
            if (!Uri.TryCreate(value.Trim(), UriKind.Absolute, out Uri uri) ||
                (uri.Scheme != "http" && uri.Scheme != "https"))
                return "כתובת תמונה לא תקינה (חייבת להתחיל ב-http או https)";
            return null;
        }

        /// <summary>
        /// מציג את השגיאה ב-TextBlock שמתחת לשדה (או מסתיר אותו אם אין שגיאה).
        /// מחזיר true אם השדה תקין - כך אפשר לכתוב:  ok &amp;= Validator.Show(ErrorText, Validator.Email(...));
        /// </summary>
        public static bool Show(TextBlock errorText, string error)
        {
            errorText.Text = error ?? "";
            errorText.Visibility = error == null ? Visibility.Collapsed : Visibility.Visible;
            return error == null;
        }
    }
}
