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
    public sealed partial class MyGamesPage : Page
    {
        public MyGamesPage()
        {
            this.InitializeComponent();
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
                List<Purchase> purchases = (await ServiceHelper.Client.GetUserPurchasesAsync(Session.CurrentUser.Id)).ToList();
                PurchasesList.ItemsSource = purchases;

                SummaryText.Text = purchases.Count + " משחקים • סה\"כ שולם ₪" + purchases.Sum(p => p.PricePaid).ToString("0.00");
                EmptyPanel.Visibility = purchases.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                await ServiceHelper.ShowErrorAsync(ex);
            }
        }

        private void PurchasesList_ItemClick(object sender, ItemClickEventArgs e)
        {
            Frame.Navigate(typeof(GameDetailsPage), ((Purchase)e.ClickedItem).GameId);
        }

        private void CatalogButton_Click(object sender, RoutedEventArgs e) => Frame.Navigate(typeof(GamesPage));
    }
}
