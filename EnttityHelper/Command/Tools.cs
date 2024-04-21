using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace EH.Command
{
    internal static class Tools
    {
        public static List<T> MapDataReaderToList<T>(this IDataReader reader, bool matchDb = true)
        {
            try
            {
                List<T> list = new();

                while (reader.Read())
                {
                    T obj = Activator.CreateInstance<T>();

                    foreach (PropertyInfo propInfo in typeof(T).GetProperties())
                    {
                        if (propInfo == null || propInfo.GetGetMethod().IsVirtual || propInfo.GetCustomAttribute<NotMappedAttribute>() != null) { continue; }

                        string nameColumn = propInfo.GetCustomAttribute<ColumnAttribute>()?.Name ?? propInfo.Name;

                        int ordinal;
                        try
                        {
                            ordinal = reader.GetOrdinal(nameColumn);
                        }
                        catch (IndexOutOfRangeException)
                        {
                            Console.WriteLine($"Column '{nameColumn}' not found in table!");

                            if (matchDb) { throw new IndexOutOfRangeException($"Column '{nameColumn}' of '{propInfo.DeclaringType}' not found in table in database!"); }
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

        public static string NormalizeText(string? text, char replaceSpace = '_', bool toLower = true)
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

        public static string FixItems(string items)
        {
            try
            {
                if (!string.IsNullOrEmpty(items))
                {
                    items = items.Replace("\n", ",").Replace(";", ","); // Replace line breaks and semicolons with commas
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


    }
}
