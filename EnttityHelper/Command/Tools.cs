﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace EH.Command
{
    internal static class Tools
    {
        /// <summary>
        /// Maps the data from an IDataReader to a list of entities of type T.     
        /// </summary>
        /// <typeparam name="T">The type of entity.</typeparam>
        /// <param name="reader">The IDataReader containing the data to be mapped.</param>
        /// <param name="matchColumn">Optional parameter indicating whether to match the column names to the entity properties. Default is true.</param>
        /// <returns>A list of entities of type T mapped from the data in the IDataReader.</returns>
        public static List<T> ToListEntity<T>(this IDataReader? reader, bool matchColumn = true)
        {
            try
            {
                if (reader == null) { throw new ArgumentNullException(nameof(reader)); }

                List<T> list = new();

                while (reader.Read())
                {
                    T obj = Activator.CreateInstance<T>();

                    foreach (PropertyInfo propInfo in typeof(T).GetProperties())
                    {
                        if (propInfo == null || propInfo.GetGetMethod().IsVirtual || propInfo.GetCustomAttribute<NotMappedAttribute>() != null) { continue; }

                        string nameColumn = propInfo.GetCustomAttribute<ColumnAttribute>()?.Name ?? propInfo.Name;

                        try
                        {
                            int ordinal = reader.GetOrdinal(nameColumn);
                        }
                        catch (IndexOutOfRangeException)
                        {
                            Debug.WriteLine($"Column '{nameColumn}' not found in table!");

                            if (matchColumn) { throw new IndexOutOfRangeException($"Column '{nameColumn}' of '{propInfo.DeclaringType.Name}' not found in table in database!"); }
                            else { continue; }
                        }

                        if (!reader.IsDBNull(reader.GetOrdinal(nameColumn)))
                        {
                            object value = reader[nameColumn];
                            Type propType = propInfo.PropertyType;

                            if (propType.IsGenericType && propType.GetGenericTypeDefinition() == typeof(Nullable<>))
                            {
                                if (value != null) { propInfo.SetValue(obj, Convert.ChangeType(value, Nullable.GetUnderlyingType(propType))); }
                            }
                            else
                            {
                                propInfo.SetValue(obj, Convert.ChangeType(value, propType));
                            }
                        }
                    }

                    list.Add(obj);
                }

                return list;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Converts an IDataReader to a DataTable.
        /// </summary>
        /// <param name="reader">The IDataReader to be converted.</param>
        /// <returns>A DataTable containing the data from the IDataReader.</returns>
        public static DataTable ToDataTable(this IDataReader? reader)
        {
            DataTable dtResult = new();
            dtResult.Load(reader);
            return dtResult;
        }

        /// <summary>
        /// Converts an enumerable collection of objects of type <typeparamref name="T"/> into a DataTable.
        /// The DataTable columns are created based on the public properties of the type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of objects in the enumerable collection.</typeparam>
        /// <param name="items">The enumerable collection of objects to be converted to a DataTable.</param>
        /// <returns>A DataTable with rows representing each object in the collection and columns corresponding to the public properties of type <typeparamref name="T"/>.</returns>
        public static DataTable ToDataTable<T>(this IEnumerable<T> items)
        {
            if (items is null) return new DataTable();

            DataTable dataTable = new(typeof(T).Name);

            PropertyInfo[] props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (PropertyInfo prop in props)
            {
                dataTable.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
            }

            dataTable.BeginLoadData(); // Suspend internal operations

            foreach (T item in items)
            {
                DataRow row = dataTable.NewRow();

                foreach (PropertyInfo prop in props)
                {
                    row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
                }

                dataTable.Rows.Add(row);
            }

            dataTable.EndLoadData(); // Resumes internal operations

            return dataTable;
        }

        public static DataTable GetFirstRows(this IDataReader reader, int rowCount)
        {
            DataTable dataTable = new();

            for (int i = 0; i < reader.FieldCount; i++)
            {
                dataTable.Columns.Add(reader.GetName(i), reader.GetFieldType(i));
            }

            for (int count = 0; reader.Read() && count < rowCount; count++)
            {
                DataRow row = dataTable.NewRow();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    row[i] = reader[i];
                }
                dataTable.Rows.Add(row);
            }

            return dataTable;
        }

        /// <summary>
        /// Fixes the format of the items string by replacing line breaks, semicolons, spaces, single quotes, and double quotes with commas.
        /// Removes repeated commas and excess spaces.
        /// </summary>
        /// <param name="items">The string containing the items.</param>
        /// <returns>The fixed items string.</returns>
        public static string? FixItems(this string? items)
        {
            try
            {
                if (!string.IsNullOrEmpty(items))
                {
                    items = items?.Replace("\n", ",").Replace(";", ","); // Replace line breaks and semicolons with commas
                    items = Regex.Replace(items, @"\s+|['""]+", ""); // Remove spaces, single quotes, and double quotes
                    items = Regex.Replace(items, ",{2,}", ",").Trim(','); // Remove repeated commas and excess spaces
                }
                return items; // "123123,13514,31234"
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static string? FixValueToString(this object value)
        {
            if (value is string vString)
            {
                if (string.IsNullOrEmpty(vString) || !vString.Contains("'")) { return vString; }
                return vString.Replace("'''", "'").Replace("'", "''");
            }
            return "NULL";
        }


        /// <summary>
        /// Normalizes the input text by removing diacritics and replacing spaces with the specified character.
        /// </summary>
        /// <param name="text">The input text to be normalized.</param>
        /// <param name="toLower">(Optional) Indicates whether the result should be converted to lowercase. Default is true.</param>
        /// <param name="replaceSpace">(Optional) The character used to replace spaces. Default is underscore ('_').</param>
        /// <returns>The normalized string.</returns>
        public static string Normalize(this string? text, bool toLower = true, char replaceSpace = '_')
        {
            try
            {
                if (string.IsNullOrEmpty(text?.Trim())) return "";

                string normalizedString = text.Trim().Normalize(NormalizationForm.FormD);
                StringBuilder stringBuilder = new();

                foreach (char c in normalizedString)
                {
                    UnicodeCategory unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                    if (unicodeCategory != UnicodeCategory.NonSpacingMark) { stringBuilder.Append(c); }
                }

                if (toLower) return stringBuilder.ToString().Normalize(NormalizationForm.FormC).Replace(' ', replaceSpace).ToLower();
                return stringBuilder.ToString().Normalize(NormalizationForm.FormC).Replace(' ', replaceSpace);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static string NormalizeColumnOrTableName(this string? name, bool replaceInvalidChars = true)
        {
            const int limitChars = 30;
            char[] InvalidCharacters = { ' ', '.', ',', ';', '!', '@', '#', '$', '%', '^', '&', '*', '(', ')', '+', '=', '{', '}', '[', ']', '|', ':', '"', '<', '>', '/', '?', '\\' };
            HashSet<string> ReservedKeywords = new() { "SELECT", "INSERT", "UPDATE", "DELETE", "FROM", "WHERE", "JOIN", "CREATE", "ALTER", "DROP", "TABLE", "COLUMN", "INDEX", "VIEW" };
            char[] AllowedSpecialCharacters = { '_', '$' }; // Allowed characters for table and column names

            name = name.Normalize(false);

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Name cannot be null or empty.", nameof(name));
            }

            StringBuilder normalizedName = new();

            // Check if the name starts with a letter or underscore
            if (!char.IsLetter(name[0]) && name[0] != '_')
            {
                throw new ArgumentException("Name must start with a letter or an underscore.");
            }

            foreach (char c in name)
            {
                if (char.IsLetterOrDigit(c) || Array.Exists(AllowedSpecialCharacters, ch => ch == c))
                {
                    normalizedName.Append(c);
                }
                else
                {
                    if (replaceInvalidChars)
                    {
                        // Replace invalid character with an underscore
                        normalizedName.Append('_');
                    }
                    else
                    {
                        throw new ArgumentException($"Invalid character '{c}' detected in name.");
                    }
                }
            }

            string result = normalizedName.ToString();

            //if (result.Length > limitChars) // Adjust the length limit as necessary
            //{                
            //    throw new ArgumentException($"Name cannot exceed {limitChars} characters.", nameof(result));
            //}

            //if (result.Any(c => InvalidCharacters.Contains(c)))
            //{
            //    throw new ArgumentException($"Name contains invalid characters. Allowed characters are alphanumeric and underscore.", nameof(result));
            //}

            if (ReservedKeywords.Contains(result.ToUpper()))
            {
                throw new ArgumentException("Name cannot be a reserved keyword.", nameof(result));

            }

            result = result.Length > 30 ? result.Substring(0, 30) : result;

            return result;
        }


    }
}
