using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using GameStore.Model;

namespace GameStore.ViewModel
{
    /// <summary>גישה לטבלת Purchases</summary>
    public class PurchaseDB : BaseDB<Purchase>
    {
        protected override string TableName => "Purchases";

        protected override string SelectSql =>
            "SELECT t.*, g.Title AS GameTitle, g.ImageUrl AS GameImageUrl, u.Username " +
            "FROM Purchases t " +
            "INNER JOIN Games g ON t.GameId = g.Id " +
            "INNER JOIN Users u ON t.UserId = u.Id";

        protected override string OrderBy => "t.PurchaseDate DESC";   // החדשות קודם

        protected override Purchase CreateModel(SqlDataReader reader)
        {
            return new Purchase
            {
                Id = Convert.ToInt32(reader["Id"]),
                UserId = Convert.ToInt32(reader["UserId"]),
                GameId = Convert.ToInt32(reader["GameId"]),
                PurchaseDate = Convert.ToDateTime(reader["PurchaseDate"]),
                PricePaid = Convert.ToDecimal(reader["PricePaid"]),
                GameTitle = reader["GameTitle"].ToString(),
                GameImageUrl = reader["GameImageUrl"].ToString(),
                Username = reader["Username"].ToString()
            };
        }

        public List<Purchase> SelectByUser(int userId)
        {
            return Select(SelectSql + " WHERE t.UserId = @UserId ORDER BY " + OrderBy, P("@UserId", userId));
        }

        public bool HasPurchased(int userId, int gameId)
        {
            object count = Scalar("SELECT COUNT(*) FROM Purchases WHERE UserId = @UserId AND GameId = @GameId",
                P("@UserId", userId), P("@GameId", gameId));
            return Convert.ToInt32(count) > 0;
        }

        /// <summary>
        /// רכישה: המחיר נלקח ישירות מטבלת Games (ולא מהלקוח), כדי שאי אפשר יהיה "לזייף" מחיר.
        /// </summary>
        public bool Insert(int userId, int gameId)
        {
            string sql =
                "INSERT INTO Purchases (UserId, GameId, PricePaid) " +
                "SELECT @UserId, Id, Price FROM Games WHERE Id = @GameId";
            return Execute(sql, P("@UserId", userId), P("@GameId", gameId)) > 0;
        }

        public decimal TotalRevenue()
        {
            // ISNULL - אם אין רכישות בכלל, SUM מחזיר NULL ואז נקבל 0
            return Convert.ToDecimal(Scalar("SELECT ISNULL(SUM(PricePaid), 0) FROM Purchases"));
        }
    }
}
