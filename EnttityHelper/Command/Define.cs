using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EH.Command
{
    internal class Define
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
            tableName = Tools.NormalizeText(tableName, '_', false);
            return tableName;
        }









    }
}
