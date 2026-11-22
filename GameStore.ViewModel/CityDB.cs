using System;
using System.Data.SqlClient;
using GameStore.Model;

namespace GameStore.ViewModel
{
    /// <summary>גישה לטבלת Cities</summary>
    public class CityDB : BaseDB<City>
    {
        protected override string TableName => "Cities";
        protected override string SelectSql => "SELECT t.* FROM Cities t";
        protected override string OrderBy => "t.Name";

        protected override City CreateModel(SqlDataReader reader)
        {
            return new City
            {
                Id = Convert.ToInt32(reader["Id"]),
                Name = reader["Name"].ToString()
            };
        }
    }
}
