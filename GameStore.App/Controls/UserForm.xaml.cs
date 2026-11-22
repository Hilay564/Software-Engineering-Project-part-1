using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GameStore.App.GameServiceReference;
using GameStore.App.Helpers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace GameStore.App.Controls
{
    public sealed partial class UserForm : UserControl
    {
        private List<City> cities = new List<City>();
        private bool isNewUser = true;

        public UserForm()
        {
            this.InitializeComponent();

            // אפשר לבחור תאריך לידה רק בטווח הגיוני
            BirthDatePicker.MinYear = DateTimeOffset.Now.AddYears(-120);
            BirthDatePicker.MaxYear = DateTimeOffset.Now;
            BirthDatePicker.SelectedDate = null;   // מכריח את המשתמש לבחור בעצמו
        }

        /// <summary>
        /// מגדיר את הטופס למשתמש חדש (הרשמה) או לעריכת משתמש קיים.
        /// בעריכה: אי אפשר לשנות שם משתמש, והסיסמה היא לא חובה.
        /// </summary>
        public void Configure(bool newUser)
        {
            isNewUser = newUser;
            UsernameBox.IsReadOnly = !newUser;
            PasswordHint.Visibility = newUser ? Visibility.Collapsed : Visibility.Visible;
            PasswordBox.Header = newUser ? "סיסמה" : "סיסמה חדשה (לא חובה)";
            ConfirmBox.Header = newUser ? "אימות סיסמה" : "אימות סיסמה חדשה";
        }

        /// <summary>טוען את רשימת הערים מהשרת לתוך ה-ComboBox</summary>
        public async Task LoadCitiesAsync()
        {
            cities = (await ServiceHelper.Client.GetAllCitiesAsync()).ToList();
            CityCombo.ItemsSource = cities;
        }

        /// <summary>ממלא את הטופס בפרטים של משתמש קיים</summary>
        public void Fill(User user)
        {
            ClearErrors();
            UsernameBox.Text = user.Username;
            PasswordBox.Password = "";
            ConfirmBox.Password = "";
            FirstNameBox.Text = user.FirstName;
            LastNameBox.Text = user.LastName;
            EmailBox.Text = user.Email;
            PhoneBox.Text = user.Phone;
            BirthDatePicker.SelectedDate = new DateTimeOffset(user.BirthDate);
            MaleRadio.IsChecked = user.Gender == "זכר";
            FemaleRadio.IsChecked = user.Gender == "נקבה";
            CityCombo.SelectedItem = cities.FirstOrDefault(c => c.Id == user.CityId);
        }

        /// <summary>
        /// בודק את כל השדות ומציג הודעת שגיאה מתחת לכל שדה לא תקין.
        /// משתמשים ב- &amp;= (ולא &amp;&amp;) כדי שכל הבדיקות ירוצו ויוצגו כל השגיאות בבת אחת.
        /// </summary>
        public bool Validate()
        {
            bool ok = true;

            ok &= Validator.Show(UsernameError, Validator.Username(UsernameBox.Text.Trim()));

            // בהרשמה הסיסמה חובה. בעריכה בודקים אותה רק אם הוקלדה סיסמה חדשה
            bool checkPassword = isNewUser || PasswordBox.Password.Length > 0 || ConfirmBox.Password.Length > 0;
            if (checkPassword)
            {
                ok &= Validator.Show(PasswordError, Validator.Password(PasswordBox.Password));
                ok &= Validator.Show(ConfirmError, Validator.ConfirmPassword(PasswordBox.Password, ConfirmBox.Password));
            }
            else
            {
                Validator.Show(PasswordError, null);
                Validator.Show(ConfirmError, null);
            }

            ok &= Validator.Show(FirstNameError, Validator.Name(FirstNameBox.Text, "שם פרטי"));
            ok &= Validator.Show(LastNameError, Validator.Name(LastNameBox.Text, "שם משפחה"));
            ok &= Validator.Show(EmailError, Validator.Email(EmailBox.Text));
            ok &= Validator.Show(PhoneError, Validator.Phone(PhoneBox.Text));
            ok &= Validator.Show(BirthDateError, Validator.BirthDate(BirthDatePicker.SelectedDate));
            ok &= Validator.Show(GenderError, Validator.Selected(SelectedGender(), "מגדר"));
            ok &= Validator.Show(CityError, Validator.Selected(CityCombo.SelectedItem, "עיר"));

            return ok;
        }

        /// <summary>
        /// יוצר אובייקט User מהשדות בטופס. (ה-Id נקבע ע"י הדף שקורא, כי הטופס לא יודע אותו)
        /// קוראים לזה רק אחרי ש-Validate() החזיר true.
        /// </summary>
        public User ToUser()
        {
            return new User
            {
                Username = UsernameBox.Text.Trim(),
                Password = PasswordBox.Password,
                FirstName = FirstNameBox.Text.Trim(),
                LastName = LastNameBox.Text.Trim(),
                Email = EmailBox.Text.Trim(),
                Phone = PhoneBox.Text.Trim(),
                BirthDate = BirthDatePicker.SelectedDate.Value.Date,
                Gender = SelectedGender(),
                CityId = ((City)CityCombo.SelectedItem).Id
            };
        }

        /// <summary>מאפשר לדף להציג שגיאה מהשרת (למשל "שם המשתמש תפוס")</summary>
        public void ShowUsernameError(string message) => Validator.Show(UsernameError, message);

        private string SelectedGender()
        {
            if (MaleRadio.IsChecked == true) return "זכר";
            if (FemaleRadio.IsChecked == true) return "נקבה";
            return null;
        }

        private void ClearErrors()
        {
            foreach (TextBlock error in new[] { UsernameError, PasswordError, ConfirmError, FirstNameError,
                                                LastNameError, EmailError, PhoneError, BirthDateError, GenderError, CityError })
                Validator.Show(error, null);
        }
    }
}
