using System;
using GameStore.App.GameServiceReference;
using GameStore.App.Helpers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace GameStore.App.Pages
{
    public sealed partial class LoginPage : Page
    {
        public LoginPage()
        {
            this.InitializeComponent();
        }

        // async = המתודה מחכה לשרת (await) בלי "להקפיא" את המסך
        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            Validator.Show(LoginError, null);

            // 1. בדיקות קלט בסיסיות: השדות לא ריקים
            bool ok = true;
            ok &= Validator.Show(UsernameError, Validator.Required(UsernameBox.Text, "שם משתמש"));
            ok &= Validator.Show(PasswordError, Validator.Required(PasswordBox.Password, "סיסמה"));
            if (!ok) return;

            // 2. שולחים לשרת
            LoginButton.IsEnabled = false;
            Loading.IsActive = true;
            try
            {
                User user = await ServiceHelper.Client.LoginAsync(UsernameBox.Text.Trim(), PasswordBox.Password);

                if (user == null)
                {
                    Validator.Show(LoginError, "שם משתמש או סיסמה שגויים");
                    return;
                }

                // 3. שומרים את המשתמש ב-Session ועוברים לדף הראשי המתאים
                Session.Login(user);
                Frame.Navigate(user.IsAdmin ? typeof(AdminHomePage) : typeof(UserHomePage));
                Frame.BackStack.Clear();   // לא חוזרים אחורה לדף ההתחברות
            }
            catch (Exception ex)
            {
                await ServiceHelper.ShowErrorAsync(ex);
            }
            finally
            {
                // finally רץ תמיד: גם אם הצליח, גם אם הייתה שגיאה וגם אחרי return
                LoginButton.IsEnabled = true;
                Loading.IsActive = false;
            }
        }

        // לחיצה על Enter בשדה הסיסמה = לחיצה על "התחבר"
        // (כותבים Windows.System.VirtualKey בשם המלא, כי ב-Windows.System יש גם מחלקה בשם User
        //  שמתנגשת עם User שלנו)
        private void PasswordBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
                LoginButton_Click(sender, null);
        }

        private void RegisterLink_Click(object sender, RoutedEventArgs e) => Frame.Navigate(typeof(RegisterPage));
    }
}
