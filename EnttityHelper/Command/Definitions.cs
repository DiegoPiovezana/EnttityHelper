using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

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
