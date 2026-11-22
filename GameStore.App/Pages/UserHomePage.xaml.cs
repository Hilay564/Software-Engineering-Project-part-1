using System;
using System.Collections.Generic;
using System.Linq;
using GameStore.App.GameServiceReference;
using GameStore.App.Helpers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace GameStore.App.Pages
{
    public sealed partial class UserHomePage : Page
    {
        public UserHomePage()
        {
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            // הגנה: דף זה רק למשתמש מחובר
            if (!Session.IsLoggedIn)
            {
                Frame.Navigate(typeof(LoginPage));
                return;
            }

            User user = Session.CurrentUser;
            WelcomeText.Text = "שלום, " + user.FirstName + "! 👋";
            JoinText.Text = user.JoinDate.Year.ToString();

            try
            {
                List<Purchase> purchases = (await ServiceHelper.Client.GetUserPurchasesAsync(user.Id)).ToList();
                OwnedCountText.Text = purchases.Count.ToString();
                SpentText.Text = "₪" + purchases.Sum(p => p.PricePaid).ToString("0.00");

                // 4 המשחקים החדשים ביותר (לפי שנת יציאה)
                List<Game> games = (await ServiceHelper.Client.GetAllGamesAsync()).ToList();
                NewGamesGrid.ItemsSource = games.OrderByDescending(g => g.ReleaseYear).ThenByDescending(g => g.Id).Take(4).ToList();
            }
            catch (Exception ex)
            {
                await ServiceHelper.ShowErrorAsync(ex);
            }
        }

        private void CatalogButton_Click(object sender, RoutedEventArgs e) => Frame.Navigate(typeof(GamesPage));
        private void MyGamesButton_Click(object sender, RoutedEventArgs e) => Frame.Navigate(typeof(MyGamesPage));
        private void ProfileButton_Click(object sender, RoutedEventArgs e) => Frame.Navigate(typeof(EditProfilePage));

        private void NewGamesGrid_ItemClick(object sender, ItemClickEventArgs e)
        {
            Frame.Navigate(typeof(GameDetailsPage), ((Game)e.ClickedItem).Id);
        }
    }
}
