using System;
using System.Linq;

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
             * Data Source={Ip}:{Port}/{Service};User Id={User};Password={Pass}
             * Data Source=127.0.0.1:1521/xe;User Id=myUser;Password=myPassword

             * SqlServer:
             * Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog={Service};User ID={User};Password={Pass}
             * Data Source={Ip};Initial Catalog={Service};User ID={User};Password={Pass}
             * Server=localhost\SQLEXPRESS;Database=MyDb;Trusted_Connection=True; 

             * Sqlite:
             * Data Source=c:\mydb.db;Version=3;Password=myPassword;            
             * Data Source =c:\mydb.db;Version=3;
             */

            try
            {
                connectionString += ";"; // Add ';' to the end of the string to facilitate the extraction of the last value'

                database.Pass = ExtractValue(connectionString, "Password=") ?? ExtractValue(connectionString, "Pwd=");
                database.User = ExtractValue(connectionString, "User ID=") ?? ExtractValue(connectionString, "UserID=");

                string? dataSource = ExtractValue(connectionString, "Data Source=")
                      ?? ExtractValue(connectionString, "Server=")
                      ?? ExtractValue(connectionString, "Address=")
                      ?? ExtractValue(connectionString, "Addr=")
                      ?? ExtractValue(connectionString, "Network Address=");

                string? initialCatalog = ExtractValue(connectionString, "Initial Catalog=") ?? ExtractValue(connectionString, "Database=");
                string? integratedSecurity = ExtractValue(connectionString, "Integrated Security=") ?? ExtractValue(connectionString, "Trusted_Connection=");

                if (dataSource is null) throw new Exception("Missing Data Source information.");

                // SQLite
                if (dataSource.Trim().EndsWith(".db", StringComparison.OrdinalIgnoreCase))
                {
                    database.Type = Enums.DbType.SQLite;
                    database.Service = dataSource.Trim();
                    return true;
                }

                // SQL Server
                if (!string.IsNullOrEmpty(initialCatalog))
                {
                    database.Type = Enums.DbType.SQLServer;
                    database.Service = initialCatalog?.Trim();

                    if (!string.IsNullOrEmpty(dataSource))
                    {
                        // Named instance? Ex: localhost\SQLEXPRESS
                        if (dataSource.Contains('\\'))
                        {
                            var parts = dataSource.Split('\\');
                            database.Ip = parts[0].Trim();
                            database.Instance = parts[1].Trim();
                        }
                        else
                        {
                            database.Ip = dataSource.Trim();
                        }
                    }

                    // Detect Windows Authentication
                    if (!string.IsNullOrEmpty(integratedSecurity) &&
                        (integratedSecurity.Trim().ToLower() == "true" || integratedSecurity.Trim().ToLower() == "sspi"))
                    {
                        database.IsWindowsAuthentication = true;
                    }

                    return true;
                }

                // Oracle
                if (dataSource.Contains(":") && dataSource.Contains("/"))
                {
                    var parts = dataSource.Split(':');
                    if (parts.Length < 2 || !parts[1].Contains('/'))
                        throw new FormatException("Invalid Oracle Data Source format.");

                    database.Type = Enums.DbType.Oracle; // Version will be determined later                    
                    database.Ip = parts[0].Trim();
                    database.Port = Convert.ToInt32(parts[1].Split('/')[0]);
                    database.Service = parts[1].Split('/')[1].Trim();
                    return true;
                }

                throw new Exception("Invalid connection string or unsupported database type!");
            }
            catch (Exception ex)
            {
                throw new Exception("Invalid connection string!", ex);
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
