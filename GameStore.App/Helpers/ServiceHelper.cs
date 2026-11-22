using System;
using System.ServiceModel;
using System.Threading.Tasks;
using GameStore.App.GameServiceReference;
using Windows.UI.Xaml.Controls;

namespace GameStore.App.Helpers
{
    /// <summary>
    /// מרכז את הגישה לשירות הרשת.
    /// GameServiceClient היא מחלקה ש-Visual Studio יצר אוטומטית כשהוספנו Service Reference.
    /// כל הדפים משתמשים ב: ServiceHelper.Client.XxxAsync(...)
    /// </summary>
    public static class ServiceHelper
    {
        private static GameServiceClient client;

        public static GameServiceClient Client
        {
            get
            {
                // אם החיבור נכשל בעבר (למשל השרת לא רץ), הלקוח נכנס למצב Faulted
                // ואי אפשר להשתמש בו יותר. במקרה כזה יוצרים לקוח חדש.
                if (client == null || client.State == CommunicationState.Faulted)
                {
                    client?.Abort();
                    client = new GameServiceClient();
                }
                return client;
            }
        }

        /// <summary>מציג חלון הודעה פשוט</summary>
        public static async Task ShowMessageAsync(string title, string message)
        {
            ContentDialog dialog = new ContentDialog
            {
                Title = title,
                Content = message,
                CloseButtonText = "אישור",
                FlowDirection = Windows.UI.Xaml.FlowDirection.RightToLeft
            };
            await dialog.ShowAsync();
        }

        /// <summary>מציג שאלת כן/לא ומחזיר true אם המשתמש אישר</summary>
        public static async Task<bool> ConfirmAsync(string title, string message)
        {
            ContentDialog dialog = new ContentDialog
            {
                Title = title,
                Content = message,
                PrimaryButtonText = "כן",
                CloseButtonText = "ביטול",
                FlowDirection = Windows.UI.Xaml.FlowDirection.RightToLeft
            };
            return await dialog.ShowAsync() == ContentDialogResult.Primary;
        }

        /// <summary>הודעה אחידה כשיש בעיה בתקשורת עם השרת</summary>
        public static Task ShowErrorAsync(Exception ex)
        {
            return ShowMessageAsync("שגיאת תקשורת",
                "לא ניתן להתחבר לשירות הרשת. ודא שהפרויקט GameStore.Service רץ.\n\nפרטים: " + ex.Message);
        }
    }
}
