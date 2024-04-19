using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Linq;
using System.Reflection;

namespace EH.Properties
{
    internal static class ToolsEH
    {
        internal static Dictionary<string, Property> GetProperties<T>(T objectEntity, bool includeNotMapped = false, bool ignoreVirtual = true)
        {
            if (objectEntity == null) { throw new ArgumentNullException(nameof(objectEntity)); }

            PropertyInfo[] properties = objectEntity.GetType().GetProperties();
            Dictionary<string, Property> propsDictionary = new();

            foreach (PropertyInfo prop in properties)
            {
                if (prop is null) { continue; }
                if (!includeNotMapped && prop.GetCustomAttribute<NotMappedAttribute>() != null) { continue; }
                if (ignoreVirtual && prop.GetGetMethod().IsVirtual) { continue; }

                Property property = new(prop, objectEntity);


                //if (getFormat)
                //{
                //    object? propType;

                //    if (prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>)) // If Nullable
                //    {
                //        property.IsNullable = true;
                //        propType = Nullable.GetUnderlyingType(prop.PropertyType);
                //    }
                //    else
                //    {
                //        property.IsNullable = false;
                //        propType = prop.PropertyType;
                //    }

                //    property.MaxLength = prop.GetCustomAttribute<MaxLengthAttribute>()?.Length;
                //    property.Type = (Type)propType;
                //}
                //else // Value
                //{
                //    object? value = prop.GetValue(objectEntity, null);
                //    property.Value = value;

                //    if (value != null)
                //    {
                //        if (prop.PropertyType == typeof(DateTime)) { value = ((DateTime)value).ToString(); } // !
                //        else if (prop.PropertyType == typeof(decimal)) { value = ((decimal)value).ToString(); } // !
                //        else if (prop.PropertyType == typeof(bool)) { value = (bool)value ? 1 : 0; }
                //    }

                //    property.ValueSql = value;
                //}

                //property.Name = prop.Name;

                if (string.IsNullOrEmpty(property?.Name)) throw new InvalidOperationException($"Error mapping entity '{nameof(objectEntity)}' property types!");
                propsDictionary.Add(property.ColumnName ?? property.Name, property);
            }

            return propsDictionary;
        }

        /// <summary>
        /// Gets FK entities according to ids.
        /// </summary>   
        internal static Dictionary<object, object> GetFKProperties<T>(T objectEntity)
        {
            if (objectEntity == null) { throw new ArgumentNullException(nameof(objectEntity)); }

            PropertyInfo[] properties = objectEntity.GetType().GetProperties();
            Dictionary<object, object> propertiesId = new();
            Dictionary<object, object> propertiesVirtual = new();
            Dictionary<object, object> propertiesObj = new();

            foreach (PropertyInfo prop in properties)
            {
                if (prop.GetCustomAttribute<NotMappedAttribute>() != null) { continue; }

                if (prop.GetCustomAttribute<ForeignKeyAttribute>() != null)
                {
                    var entityNameFk = prop.GetCustomAttribute<ForeignKeyAttribute>().Name;
                    var idFk = prop.GetValue(objectEntity, null);
                    propertiesId.Add(entityNameFk, idFk);
                }

                if (prop.GetGetMethod().IsVirtual)
                {
                    var obj = Activator.CreateInstance(prop.PropertyType);
                    if (obj != null) propertiesVirtual.Add(prop.Name, obj);
                }
            }

            foreach (var propFkKey in propertiesId.Keys.ToList())
            {
                var propFk = propertiesVirtual[propFkKey];
                propFk.GetType().GetProperty(GetPK(propFk).Name).SetValue(propFk, propertiesId[propFkKey]);
                propertiesObj.Add(propFkKey, propFk);
            }

            return propertiesObj;
        }

        public static PropertyInfo GetPK<T>(T obj) where T : class
        {
            try
            {
                var propPk = obj.GetType().GetProperties().FirstOrDefault(p => Attribute.IsDefined(p, typeof(KeyAttribute)));

                if (propPk is null)
                {
                    if (false) // TODO: Custom Exception List
                    {
                        throw new InvalidOperationException($"No primary key found in '{obj.GetType().Name}'!");
                    }
                    else
                    {
                        propPk = obj.GetType().GetProperties().FirstOrDefault();
                    }
                }

                return propPk;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static TableAttribute? GetTableAttribute(Type type)
        {
            object[] attributes = type.GetCustomAttributes(true);
            foreach (object attribute in attributes) { if (attribute is TableAttribute tbAttribute) { return tbAttribute; } }
            return default;
        }

        public static string GetTableName<TEntity>(Dictionary<string, string>? replacesTableName = null)
        {
            TableAttribute? ta = GetTableAttribute(typeof(TEntity));
            string schema = ta?.Schema != null ? $"{ta.Schema}." : "";
            string tableName = ta?.Name ?? typeof(TEntity).Name;
            if (replacesTableName is not null) tableName = replacesTableName.Aggregate(tableName, (text, replace) => text.Replace(replace.Key, replace.Value));
            return $"{schema}{tableName}";
        }

        public static List<T> MapDataReaderToList<T>(IDataReader reader, bool matchDb = true)
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

    }
}
