using EH.Connection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EH.Command
{
    internal static class Definitions
    {
        internal static void DefineTypesDefaultColumnsDb(Database? dbContext)
        {
            if (dbContext is null) throw new InvalidOperationException("DbContext cannot be null.");
            if (dbContext.Provider is null) throw new InvalidOperationException("DbContext Type cannot be null.");

            var typesDefaultColumnsDb = dbContext.Provider switch
            {
                Enums.DbProvider.Oracle => new Dictionary<string, string> {
                    { "String", "VARCHAR2(4000)" },
                    { "Boolean", "NUMBER(1)" },
                    { "DateTime", "TIMESTAMP" },
                    { "Decimal", "NUMBER(18,6)" },
                    { "Double", "BINARY_DOUBLE" },
                    { "Int16", "NUMBER(5)" },
                    { "Int32", "NUMBER(10)" },
                    { "Int64", "NUMBER(19)" },
                    { "Single", "BINARY_FLOAT" },
                    { "TimeSpan", "INTERVAL DAY TO SECOND" },
                    { "Guid", "RAW(16)" },
                    { "Char", "NCHAR(1)" },
                    { "ByteArray", "BLOB" },
                    { "Object", "CLOB" }
                    },
                Enums.DbProvider.SqlServer => new Dictionary<string, string>
                    {
                    { "String", "NVARCHAR(4000)" },
                    { "Boolean", "BIT" },
                    { "DateTime", "DATETIME" },
                    { "Decimal", "DECIMAL" },
                    { "Double", "FLOAT" },
                    { "Int16", "SMALLINT" },
                    { "Int32", "INT" },
                    { "Int64", "BIGINT" },
                    { "Single", "REAL" },
                    { "TimeSpan", "TIME" },
                    { "Guid", "UNIQUEIDENTIFIER" },
                    { "Object", "NVARCHAR(4000)" }
                    },
                Enums.DbProvider.SqLite => new Dictionary<string, string>
                    {
                    { "String", "TEXT" },
                    { "Boolean", "INTEGER" },
                    { "DateTime", "TEXT" },
                    { "Decimal", "REAL" },
                    { "Double", "REAL" },
                    { "Int16", "INTEGER" },
                    { "Int32", "INTEGER" },
                    { "Int64", "INTEGER" },
                    { "Single", "REAL" },
                    { "TimeSpan", "TEXT" },
                    { "Guid", "TEXT" },
                    { "Object", "TEXT" }
                    },
                //case Enums.DbType.PostgreSQL:
                //    break;
                //case Enums.DbType.MySQL:
                //    break;
                null => throw new InvalidOperationException($"Database type is invalid."),
                _ => throw new InvalidOperationException($"Database type '{dbContext.Provider}' not yet supported."),
            };
            
            dbContext.TypesDefault = typesDefaultColumnsDb;
        }

        //internal static void DefineTypeVersionDb(Database? dbContext, Features features)
        //{
        //    if (dbContext is null) throw new InvalidOperationException("DbContext cannot be null.");
        //    if (dbContext.Type is null) throw new InvalidOperationException("DbContext Type cannot be null.");
        //    if (!dbContext.Type.Equals(Enums.DbType.Oracle)) return; // The need to define the database type version is only for Oracle

        //    const int modernOracleVersion = 12; // 12c or newer

        //    var versionDb = features.GetDatabaseVersion(dbContext);
        //    var versionMatch = System.Text.RegularExpressions.Regex.Match(versionDb, @"\b\d+\b");

        //    if (!versionMatch.Success)
        //    {
        //        dbContext.Type = Enums.DbType.Oracle;
        //        return;
        //    }

        //    var majorVersion = versionMatch.Value;
        //    dbContext.Type = Convert.ToInt32(majorVersion) >= modernOracleVersion ? Enums.DbType.Oracle_Newer : Enums.DbType.Oracle;
        //}

        internal static void DefineVersionDb(Database? dbContext, Features features)
        {
            try
            {
                if (dbContext is null) throw new InvalidOperationException("DbContext cannot be null.");
                if (dbContext.Provider is null) throw new InvalidOperationException("DbContext Type cannot be null.");

                var versionDb = features.GetDatabaseVersion(dbContext); // Ex.: "Oracle Database 19c Enterprise Edition Release 19.0.0.0.0 - Production"
                var versionMatch = System.Text.RegularExpressions.Regex.Match(versionDb, @"\b(\d+(\.\d+){0,3})\b"); // @"\b(\d+\.\d+\.\d+\.\d+)\b"
                if (versionMatch.Success)
                {
                    var versionString = versionMatch.Groups[1].Value; // "19.0.0.0"
                    var parts = versionString.Split('.').ToList();
                    while (parts.Count < 4) parts.Add("0");
                    versionString = string.Join(".", parts);

                    dbContext.Version = new Version(versionString);
                }
                else
                {
                    throw new InvalidOperationException("Failed to parse the database version.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Error] Unexpected error while defining database version: {ex.Message}");
            }
        }

        internal static string NameTableFromDataTable(string tableName, Dictionary<string, string>? replacesTableName, int limitNameLength)
        {
            if (string.IsNullOrEmpty(tableName) && replacesTableName?.Keys != null)
            {
                foreach (string replace in replacesTableName.Keys)
                {
                    tableName = tableName.Replace(replace, replacesTableName[replace]);
                }
            }

            tableName = tableName.Length > limitNameLength ? tableName.Substring(0, limitNameLength) : tableName;
            tableName = tableName.NormalizeColumnOrTableName(limitNameLength);
            return tableName;
        }

        /// <summary>
        /// Receives rows as a string and returns an array of integers with the first and last row.
        /// </summary>
        internal static int[] DefineRows(string rows, int limitIndexRows)
        {
            List<int> indexRows = new();

            if (string.IsNullOrEmpty(rows) || string.IsNullOrEmpty(rows.Trim())) // If rows not specified
            {
                indexRows.AddRange(Enumerable.Range(1, limitIndexRows)); // Convert all rows                
                return indexRows.ToArray();
            }

            rows = Tools.FixItems(rows); //"1:23,34:-56,23:1,70,75,-1"


            foreach (string row in rows.Split(','))
            {
                if (row.Contains(":")) // "1:23", "34:-56" or "23:1"
                {
                    string[] rowsArray = row.Split(':'); // e.g.: {"A","Z"}

                    if (rowsArray.Length != 2)
                        throw new Exception($"E-0000-EH: Row '{row}' is not a valid pattern!");

                    if (string.IsNullOrEmpty(rowsArray[0])) // If first row not defined
                        rowsArray[0] = "1"; // Then, convert from the first row

                    if (string.IsNullOrEmpty(rowsArray[1])) // If last row not defined
                        rowsArray[1] = limitIndexRows.ToString(); // Then, convert until the last row

                    int firstRowIndex = ConvertIndexRow(rowsArray[0]);
                    int lastRowIndex = ConvertIndexRow(rowsArray[1]);

                    if (firstRowIndex > lastRowIndex)
                        indexRows.AddRange(Enumerable.Range(lastRowIndex, firstRowIndex - lastRowIndex + 1).Reverse());
                    else
                        indexRows.AddRange(Enumerable.Range(firstRowIndex, lastRowIndex - firstRowIndex + 1));
                }
                else // "70", "75" or "-1"
                {
                    indexRows.Add(ConvertIndexRow(row));
                }
            }
            return indexRows.ToArray();


            int ConvertIndexRow(string row)
            {
                if (row.All(c => char.IsLetter(c))) throw new Exception($"E-0000-EH: The row '{row}' is not a valid!");

                int indexRow = Convert.ToInt32(row);

                if (indexRow >= 0)  // "75"
                {
                    if (indexRow == 0 || indexRow > limitIndexRows)
                        throw new Exception($"E-0000-EH: The row '{row}' is out of range (min 1, max {limitIndexRows})!");

                    return indexRow;
                }
                else // "-2"
                {
                    if (limitIndexRows + indexRow + 1 > limitIndexRows)
                        throw new Exception($"E-0000-EH: The row '{row}' is out of range, because it refers to row '{limitIndexRows + indexRow + 1}' (min 1, max {limitIndexRows})!");

                    return limitIndexRows + indexRow + 1;
                }
            }
        }


    }
}
