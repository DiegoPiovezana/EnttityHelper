using System.Collections.Generic;

namespace EH.Command
{
    internal static class Definitions
    {
        internal static string NameTableFromDataTable(string tableName, Dictionary<string, string>? replacesTableName)
        {
            if (string.IsNullOrEmpty(tableName) && replacesTableName?.Keys != null)
            {
                foreach (string replace in replacesTableName.Keys)
                {
                    tableName = tableName.Replace(replace, replacesTableName[replace]);
                }
            }

            tableName = tableName.Length > 30 ? tableName.Substring(0, 30) : tableName;
            tableName = tableName.NormalizeColumnOrTableName();
            return tableName;
        }









    }
}
