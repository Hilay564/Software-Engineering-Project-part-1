using System;
using System.Linq;
using GameStore.App.Helpers;
using GameStore.App.Pages;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace GameStore.App
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();

            // בכל פעם שמישהו מתחבר / מתנתק - בונים את התפריט מחדש
            Session.Changed += BuildMenu;

            BuildMenu();
            ContentFrame.Navigate(typeof(HomePage));   // הדף הראשון: דף השער
        }

        /// <summary>
        /// בונה את התפריט לפי סוג המשתמש:
        /// אורח - דף שער, קטלוג, אודות, התחברות, הרשמה
        /// משתמש - דף הבית שלו, קטלוג, המשחקים שלי, עריכת פרופיל, התנתקות
        /// מנהל - כל מה שיש למשתמש + לוח מנהל, הוספת משחק, ניהול משתמשים
        /// </summary>
        private void BuildMenu()
        {
            NavView.MenuItems.Clear();

            if (!Session.IsLoggedIn)
            {
                AddItem("דף הבית", Symbol.Home, typeof(HomePage));
                AddItem("קטלוג משחקים", Symbol.Shop, typeof(GamesPage));
                AddItem("אודות האפליקציה", Symbol.Help, typeof(AboutPage));
                NavView.MenuItems.Add(new NavigationViewItemSeparator());
                AddItem("התחברות", Symbol.Contact, typeof(LoginPage));
                AddItem("הרשמה", Symbol.AddFriend, typeof(RegisterPage));
                UserInfoText.Text = "מצב אורח";
            }
            else
            {
                if (Session.IsAdmin)
                {
                    AddItem("לוח מנהל", Symbol.Setting, typeof(AdminHomePage));
                    AddItem("הוספת משחק", Symbol.Add, typeof(AddGamePage));
                    AddItem("ניהול משתמשים", Symbol.People, typeof(ManageUsersPage));
                    NavView.MenuItems.Add(new NavigationViewItemSeparator());
                }

                AddItem("הדף שלי", Symbol.Home, typeof(UserHomePage));
                AddItem("קטלוג משחקים", Symbol.Shop, typeof(GamesPage));
                AddItem("המשחקים שלי", Symbol.Library, typeof(MyGamesPage));
                AddItem("עריכת פרופיל", Symbol.Edit, typeof(EditProfilePage));
                AddItem("אודות האפליקציה", Symbol.Help, typeof(AboutPage));
                NavView.MenuItems.Add(new NavigationViewItemSeparator());

                // התנתקות היא לא דף, לכן ה-Tag שלה הוא מחרוזת ולא Type
                NavView.MenuItems.Add(new NavigationViewItem
                {
                    Content = "התנתקות",
                    Icon = new FontIcon { Glyph = "" },
                    Tag = "logout"
                });

                UserInfoText.Text = "מחובר: " + Session.CurrentUser.FirstName + " " + Session.CurrentUser.LastName +
                                    (Session.IsAdmin ? " (מנהל)" : "");
            }
        }

        private void AddItem(string text, Symbol icon, Type pageType)
        {
            NavView.MenuItems.Add(new NavigationViewItem
            {
                Content = text,
                Icon = new SymbolIcon(icon),
                Tag = pageType          // שומרים איזה דף לפתוח בלחיצה
            });
        }

        // לחיצה על פריט בתפריט
        private async void NavView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            object tag = args.InvokedItemContainer?.Tag;

            if (tag is string s && s == "logout")
            {
                if (await ServiceHelper.ConfirmAsync("התנתקות", "האם אתה בטוח שברצונך להתנתק?"))
                {
                    Session.Logout();
                    ContentFrame.Navigate(typeof(HomePage));
                    ContentFrame.BackStack.Clear();   // שלא יוכלו לחזור אחורה לדפים של המשתמש
                }
                return;
            }

            if (tag is Type pageType && ContentFrame.CurrentSourcePageType != pageType)
                ContentFrame.Navigate(pageType);
        }

        // כפתור "חזור" של התפריט
        private void NavView_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
        {
            if (ContentFrame.CanGoBack)
                ContentFrame.GoBack();
        }

        // אחרי כל מעבר דף: מעדכנים את כפתור החזור ומסמנים בתפריט את הדף הנוכחי
        private void ContentFrame_Navigated(object sender, NavigationEventArgs e)
        {
            NavView.IsBackEnabled = ContentFrame.CanGoBack;

            NavView.SelectedItem = NavView.MenuItems
                .OfType<NavigationViewItem>()
                .FirstOrDefault(item => item.Tag is Type t && t == e.SourcePageType);
        }
    }
}
