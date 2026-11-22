using System.Collections.Generic;
using System.ServiceModel;
using GameStore.Model;

namespace GameStore.Service
{
    /// <summary>
    /// החוזה (Contract) של שירות הרשת: רשימת כל הפעולות שהאפליקציה יכולה לבקש מהשרת.
    /// [ServiceContract]  - הממשק הזה הוא שירות WCF
    /// [OperationContract] - הפעולה הזאת חשופה ללקוחות דרך הרשת
    ///
    /// כשמוסיפים Service Reference באפליקציית ה-UWP, Visual Studio קורא את החוזה הזה
    /// ומייצר מחלקה GameServiceClient עם אותן פעולות (בגרסה אסינכרונית: LoginAsync וכו').
    /// </summary>
    [ServiceContract]
    public interface IGameService
    {
        // ---------- משתמשים ----------
        [OperationContract] User Login(string username, string password);
        [OperationContract] int Register(User user);            // מחזיר Id חדש, או -1 אם נכשל
        [OperationContract] bool IsUsernameTaken(string username);
        [OperationContract] List<User> GetAllUsers();
        [OperationContract] User GetUserById(int id);
        [OperationContract] bool UpdateUser(User user);
        [OperationContract] bool SetUserAdmin(int userId, bool isAdmin);
        [OperationContract] bool DeleteUser(int id);

        // ---------- ערים וז'אנרים ----------
        [OperationContract] List<City> GetAllCities();
        [OperationContract] List<Genre> GetAllGenres();
        [OperationContract] int AddGenre(string name);           // מחזיר Id חדש, או -1 אם קיים

        // ---------- משחקים ----------
        [OperationContract] List<Game> GetAllGames();
        [OperationContract] List<Game> GetGamesByGenre(int genreId);
        [OperationContract] Game GetGameById(int id);
        [OperationContract] int AddGame(Game game);
        [OperationContract] bool UpdateGame(Game game);
        [OperationContract] bool DeleteGame(int id);

        // ---------- רכישות ----------
        [OperationContract] bool BuyGame(int userId, int gameId);
        [OperationContract] bool HasPurchased(int userId, int gameId);
        [OperationContract] List<Purchase> GetUserPurchases(int userId);
        [OperationContract] List<Purchase> GetAllPurchases();

        // ---------- סטטיסטיקה ללוח המנהל ----------
        [OperationContract] AdminStats GetAdminStats();
    }
}
