using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using GameStore.Model;

namespace GameStore.ViewModel
{
    /// <summary>גישה לטבלת Games</summary>
    public class GameDB : BaseDB<Game>
    {
        protected override string TableName => "Games";

        protected override string SelectSql =>
            "SELECT t.*, g.Name AS GenreName FROM Games t INNER JOIN Genres g ON t.GenreId = g.Id";

        protected override string OrderBy => "t.Title";

        protected override Game CreateModel(SqlDataReader reader)
        {
            return new Game
            {
                Id = Convert.ToInt32(reader["Id"]),
                Title = reader["Title"].ToString(),
                Description = reader["Description"].ToString(),
                Developer = reader["Developer"].ToString(),
                Price = Convert.ToDecimal(reader["Price"]),
                ReleaseYear = Convert.ToInt32(reader["ReleaseYear"]),
                MinAge = Convert.ToInt32(reader["MinAge"]),
                GenreId = Convert.ToInt32(reader["GenreId"]),
                GenreName = reader["GenreName"].ToString(),
                ImageUrl = reader["ImageUrl"].ToString()
            };
        }

        public List<Game> SelectByGenre(int genreId)
        {
            return Select(SelectSql + " WHERE t.GenreId = @GenreId ORDER BY " + OrderBy, P("@GenreId", genreId));
        }

        public int Insert(Game game)
        {
            string sql =
                "INSERT INTO Games (Title, Description, Developer, Price, ReleaseYear, MinAge, GenreId, ImageUrl) " +
                "VALUES (@Title, @Description, @Developer, @Price, @ReleaseYear, @MinAge, @GenreId, @ImageUrl); " +
                "SELECT CAST(SCOPE_IDENTITY() AS INT);";
            return Convert.ToInt32(Scalar(sql, Parameters(game)));
        }

        public bool Update(Game game)
        {
            string sql =
                "UPDATE Games SET Title = @Title, Description = @Description, Developer = @Developer, Price = @Price, " +
                "ReleaseYear = @ReleaseYear, MinAge = @MinAge, GenreId = @GenreId, ImageUrl = @ImageUrl WHERE Id = @Id";

            List<SqlParameter> parameters = new List<SqlParameter>(Parameters(game)) { P("@Id", game.Id) };
            return Execute(sql, parameters.ToArray()) > 0;
        }

        // הפרמטרים המשותפים ל-Insert ול-Update (כדי לא לכתוב אותם פעמיים)
        private SqlParameter[] Parameters(Game game)
        {
            return new[]
            {
                P("@Title", game.Title),
                P("@Description", game.Description),
                P("@Developer", game.Developer),
                P("@Price", game.Price),
                P("@ReleaseYear", game.ReleaseYear),
                P("@MinAge", game.MinAge),
                P("@GenreId", game.GenreId),
                P("@ImageUrl", game.ImageUrl ?? "")
            };
        }
    }
}
