using System;
using GameStore.App.GameServiceReference;
using GameStore.App.Helpers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace GameStore.App.Pages
{
    public sealed partial class RegisterPage : Page
    {
        public RegisterPage()
        {
            this.InitializeComponent();
            Form.Configure(newUser: true);
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            try
            {
                await Form.LoadCitiesAsync();   // ממלא את ה-ComboBox של הערים מהשרת
            }
            catch (Exception ex)
            {
                await ServiceHelper.ShowErrorAsync(ex);
            }
        }

        private async void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            // 1. בדיקות קלט בצד הלקוח
            if (!Form.Validate())
                return;

            User newUser = Form.ToUser();

            RegisterButton.IsEnabled = false;
            Loading.IsActive = true;
            try
            {
                // 2. בדיקה מול השרת: האם שם המשתמש כבר קיים?
                if (await ServiceHelper.Client.IsUsernameTakenAsync(newUser.Username))
                {
                    Form.ShowUsernameError("שם המשתמש כבר תפוס, בחר שם אחר");
                    return;
                }

                // 3. הרשמה
                int newId = await ServiceHelper.Client.RegisterAsync(newUser);
                if (newId <= 0)
                {
                    await ServiceHelper.ShowMessageAsync("שגיאה", "ההרשמה נכשלה. בדוק את הפרטים ונסה שוב.");
                    return;
                }

                // 4. התחברות אוטומטית אחרי הרשמה מוצלחת
                User user = await ServiceHelper.Client.LoginAsync(newUser.Username, newUser.Password);
                Session.Login(user);

                await ServiceHelper.ShowMessageAsync("ברוך הבא!", "נרשמת בהצלחה, " + user.FirstName + ".");
                Frame.Navigate(typeof(UserHomePage));
                Frame.BackStack.Clear();
            }
            catch (Exception ex)
            {
                await ServiceHelper.ShowErrorAsync(ex);
            }
            finally
            {
                RegisterButton.IsEnabled = true;
                Loading.IsActive = false;
            }
        }

        private void LoginLink_Click(object sender, RoutedEventArgs e) => Frame.Navigate(typeof(LoginPage));
    }
}
