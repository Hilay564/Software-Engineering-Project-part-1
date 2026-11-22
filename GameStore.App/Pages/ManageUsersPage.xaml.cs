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
    public sealed partial class ManageUsersPage : Page
    {
        private List<User> allUsers = new List<User>();
        private User selectedUser;

        public ManageUsersPage()
        {
            this.InitializeComponent();
            Form.Configure(newUser: false);
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
                await Form.LoadCitiesAsync();
                await LoadUsersAsync();
            }
            catch (Exception ex)
            {
                await ServiceHelper.ShowErrorAsync(ex);
            }
        }

        private async Task LoadUsersAsync()
        {
            allUsers = (await ServiceHelper.Client.GetAllUsersAsync()).ToList();
            ApplyFilter();
        }

        private void ApplyFilter()
        {
            string text = SearchBox.Text.Trim().ToLower();
            UsersList.ItemsSource = allUsers
                .Where(u => text == "" ||
                            u.Username.ToLower().Contains(text) ||
                            u.FirstName.ToLower().Contains(text) ||
                            u.LastName.ToLower().Contains(text))
                .ToList();
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e) => ApplyFilter();

        // בחירת משתמש מהרשימה: ממלאים את הטופס בפרטים שלו
        private void UsersList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedUser = UsersList.SelectedItem as User;
            if (selectedUser == null)
            {
                EditPanel.Visibility = Visibility.Collapsed;
                SelectHint.Visibility = Visibility.Visible;
                return;
            }

            SelectHint.Visibility = Visibility.Collapsed;
            EditPanel.Visibility = Visibility.Visible;

            EditTitle.Text = "עריכת המשתמש " + selectedUser.Username;
            JoinDateText.Text = "הצטרף בתאריך " + selectedUser.JoinDate.ToString("dd/MM/yyyy");
            Form.Fill(selectedUser);
            AdminToggle.IsOn = selectedUser.IsAdmin;

            // מנהל לא יכול למחוק את עצמו או להוריד לעצמו הרשאות (כדי שלא יינעל מחוץ למערכת)
            bool isMe = selectedUser.Id == Session.CurrentUser.Id;
            AdminToggle.IsEnabled = !isMe;
            DeleteButton.IsEnabled = !isMe;
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedUser == null || !Form.Validate())
                return;

            User updated = Form.ToUser();
            updated.Id = selectedUser.Id;
            updated.Username = selectedUser.Username;
            int id = selectedUser.Id;

            SaveButton.IsEnabled = false;
            try
            {
                bool ok = await ServiceHelper.Client.UpdateUserAsync(updated);

                // הרשאת מנהל מתעדכנת בפעולה נפרדת, רק אם השתנתה
                if (ok && AdminToggle.IsOn != selectedUser.IsAdmin)
                    ok = await ServiceHelper.Client.SetUserAdminAsync(id, AdminToggle.IsOn);

                if (!ok)
                {
                    await ServiceHelper.ShowMessageAsync("שגיאה", "העדכון נכשל");
                    return;
                }

                // אם המנהל ערך את עצמו - מעדכנים גם את ה-Session
                if (id == Session.CurrentUser.Id)
                    Session.Refresh(await ServiceHelper.Client.GetUserByIdAsync(id));

                await LoadUsersAsync();
                UsersList.SelectedItem = allUsers.FirstOrDefault(u => u.Id == id);
                await ServiceHelper.ShowMessageAsync("נשמר", "פרטי המשתמש עודכנו בהצלחה");
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

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedUser == null) return;

            if (!await ServiceHelper.ConfirmAsync("מחיקת משתמש",
                    "למחוק את המשתמש " + selectedUser.Username + "? כל הרכישות שלו יימחקו גם הן."))
                return;

            try
            {
                if (await ServiceHelper.Client.DeleteUserAsync(selectedUser.Id))
                {
                    await LoadUsersAsync();
                    await ServiceHelper.ShowMessageAsync("נמחק", "המשתמש נמחק");
                }
            }
            catch (Exception ex)
            {
                await ServiceHelper.ShowErrorAsync(ex);
            }
        }
    }
}
