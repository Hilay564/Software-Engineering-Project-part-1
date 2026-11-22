using System;
using GameStore.App.GameServiceReference;
using GameStore.App.Helpers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace GameStore.App.Pages
{
    public sealed partial class EditProfilePage : Page
    {
        public EditProfilePage()
        {
            this.InitializeComponent();
            Form.Configure(newUser: false);   // מצב עריכה: שם משתמש נעול, סיסמה לא חובה
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            if (!Session.IsLoggedIn)
            {
                Frame.Navigate(typeof(LoginPage));
                return;
            }

            try
            {
                await Form.LoadCitiesAsync();        // קודם הערים, כדי שנוכל לבחור את העיר של המשתמש
                Form.Fill(Session.CurrentUser);
            }
            catch (Exception ex)
            {
                await ServiceHelper.ShowErrorAsync(ex);
            }
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!Form.Validate())
                return;

            User updated = Form.ToUser();
            updated.Id = Session.CurrentUser.Id;              // את ה-Id לוקחים מהמשתמש המחובר
            updated.Username = Session.CurrentUser.Username;

            SaveButton.IsEnabled = false;
            Loading.IsActive = true;
            try
            {
                if (await ServiceHelper.Client.UpdateUserAsync(updated))
                {
                    // טוענים את המשתמש מחדש מהשרת כדי שה-Session יכיל את הפרטים המעודכנים
                    User fresh = await ServiceHelper.Client.GetUserByIdAsync(updated.Id);
                    Session.Refresh(fresh);
                    Form.Fill(fresh);
                    await ServiceHelper.ShowMessageAsync("נשמר", "הפרטים עודכנו בהצלחה");
                }
                else
                {
                    await ServiceHelper.ShowMessageAsync("שגיאה", "העדכון נכשל. בדוק את הפרטים ונסה שוב.");
                }
            }
            catch (Exception ex)
            {
                await ServiceHelper.ShowErrorAsync(ex);
            }
            finally
            {
                SaveButton.IsEnabled = true;
                Loading.IsActive = false;
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            if (Frame.CanGoBack) Frame.GoBack();
        }
    }
}
