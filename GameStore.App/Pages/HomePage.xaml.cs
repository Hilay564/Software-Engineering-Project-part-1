using GameStore.App.Helpers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace GameStore.App.Pages
{
    public sealed partial class HomePage : Page
    {
        public HomePage()
        {
            this.InitializeComponent();
        }

        // OnNavigatedTo רץ בכל פעם שמגיעים לדף
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // משתמש מחובר לא צריך לראות כפתורי התחברות והרשמה
            Visibility guestOnly = Session.IsLoggedIn ? Visibility.Collapsed : Visibility.Visible;
            LoginButton.Visibility = guestOnly;
            RegisterButton.Visibility = guestOnly;
        }

        private void CatalogButton_Click(object sender, RoutedEventArgs e) => Frame.Navigate(typeof(GamesPage));
        private void LoginButton_Click(object sender, RoutedEventArgs e) => Frame.Navigate(typeof(LoginPage));
        private void RegisterButton_Click(object sender, RoutedEventArgs e) => Frame.Navigate(typeof(RegisterPage));
    }
}
