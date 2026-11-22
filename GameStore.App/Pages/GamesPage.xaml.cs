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
    public sealed partial class GamesPage : Page
    {
        // כל המשחקים מהשרת. הסינון נעשה על הרשימה הזאת בזיכרון, בלי לפנות שוב לשרת
        private List<Game> allGames = new List<Game>();

        public GamesPage()
        {
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            Loading.IsActive = true;
            try
            {
                // פריט מיוחד "כל הז'אנרים" (Id = 0) ואחריו הז'אנרים מהשרת
                List<Genre> genres = new List<Genre> { new Genre { Id = 0, Name = "כל הז'אנרים" } };
                genres.AddRange(await ServiceHelper.Client.GetAllGenresAsync());

                allGames = (await ServiceHelper.Client.GetAllGamesAsync()).ToList();

                GenreCombo.ItemsSource = genres;
                GenreCombo.SelectedIndex = 0;
                ApplyFilter();
            }
            catch (Exception ex)
            {
                await ServiceHelper.ShowErrorAsync(ex);
            }
            finally
            {
                Loading.IsActive = false;
            }
        }

        /// <summary>מסנן את המשחקים לפי טקסט החיפוש והז'אנר שנבחר</summary>
        private void ApplyFilter()
        {
            string text = SearchBox.Text.Trim().ToLower();
            Genre genre = GenreCombo.SelectedItem as Genre;

            // LINQ: Where = סינון, OrderBy = מיון
            List<Game> filtered = allGames
                .Where(g => text == "" || g.Title.ToLower().Contains(text) || g.Developer.ToLower().Contains(text))
                .Where(g => genre == null || genre.Id == 0 || g.GenreId == genre.Id)
                .OrderBy(g => g.Title)
                .ToList();

            GamesGrid.ItemsSource = filtered;
            CountText.Text = filtered.Count + " משחקים";
            EmptyText.Visibility = filtered.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e) => ApplyFilter();

        private void GenreCombo_SelectionChanged(object sender, SelectionChangedEventArgs e) => ApplyFilter();

        // לחיצה על כרטיס: עוברים לדף הפרטים ושולחים את ה-Id של המשחק
        private void GamesGrid_ItemClick(object sender, ItemClickEventArgs e)
        {
            Game game = (Game)e.ClickedItem;
            Frame.Navigate(typeof(GameDetailsPage), game.Id);
        }
    }
}
