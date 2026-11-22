using System.Configuration;
using System.Data.SqlClient;

namespace GameStore.ViewModel
{
    /// <summary>
    /// מחלקת Connection - נקודת הכניסה היחידה למסד הנתונים.
    ///
    /// 1. היא יודעת לפתוח חיבור ל-SQL Server (המתודה Open).
    /// 2. היא נותנת גישה לכל הטבלאות דרך מאפיינים:
    ///        Connection db = new Connection();
    ///        List&lt;Game&gt; games = db.Games.SelectAll();
    ///        User u = db.Users.Login("dani", "Dani1234");
    /// </summary>
    public class Connection
    {
        // השם של מחרוזת החיבור בקובץ Web.config של השירות
        private const string ConnectionStringName = "GameStoreDB";

        // ברירת מחדל אם לא נמצאה מחרוזת חיבור בקובץ התצורה
        private const string DefaultConnectionString =
            @"Data Source=(LocalDB)\MSSQLLocalDB;Initial Catalog=GameStoreDB;Integrated Security=True";

        /// <summary>
        /// פותח חיבור חדש למסד הנתונים ומחזיר אותו.
        /// מי שקורא למתודה חייב לסגור את החיבור - לכן תמיד משתמשים בה בתוך using.
        /// </summary>
        internal static SqlConnection Open()
        {
            ConnectionStringSettings settings = ConfigurationManager.ConnectionStrings[ConnectionStringName];
            string connectionString = settings != null ? settings.ConnectionString : DefaultConnectionString;

            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();
            return connection;
        }

        // ===== גישה לכל טבלה במסד הנתונים =====
        public UserDB Users { get; } = new UserDB();
        public CityDB Cities { get; } = new CityDB();
        public GenreDB Genres { get; } = new GenreDB();
        public GameDB Games { get; } = new GameDB();
        public PurchaseDB Purchases { get; } = new PurchaseDB();
    }
}
