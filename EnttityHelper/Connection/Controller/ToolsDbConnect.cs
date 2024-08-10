using System;

namespace EH.Connection
{
    /// <summary>
    /// Secondary functionality for manipulating connection.
    /// </summary>
    internal static class ToolsDbConnect
    {
        public static bool MapDatabase(string connectionString, Database database)
        {
            /*
             * Oracle:
             * "Data Source={Ip}:{Port}/{Service};User Id={User};Password={Pass}"
             * Data Source=127.0.0.1:1521/xe;User Id=myUser;Password=myPassword

             * SqlServer:
             * Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog={Service};User ID={User};Password={Pass}"
             * "Data Source={Ip};Initial Catalog={Service};User ID={User};Password={Pass}"

             * Sqlite:
             * Data Source=c:\mydb.db;Version=3;Password=myPassword;            
             * Data Source =c:\mydb.db;Version=3;
             */

            try
            {
                connectionString += ";"; // Add ';' to the end of the string to facilitate the extraction of the last value'
                database.Pass = ExtractValue(connectionString, "Password=");
                database.User = ExtractValue(connectionString, "User ID=");
                string? dataSource = ExtractValue(connectionString, "Data Source=");
                string? inicialCatalog = ExtractValue(connectionString, "Initial Catalog=");

                if (inicialCatalog != null) // SqlServer
                {
                    database.Ip = dataSource;
                    database.Service = inicialCatalog;
                    database.Type = Enums.DbType.SQLServer;
                    return true;
                }
                else if (dataSource is not null) // Oracle
                {
                    database.Ip = dataSource.Split(':')[0];
                    database.Port = Convert.ToInt32(connectionString.Split(':')[1].Split('/')[0]);
                    database.Service = dataSource.Split(':')[1].Split('/')[1];
                    database.Type = Enums.DbType.Oracle;
                    return true;
                }

                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        static string? ExtractValue(string connectionString, string key, string delimiter = ";")
        {
            int startIndex = connectionString.ToLower().IndexOf(key.ToLower()) + key.Length;
            int endIndex = connectionString.IndexOf(delimiter, startIndex);

            if (startIndex >= key.Length && endIndex > startIndex)
            {
                return connectionString.Substring(startIndex, endIndex - startIndex);
            }

            return null;
        }

    }
}
