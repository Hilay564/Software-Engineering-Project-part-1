using System;
using System.Data.SqlClient;
using GameStore.Model;

namespace GameStore.ViewModel
{
    /// <summary>גישה לטבלת Genres</summary>
    public class GenreDB : BaseDB<Genre>
    {
        protected override string TableName => "Genres";
        protected override string SelectSql => "SELECT t.* FROM Genres t";
        protected override string OrderBy => "t.Name";

        protected override Genre CreateModel(SqlDataReader reader)
        {
            return new Genre
            {
                Id = Convert.ToInt32(reader["Id"]),
                Name = reader["Name"].ToString()
            };
        }

        public bool Exists(string name)
        {
            return Convert.ToInt32(Scalar("SELECT COUNT(*) FROM Genres WHERE Name = @Name", P("@Name", name))) > 0;
        }

        /// <summary>מוסיף ז'אנר ומחזיר את ה-Id החדש</summary>
        public int Insert(string name)
        {
            // SCOPE_IDENTITY() מחזיר את ה-Id שנוצר הרגע ע"י IDENTITY
            string sql = "INSERT INTO Genres (Name) VALUES (@Name); SELECT CAST(SCOPE_IDENTITY() AS INT);";
            return Convert.ToInt32(Scalar(sql, P("@Name", name)));
        }
    }
}
