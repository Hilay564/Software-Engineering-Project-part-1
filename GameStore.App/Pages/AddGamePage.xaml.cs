using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GameStore.App.GameServiceReference;
using GameStore.App.Helpers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace GameStore.App.Pages
{
    /// <summary>
    /// דף אחד לשתי פעולות:
    ///   Frame.Navigate(typeof(AddGamePage))           -> הוספת משחק חדש
    ///   Frame.Navigate(typeof(AddGamePage), gameId)   -> עריכת משחק קיים
    /// </summary>
    public sealed partial class AddGamePage : Page
    {
        private static readonly int[] Ages = { 3, 7, 12, 16, 18 };

        private List<Genre> genres = new List<Genre>();
        private Game editingGame;   // null = מצב הוספה

        public AddGamePage()
        {
            this.InitializeComponent();
            AgeCombo.ItemsSource = Ages;
            UpdatePreview();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            if (!Session.IsAdmin)
            {
                Frame.Navigate(typeof(HomePage));
                return;
            }

            try
            {
                await LoadGenresAsync();

                if (e.Parameter is int gameId)
                {
                    editingGame = await ServiceHelper.Client.GetGameByIdAsync(gameId);
                    if (editingGame != null)
                        FillForm(editingGame);
                }
            }
            catch (Exception ex)
            {
                await ServiceHelper.ShowErrorAsync(ex);
            }
        }

        private async Task LoadGenresAsync()
        {
            genres = (await ServiceHelper.Client.GetAllGenresAsync()).ToList();
            GenreCombo.ItemsSource = genres;
        }

        private void FillForm(Game g)
        {
            TitleText.Text = "עריכת משחק";
            GameTitleBox.Text = g.Title;
            DeveloperBox.Text = g.Developer;
            DescriptionBox.Text = g.Description;
            PriceBox.Text = g.Price.ToString("0.00");
            YearBox.Text = g.ReleaseYear.ToString();
            AgeCombo.SelectedItem = Ages.Contains(g.MinAge) ? (object)g.MinAge : null;
            GenreCombo.SelectedItem = genres.FirstOrDefault(x => x.Id == g.GenreId);
            ImageUrlBox.Text = g.ImageUrl ?? "";
            UpdatePreview();
        }

        private bool Validate()
        {
            bool ok = true;
            ok &= Validator.Show(GameTitleError, Validator.Required(GameTitleBox.Text, "שם משחק"));
            ok &= Validator.Show(DeveloperError, Validator.Required(DeveloperBox.Text, "חברת פיתוח"));
            ok &= Validator.Show(DescriptionError, Validator.Required(DescriptionBox.Text, "תיאור"));
            ok &= Validator.Show(PriceError, Validator.Price(PriceBox.Text));
            ok &= Validator.Show(YearError, Validator.Year(YearBox.Text));
            ok &= Validator.Show(AgeError, Validator.Selected(AgeCombo.SelectedItem, "גיל מומלץ"));
            ok &= Validator.Show(GenreError, Validator.Selected(GenreCombo.SelectedItem, "ז'אנר"));
            ok &= Validator.Show(ImageUrlError, Validator.ImageUrl(ImageUrlBox.Text));
            return ok;
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!Validate())
                return;

            Game game = new Game
            {
                Id = editingGame?.Id ?? 0,
                Title = GameTitleBox.Text.Trim(),
                Developer = DeveloperBox.Text.Trim(),
                Description = DescriptionBox.Text.Trim(),
                Price = decimal.Parse(PriceBox.Text),        // בטוח, כי Validate כבר בדק שזה מספר
                ReleaseYear = int.Parse(YearBox.Text),
                MinAge = (int)AgeCombo.SelectedItem,
                GenreId = ((Genre)GenreCombo.SelectedItem).Id,
                ImageUrl = ImageUrlBox.Text.Trim()
            };

            SaveButton.IsEnabled = false;
            try
            {
                if (editingGame == null)
                {
                    int newId = await ServiceHelper.Client.AddGameAsync(game);
                    if (newId > 0)
                    {
                        await ServiceHelper.ShowMessageAsync("נוסף", "המשחק \"" + game.Title + "\" נוסף לחנות");
                        ClearForm();
                    }
                    else
                    {
                        await ServiceHelper.ShowMessageAsync("שגיאה", "הוספת המשחק נכשלה");
                    }
                }
                else
                {
                    if (await ServiceHelper.Client.UpdateGameAsync(game))
                    {
                        await ServiceHelper.ShowMessageAsync("נשמר", "המשחק עודכן בהצלחה");
                        if (Frame.CanGoBack) Frame.GoBack();   // חוזרים לדף פרטי המשחק
                    }
                    else
                    {
                        await ServiceHelper.ShowMessageAsync("שגיאה", "עדכון המשחק נכשל");
                    }
                }
            }
            catch (Exception ex)
            {
                await ServiceHelper.ShowErrorAsync(ex);
            }
            finally
            {
                SaveButton.IsEnabled = true;
            }
        }

        private async void AddGenreButton_Click(object sender, RoutedEventArgs e)
        {
            string name = NewGenreBox.Text.Trim();
            if (!Validator.Show(NewGenreError, Validator.Required(name, "שם ז'אנר")))
                return;

            try
            {
                int newId = await ServiceHelper.Client.AddGenreAsync(name);
                if (newId <= 0)
                {
                    Validator.Show(NewGenreError, "הז'אנר כבר קיים");
                    return;
                }

                NewGenreBox.Text = "";
                await LoadGenresAsync();
                GenreCombo.SelectedItem = genres.FirstOrDefault(g => g.Id == newId);   // בוחרים אותו אוטומטית
            }
            catch (Exception ex)
            {
                await ServiceHelper.ShowErrorAsync(ex);
            }
        }

        private void ImageUrlBox_LostFocus(object sender, RoutedEventArgs e) => UpdatePreview();

        private void UpdatePreview()
        {
            PreviewImage.Source = (Windows.UI.Xaml.Media.ImageSource)
                new ImageUrlConverter().Convert(ImageUrlBox.Text.Trim(), null, null, null);
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e) => ClearForm();

        private void ClearForm()
        {
            editingGame = null;
            TitleText.Text = "הוספת משחק חדש";
            GameTitleBox.Text = DeveloperBox.Text = DescriptionBox.Text = PriceBox.Text = YearBox.Text = ImageUrlBox.Text = "";
            AgeCombo.SelectedItem = null;
            GenreCombo.SelectedItem = null;
            foreach (TextBlock error in new[] { GameTitleError, DeveloperError, DescriptionError, PriceError, YearError, AgeError, GenreError, ImageUrlError })
                Validator.Show(error, null);
            UpdatePreview();
        }
    }
}
