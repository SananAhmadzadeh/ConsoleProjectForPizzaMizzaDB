using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConsolePojectForPizzaMizzaDB.DataBase
{
    public static class DataBase
    {
        private static readonly string _connectionString =
            @"Data Source=(localdb)\MSSQLLocalDB;
                    Initial Catalog=PizzaMizzaDB;
                    Integrated Security=True;
                    Persist Security Info=False;
                    Pooling=False;
                    MultipleActiveResultSets=False;
                    Encrypt=True;
                    TrustServerCertificate=True;";

        public static SqlConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }
    }

}
