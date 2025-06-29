using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using EH.Properties;

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

                    var properties =
                        typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    
                    foreach (PropertyInfo propInfo in properties)
                    {
                        if (propInfo?.GetGetMethod().IsVirtual != false || propInfo.GetCustomAttribute<NotMappedAttribute>() != null) { continue; }
                        if (ToolsProp.IsFkEntity(propInfo)) { continue; }
                        if (propInfo.PropertyType != typeof(string) && typeof(System.Collections.IEnumerable).IsAssignableFrom(propInfo.PropertyType)) { continue; }

                        string nameColumn = propInfo.GetCustomAttribute<ColumnAttribute>()?.Name ?? propInfo.Name;

                        try
                        {
                            int ordinal = reader.GetOrdinal(nameColumn);
                        }
                        catch (IndexOutOfRangeException)
                        {
                            Debug.WriteLine($"Column '{nameColumn}' not found in table!");

                            if (matchColumn) { throw new IndexOutOfRangeException($"Column '{nameColumn}' of '{propInfo.DeclaringType?.Name}' not found in table in database!"); }
                            else { continue; }
                        }

                        if (!reader.IsDBNull(reader.GetOrdinal(nameColumn)))
                        {
                            object value = reader[nameColumn];
                            Type propType = propInfo.PropertyType;

                            object? convertedValue;

                            // Converte Enums and Nullable
                            if (propType.IsEnum)
                            {
                                if (value is string strValue)
                                {
                                    convertedValue = Enum.Parse(propType, strValue); // string -> enum
                                }
                                else
                                {
                                    Type enumBaseType = Enum.GetUnderlyingType(propType);
                                    object baseValue = Convert.ChangeType(value, enumBaseType);
                                    convertedValue = Enum.ToObject(propType, baseValue); // numeric -> enum
                                }
                            }
                            else if (Nullable.GetUnderlyingType(propType) is Type underlyingType && underlyingType.IsEnum)
                            {
                                convertedValue = Enum.ToObject(underlyingType, value);
                            }
                            else
                            {
                                convertedValue = Convert.ChangeType(value, Nullable.GetUnderlyingType(propType) ?? propType);
                            }

                            var setMethod = propInfo.GetSetMethod(true); // allow private setter

                            if (setMethod != null)
                            {
                                setMethod.Invoke(obj, new[] { convertedValue });
                            }
                            else
                            {
                                // Try with backing field 
                                var backingField = propInfo.DeclaringType?.GetField($"<{propInfo.Name}>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
                                if (backingField != null)
                                {
                                    backingField.SetValue(obj, convertedValue);
                                }
                                else
                                {
                                    throw new Exception($"Property '{propInfo.Name}' has no accessible setter or backing field.");
                                }
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

        public static string NormalizeColumnOrTableName(this string? name, bool adjustInvalidChars = true)
        {
            const int limitChars = 30;
            char[] InvalidCharacters = { ' ', '.', ',', ';', '!', '@', '#', '$', '%', '^', '&', '*', '(', ')', '+', '=', '{', '}', '[', ']', '|', ':', '"', '<', '>', '/', '?', '\\' };
            HashSet<string> ReservedKeywords = new() { "SELECT", "INSERT", "UPDATE", "DELETE", "FROM", "WHERE", "JOIN", "CREATE", "ALTER", "DROP", "TABLE", "COLUMN", "INDEX", "VIEW" };
            char[] AllowedSpecialCharacters = { '_' }; // Allowed characters for table and column names

            name = name.Normalize(false);

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Name cannot be null or empty.", nameof(name));
            }

            StringBuilder normalizedName = new();

            foreach (char c in name)
            {
                if (char.IsLetterOrDigit(c) || Array.Exists(AllowedSpecialCharacters, ch => ch == c))
                {
                    normalizedName.Append(c);
                }
                else
                {
                    if (adjustInvalidChars)
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

            //if (result.Any(c => InvalidCharacters.Contains(c)))
            //{
            //    throw new ArgumentException($"Name contains invalid characters. Allowed characters are alphanumeric and underscore.", nameof(result));
            //}

            if (!char.IsLetter(name[0]) && name[0] != '_')
            {
                if (adjustInvalidChars) result = "c_" + result;
                else throw new ArgumentException("Name must start with a letter or an underscore.");
            }

            if (ReservedKeywords.Contains(result.ToUpper()))
            {
                if (adjustInvalidChars) result = "c_" + result;
                else throw new ArgumentException("Name cannot be a reserved keyword.", nameof(result));
            }

            //if (result.Length > limitChars)
            //{
            //    throw new ArgumentException($"Name cannot exceed {limitChars} characters.", nameof(result));
            //}

            result = result.Length > limitChars ? result.Substring(0, limitChars) : result;
            return result;
        }

        public static string[] ParseCsvLine(string line, char delimiter)
        {
            if (string.IsNullOrEmpty(line)) return Array.Empty<string>();

            var fields = new List<string>();
            var currentField = new StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (c == '"' && !inQuotes)
                {
                    inQuotes = true;
                }
                else if (c == '"' && inQuotes)
                {
                    if (i + 1 < line.Length && line[i + 1] == '"')
                    {
                        currentField.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = false;
                    }
                }
                else if (c == delimiter && !inQuotes)
                {
                    fields.Add(currentField.ToString().Trim());
                    currentField.Clear();
                }
                else
                {
                    currentField.Append(c);
                }
            }

            fields.Add(currentField.ToString().Trim());
            return fields.ToArray();
        }



    }
}
