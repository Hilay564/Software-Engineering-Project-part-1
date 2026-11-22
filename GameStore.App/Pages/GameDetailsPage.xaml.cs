using System;
using GameStore.App.GameServiceReference;
using GameStore.App.Helpers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace GameStore.App.Pages
{
    public sealed partial class GameDetailsPage : Page
    {
        private Game game;

        public GameDetailsPage()
        {
            this.InitializeComponent();
        }

        // e.Parameter = מה שהדף הקודם שלח ב-Frame.Navigate(typeof(GameDetailsPage), game.Id)
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            int gameId = (int)e.Parameter;
            try
            {
                game = await ServiceHelper.Client.GetGameByIdAsync(gameId);
                if (game == null)
                {
                    await ServiceHelper.ShowMessageAsync("שגיאה", "המשחק לא נמצא");
                    GoBackOrCatalog();
                    return;
                }

                DataContext = game;          // מכאן כל ה-{Binding} ב-XAML יודעים מה להציג
                await UpdateButtonsAsync();
            }
            catch (Exception ex)
            {
                await ServiceHelper.ShowErrorAsync(ex);
            }
        }

        /// <summary>מחליט אילו כפתורים להציג לפי מצב המשתמש</summary>
        private async System.Threading.Tasks.Task UpdateButtonsAsync()
        {
            BuyButton.Visibility = Visibility.Collapsed;
            OwnedText.Visibility = Visibility.Collapsed;
            LoginToBuyButton.Visibility = Visibility.Collapsed;
            AdminPanel.Visibility = Session.IsAdmin ? Visibility.Visible : Visibility.Collapsed;

            if (!Session.IsLoggedIn)
            {
                LoginToBuyButton.Visibility = Visibility.Visible;     // אורח
                return;
            }

            bool owned = await ServiceHelper.Client.HasPurchasedAsync(Session.CurrentUser.Id, game.Id);
            if (owned)
                OwnedText.Visibility = Visibility.Visible;
            else
                BuyButton.Visibility = Visibility.Visible;
        }

        private async void BuyButton_Click(object sender, RoutedEventArgs e)
        {
            string price = new PriceConverter().Convert(game.Price, null, null, null).ToString();
            if (!await ServiceHelper.ConfirmAsync("אישור רכישה", "לרכוש את " + game.Title + " במחיר " + price + "?"))
                return;

            try
            {
                bool success = await ServiceHelper.Client.BuyGameAsync(Session.CurrentUser.Id, game.Id);
                if (success)
                    await ServiceHelper.ShowMessageAsync("הרכישה הושלמה", "המשחק נוסף לספרייה שלך. משחק מהנה!");
                else
                    await ServiceHelper.ShowMessageAsync("שגיאה", "הרכישה נכשלה (ייתכן שהמשחק כבר ברשותך).");

                await UpdateButtonsAsync();
            }
            catch (Exception ex)
            {
                await ServiceHelper.ShowErrorAsync(ex);
            }
        }

        private void LoginToBuyButton_Click(object sender, RoutedEventArgs e) => Frame.Navigate(typeof(LoginPage));

        // מנהל: עריכה - עוברים לדף ההוספה במצב עריכה (שולחים את ה-Id)
        private void EditButton_Click(object sender, RoutedEventArgs e) => Frame.Navigate(typeof(AddGamePage), game.Id);

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (!await ServiceHelper.ConfirmAsync("מחיקת משחק",
                    "למחוק את " + game.Title + "? גם כל הרכישות של המשחק יימחקו. אי אפשר לבטל פעולה זו."))
                return;

            try
            {
                if (await ServiceHelper.Client.DeleteGameAsync(game.Id))
                {
                    await ServiceHelper.ShowMessageAsync("נמחק", "המשחק נמחק בהצלחה");
                    GoBackOrCatalog();
                }
            }
            catch (Exception ex)
            {
                await ServiceHelper.ShowErrorAsync(ex);
            }
        }

        private void GoBackOrCatalog()
        {
            if (Frame.CanGoBack) Frame.GoBack();
            else Frame.Navigate(typeof(GamesPage));
        }
    }
}
