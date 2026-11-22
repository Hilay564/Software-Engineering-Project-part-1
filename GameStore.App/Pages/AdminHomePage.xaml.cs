using System;
using System.Linq;
using GameStore.App.GameServiceReference;
using GameStore.App.Helpers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace GameStore.App.Pages
{
    public sealed partial class AdminHomePage : Page
    {
        public AdminHomePage()
        {
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            // הגנה: רק מנהל יכול להיכנס לדף הזה
            if (!Session.IsAdmin)
            {
                Frame.Navigate(typeof(HomePage));
                return;
            }

            WelcomeText.Text = "שלום, " + Session.CurrentUser.FirstName;

            try
            {
                AdminStats stats = await ServiceHelper.Client.GetAdminStatsAsync();
                UsersCountText.Text = stats.UsersCount.ToString();
                GamesCountText.Text = stats.GamesCount.ToString();
                PurchasesCountText.Text = stats.PurchasesCount.ToString();
                RevenueText.Text = "₪" + stats.TotalRevenue.ToString("0");

                // 10 הרכישות האחרונות (השרת כבר ממיין מהחדשה לישנה)
                RecentList.ItemsSource = (await ServiceHelper.Client.GetAllPurchasesAsync()).Take(10).ToList();
            }
            catch (Exception ex)
            {
                await ServiceHelper.ShowErrorAsync(ex);
            }
        }

        private void AddGameButton_Click(object sender, RoutedEventArgs e) => Frame.Navigate(typeof(AddGamePage));
        private void UsersButton_Click(object sender, RoutedEventArgs e) => Frame.Navigate(typeof(ManageUsersPage));
        private void CatalogButton_Click(object sender, RoutedEventArgs e) => Frame.Navigate(typeof(GamesPage));
    }
}
