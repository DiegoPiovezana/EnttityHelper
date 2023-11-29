﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace EH
{
    /// <summary>
    /// Secondary functionality for manipulating list.
    /// </summary>
    internal static class ToolsEH
    {
        internal static Dictionary<string, object> GetProperties<T>(T objectEntity, bool includeNotMapped = false, bool ignoreVirtual = true, bool getFormat = false)
        {
            if (objectEntity == null) { throw new ArgumentNullException(nameof(objectEntity)); }

            PropertyInfo[] properties = objectEntity.GetType().GetProperties();
            Dictionary<string, object> propsDictionary = new();

            foreach (PropertyInfo prop in properties)
            {
                if (prop is null) { continue; }
                if (!includeNotMapped && prop.GetCustomAttribute<NotMappedAttribute>() != null) { continue; }
                if (ignoreVirtual && prop.GetGetMethod().IsVirtual) { continue; }

                //var value = prop.GetValue(objectEntity, null);
                object? value;

                if (getFormat)
                {
                    if (prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>)) // If Nullable
                    {
                        value = Nullable.GetUnderlyingType(prop.PropertyType);
                        //value = prop.PropertyType.UnderlyingSystemType;
                    }
                    else
                    {
                        value = prop.PropertyType;
                    }

                    value = $"{((Type)value).Name}";
                    var maxLengthProp = prop.GetCustomAttribute<MaxLengthAttribute>();
                    if (maxLengthProp != null) { value += $" ({maxLengthProp.Length})"; }
                }
                else
                {
                    value = prop.GetValue(objectEntity, null);

                    if (value != null)
                    {
                        if (prop.PropertyType == typeof(DateTime)) { value = ((DateTime)value).ToString(); } // !
                        if (prop.PropertyType == typeof(decimal)) { value = ((decimal)value).ToString(); } // !
                        if (prop.PropertyType == typeof(bool)) { value = (bool)value ? 1 : 0; }
                    }
                }

                propsDictionary.Add(prop.Name, value);
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
            return obj.GetType().GetProperties().FirstOrDefault(p => Attribute.IsDefined(p, typeof(KeyAttribute)));
        }

        public static TableAttribute? GetTableAttribute(Type type)
        {
            object[] attributes = type.GetCustomAttributes(true);
            foreach (object attribute in attributes) { if (attribute is TableAttribute tbAttribute) { return tbAttribute; } }
            return default;
        }

        public static string GetTable<TEntity>()
        {
            TableAttribute? ta = GetTableAttribute(typeof(TEntity));

            string schema = ta?.Schema != null ? $"{ta.Schema}." : "";
            string tableName = ta?.Name ?? typeof(TEntity).Name; // entity.GetType().Name;

            return $"{schema}{tableName}";
        }

        internal static List<T> MapDataReaderToList<T>(IDataReader reader)
        {
            List<T> list = new();

            while (reader.Read())
            {
                T obj = Activator.CreateInstance<T>();

                foreach (PropertyInfo propInfo in typeof(T).GetProperties())
                {
                    if (propInfo == null || propInfo.GetGetMethod().IsVirtual || propInfo.GetCustomAttribute<NotMappedAttribute>() != null) { continue; }

                    if (!reader.IsDBNull(reader.GetOrdinal(propInfo.Name)))
                    {
                        object value = reader[propInfo.Name];
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

        internal static string GetSqlType(object value)
        {
            if (value is null) { throw new ArgumentNullException(nameof(value)); }

            string sqlType = value.GetType().Name switch
            {
                "String" => "VARCHAR2(255)",
                "Int32" => "NUMBER(10)",
                "Int64" => "NUMBER(19)",
                "Int16" => "NUMBER(5)",
                "Decimal" => "NUMBER(19,4)",
                "Double" => "NUMBER(19,4)",
                "DateTime" => "DATE",
                "Boolean" => "NUMBER(1)",
                _ => throw new ArgumentException("Invalid type!"),
            };

            return sqlType;
        }
    }
}
