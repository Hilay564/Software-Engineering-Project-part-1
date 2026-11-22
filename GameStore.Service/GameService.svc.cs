using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using GameStore.Model;
using GameStore.ViewModel;

namespace GameStore.Service
{
    /// <summary>
    /// המימוש של שירות הרשת. כל פעולה פשוט מעבירה את הבקשה לשכבת ה-ViewModel
    /// דרך מחלקת Connection. השירות עצמו לא כותב SQL, הוא רק "מתווך" בין האפליקציה למסד הנתונים.
    ///
    /// בנוסף, השירות בודק שוב את תקינות הנתונים (בדיקות צד-שרת), כי אסור לסמוך רק על הלקוח.
    /// </summary>
    public class GameService : IGameService
    {
        private readonly Connection db = new Connection();

        // =========================== משתמשים ===========================

        public User Login(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrEmpty(password))
                return null;
            return db.Users.Login(username.Trim(), password);
        }

        public int Register(User user)
        {
            if (!IsValidUser(user, isNew: true) || db.Users.IsUsernameTaken(user.Username))
                return -1;

            user.IsAdmin = false;   // משתמש חדש תמיד נרשם כמשתמש רגיל
            return db.Users.Insert(user);
        }

        public bool IsUsernameTaken(string username) => db.Users.IsUsernameTaken(username);

        public List<User> GetAllUsers() => db.Users.SelectAll();

        public User GetUserById(int id) => db.Users.SelectById(id);

        public bool UpdateUser(User user)
        {
            if (!IsValidUser(user, isNew: false))
                return false;
            return db.Users.Update(user);
        }

        public bool SetUserAdmin(int userId, bool isAdmin) => db.Users.SetAdmin(userId, isAdmin);

        public bool DeleteUser(int id) => db.Users.Delete(id);

        // ======================= ערים וז'אנרים =======================

        public List<City> GetAllCities() => db.Cities.SelectAll();

        public List<Genre> GetAllGenres() => db.Genres.SelectAll();

        public int AddGenre(string name)
        {
            if (string.IsNullOrWhiteSpace(name) || db.Genres.Exists(name.Trim()))
                return -1;
            return db.Genres.Insert(name.Trim());
        }

        // =========================== משחקים ===========================

        public List<Game> GetAllGames() => db.Games.SelectAll();

        public List<Game> GetGamesByGenre(int genreId) => db.Games.SelectByGenre(genreId);

        public Game GetGameById(int id) => db.Games.SelectById(id);

        public int AddGame(Game game)
        {
            if (!IsValidGame(game))
                return -1;
            return db.Games.Insert(game);
        }

        public bool UpdateGame(Game game)
        {
            if (!IsValidGame(game))
                return false;
            return db.Games.Update(game);
        }

        public bool DeleteGame(int id) => db.Games.Delete(id);

        // =========================== רכישות ===========================

        public bool BuyGame(int userId, int gameId)
        {
            if (db.Purchases.HasPurchased(userId, gameId))
                return false;   // כבר קנה את המשחק
            return db.Purchases.Insert(userId, gameId);
        }

        public bool HasPurchased(int userId, int gameId) => db.Purchases.HasPurchased(userId, gameId);

        public List<Purchase> GetUserPurchases(int userId) => db.Purchases.SelectByUser(userId);

        public List<Purchase> GetAllPurchases() => db.Purchases.SelectAll();

        public AdminStats GetAdminStats()
        {
            return new AdminStats
            {
                UsersCount = db.Users.Count(),
                GamesCount = db.Games.Count(),
                PurchasesCount = db.Purchases.Count(),
                TotalRevenue = db.Purchases.TotalRevenue()
            };
        }

        // ===================== בדיקות תקינות בצד השרת =====================

        private static bool IsValidUser(User u, bool isNew)
        {
            if (u == null) return false;
            if (isNew && (string.IsNullOrWhiteSpace(u.Username) || string.IsNullOrEmpty(u.Password) || u.Password.Length < 8))
                return false;
            if (string.IsNullOrWhiteSpace(u.FirstName) || string.IsNullOrWhiteSpace(u.LastName)) return false;
            if (string.IsNullOrWhiteSpace(u.Email) || !Regex.IsMatch(u.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$")) return false;
            if (u.CityId <= 0) return false;
            if (u.BirthDate > DateTime.Today || u.BirthDate.Year < 1900) return false;
            return true;
        }

        private static bool IsValidGame(Game g)
        {
            return g != null
                && !string.IsNullOrWhiteSpace(g.Title)
                && !string.IsNullOrWhiteSpace(g.Developer)
                && !string.IsNullOrWhiteSpace(g.Description)
                && g.Price >= 0
                && g.GenreId > 0
                && g.ReleaseYear >= 1970 && g.ReleaseYear <= DateTime.Today.Year + 2;
        }
    }
}
