using System;
using GameStore.App.GameServiceReference;

namespace GameStore.App.Helpers
{
    /// <summary>
    /// "זיכרון" של האפליקציה: מי המשתמש שמחובר כרגע.
    /// static = יש רק עותק אחד, וכל דף יכול לגשת אליו: Session.CurrentUser
    /// </summary>
    public static class Session
    {
        public static User CurrentUser { get; private set; }

        public static bool IsLoggedIn => CurrentUser != null;
        public static bool IsAdmin => CurrentUser != null && CurrentUser.IsAdmin;

        /// <summary>אירוע שמופעל כשמשתמש מתחבר או מתנתק. MainPage מאזין לו כדי לעדכן את התפריט.</summary>
        public static event Action Changed;

        public static void Login(User user)
        {
            CurrentUser = user;
            Changed?.Invoke();
        }

        public static void Logout()
        {
            CurrentUser = null;
            Changed?.Invoke();
        }

        /// <summary>מעדכן את פרטי המשתמש המחובר אחרי עריכת פרופיל (בלי לשנות את מצב ההתחברות)</summary>
        public static void Refresh(User user)
        {
            CurrentUser = user;
            Changed?.Invoke();
        }
    }
}
