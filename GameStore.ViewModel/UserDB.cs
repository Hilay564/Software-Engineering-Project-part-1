using System;
using System.Data.SqlClient;
using System.Linq;
using GameStore.Model;

namespace GameStore.ViewModel
{
    /// <summary>גישה לטבלת Users</summary>
    public class UserDB : BaseDB<User>
    {
        protected override string TableName => "Users";

        // JOIN עם Cities כדי לקבל גם את שם העיר ולא רק את המספר שלה
        protected override string SelectSql =>
            "SELECT t.*, c.Name AS CityName FROM Users t INNER JOIN Cities c ON t.CityId = c.Id";

        protected override string OrderBy => "t.Username";

        protected override User CreateModel(SqlDataReader reader)
        {
            return new User
            {
                Id = Convert.ToInt32(reader["Id"]),
                Username = reader["Username"].ToString(),
                Password = "",   // את ה-Hash לא שולחים ללקוח!
                FirstName = reader["FirstName"].ToString(),
                LastName = reader["LastName"].ToString(),
                Email = reader["Email"].ToString(),
                Phone = reader["Phone"].ToString(),
                BirthDate = Convert.ToDateTime(reader["BirthDate"]),
                Gender = reader["Gender"].ToString(),
                CityId = Convert.ToInt32(reader["CityId"]),
                CityName = reader["CityName"].ToString(),
                IsAdmin = Convert.ToBoolean(reader["IsAdmin"]),
                JoinDate = Convert.ToDateTime(reader["JoinDate"])
            };
        }

        /// <summary>
        /// התחברות: מחפש משתמש עם שם המשתמש וה-Hash של הסיסמה.
        /// מחזיר את המשתמש אם נמצא, אחרת null.
        /// </summary>
        public User Login(string username, string password)
        {
            string sql = SelectSql + " WHERE t.Username = @Username AND t.PasswordHash = @Hash";
            return Select(sql,
                P("@Username", username),
                P("@Hash", PasswordHelper.Hash(password))).FirstOrDefault();
        }

        public bool IsUsernameTaken(string username)
        {
            object count = Scalar("SELECT COUNT(*) FROM Users WHERE Username = @Username", P("@Username", username));
            return Convert.ToInt32(count) > 0;
        }

        /// <summary>הרשמה: מוסיף משתמש חדש ומחזיר את ה-Id שלו</summary>
        public int Insert(User user)
        {
            string sql =
                "INSERT INTO Users (Username, PasswordHash, FirstName, LastName, Email, Phone, BirthDate, Gender, CityId, IsAdmin) " +
                "VALUES (@Username, @Hash, @FirstName, @LastName, @Email, @Phone, @BirthDate, @Gender, @CityId, @IsAdmin); " +
                "SELECT CAST(SCOPE_IDENTITY() AS INT);";

            return Convert.ToInt32(Scalar(sql,
                P("@Username", user.Username),
                P("@Hash", PasswordHelper.Hash(user.Password)),
                P("@FirstName", user.FirstName),
                P("@LastName", user.LastName),
                P("@Email", user.Email),
                P("@Phone", user.Phone),
                P("@BirthDate", user.BirthDate.Date),
                P("@Gender", user.Gender),
                P("@CityId", user.CityId),
                P("@IsAdmin", user.IsAdmin)));
        }

        /// <summary>
        /// עדכון פרטי משתמש. שם המשתמש ו-IsAdmin לא משתנים כאן.
        /// אם Password ריק - הסיסמה הקיימת נשארת. אחרת - נשמר Hash של הסיסמה החדשה.
        /// </summary>
        public bool Update(User user)
        {
            bool changePassword = !string.IsNullOrEmpty(user.Password);

            string sql =
                "UPDATE Users SET FirstName = @FirstName, LastName = @LastName, Email = @Email, Phone = @Phone, " +
                "BirthDate = @BirthDate, Gender = @Gender, CityId = @CityId" +
                (changePassword ? ", PasswordHash = @Hash" : "") +
                " WHERE Id = @Id";

            return Execute(sql,
                P("@FirstName", user.FirstName),
                P("@LastName", user.LastName),
                P("@Email", user.Email),
                P("@Phone", user.Phone),
                P("@BirthDate", user.BirthDate.Date),
                P("@Gender", user.Gender),
                P("@CityId", user.CityId),
                P("@Hash", changePassword ? PasswordHelper.Hash(user.Password) : ""),
                P("@Id", user.Id)) > 0;
        }

        /// <summary>מנהל יכול להפוך משתמש למנהל או לבטל הרשאת מנהל</summary>
        public bool SetAdmin(int userId, bool isAdmin)
        {
            return Execute("UPDATE Users SET IsAdmin = @IsAdmin WHERE Id = @Id",
                P("@IsAdmin", isAdmin), P("@Id", userId)) > 0;
        }
    }
}
