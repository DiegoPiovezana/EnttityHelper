using EH.Command;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Linq;
using System.Reflection;

namespace EH.Properties
{
    internal static class ToolsProp
    {
        internal static Dictionary<string, Property> GetProperties<T>(this T objectEntity, bool ignoreVirtual, bool includeNotMapped)
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
        internal static Dictionary<object, object> GetFKProperties<T>(this T objectEntity)
        {
            if (objectEntity == null) { throw new ArgumentNullException(nameof(objectEntity)); }

            PropertyInfo[] properties = objectEntity.GetType().GetProperties();
            Dictionary<object, object> propertiesFk = new();
            Dictionary<object, object> propertiesVirtual = new();
            Dictionary<object, object> propertiesObj = new();

            foreach (PropertyInfo prop in properties)
            {
                if (prop.GetCustomAttribute<NotMappedAttribute>() != null) { continue; }

                if (prop.GetCustomAttribute<InversePropertyAttribute>() != null) { continue; }

                if (prop.GetCustomAttribute<ForeignKeyAttribute>() != null)
                {
                    var entityNameFk = prop.GetCustomAttribute<ForeignKeyAttribute>().Name;
                    var idFk = prop.GetValue(objectEntity, null);
                    propertiesFk.Add(entityNameFk, idFk);
                }

                if (prop.GetGetMethod().IsVirtual)
                {
                    var obj = Activator.CreateInstance(prop.PropertyType);
                    if (obj != null) propertiesVirtual.Add(prop.Name, obj);
                }
            }

            foreach (var propFkKey in propertiesFk.Keys.ToList())
            {
                var propFk = propertiesVirtual[propFkKey];
                propFk.GetType().GetProperty(GetPK(propFk).Name).SetValue(propFk, propertiesFk[propFkKey]);
                propertiesObj.Add(propFkKey, propFk);
            }

            return propertiesObj;
        }

        /// <summary>
        /// Gets InverseProperty entities.
        /// </summary>   
        internal static List<PropertyInfo>? GetInverseProperties<T>(this T objectEntity)
        {
            try
            {
                if (objectEntity == null) { throw new ArgumentNullException(nameof(objectEntity)); }

                PropertyInfo[] properties = objectEntity.GetType().GetProperties();
                List<PropertyInfo> propertiesInverseProperty = new();

                foreach (PropertyInfo prop in properties)
                {
                    if (prop.GetCustomAttribute<InversePropertyAttribute>() is null) { continue; }
                    propertiesInverseProperty.Add(prop);

                    //IncludeInverseProperties(objectEntity, prop.PropertyType, replacesTableName, eh);
                }

                return propertiesInverseProperty;
            }
            catch (Exception)
            {
                //throw;
                return null; // TODO: throw new InvalidOperationException("Error getting InverseProperty entities.");
            }
        }

        /// <summary>
        /// Gets the primary key of an entity (class or object).
        /// </summary> 
        internal static PropertyInfo GetPK<T>(this T obj) where T : class
        {
            try
            {
                var objType = obj is Type ? obj as Type : obj.GetType();
                var propPk = objType.GetProperties().FirstOrDefault(p => Attribute.IsDefined(p, typeof(KeyAttribute)));

                if (propPk is null)
                {
                    if (false) // TODO: Custom Exception List
                    {
                        throw new InvalidOperationException($"No primary key found in '{objType.Name}'!");
                    }
                    else
                    {
                        propPk = objType.GetProperties().FirstOrDefault();
                    }
                }

                return propPk;
            }
            catch (Exception)
            {
                throw;
            }
        }

        internal static TableAttribute? GetTableAttribute(this Type type)
        {
            //object[] attributes = type.GetCustomAttributes(true);
            //foreach (object attribute in attributes) { if (attribute is TableAttribute tbAttribute) { return tbAttribute; } }
            //return default;

            return type.GetCustomAttributes(typeof(TableAttribute), true).FirstOrDefault() as TableAttribute;
        }

        internal static string GetTableName<TEntity>(Dictionary<string, string>? replacesTableName)
        {
            TableAttribute? ta = GetTableAttribute(typeof(TEntity));
            string schema = ta?.Schema != null ? $"{ta.Schema}." : "";
            string tableName = ta?.Name ?? typeof(TEntity).Name;
            if (replacesTableName is not null) tableName = replacesTableName.Aggregate(tableName, (text, replace) => text.Replace(replace.Key, replace.Value));
            return $"{schema}{tableName}";
        }

        internal static string GetTableName(Type entity, Dictionary<string, string>? replacesTableName)
        {
            if (entity is null) return null;
            TableAttribute? ta = GetTableAttribute(entity);
            string schema = ta?.Schema != null ? $"{ta.Schema}." : "";
            string tableName = ta?.Name ?? entity.Name;
            if (replacesTableName is not null) tableName = replacesTableName.Aggregate(tableName, (text, replace) => text.Replace(replace.Key, replace.Value));
            return $"{schema}{tableName}";
        }

        internal static string GetTableNameManyToMany(Type entity1, Type entity2, Dictionary<string, string>? replacesTableName)
        {
            string tableName1 = GetTableName(entity1, replacesTableName);
            string tableName2 = GetTableName(entity2, replacesTableName);

            if (string.Compare(tableName1, tableName2) < 0)
            {
                (entity2, entity1) = (entity1, entity2);
                (tableName2, tableName1) = (tableName1, tableName2);                
            }

            string tableNameResult = $"{tableName1.ToUpper()}to{entity2.Name.ToUpper()}";

            //string tableName1 = string.Compare(nameTbEntity1, nameTbEntity2) > 0 ? nameTbEntity1 : nameTbEntity2;
            //string tableName2 = string.Compare(nameTbEntity1, nameTbEntity2) > 0 ? nameTbEntity2 : nameTbEntity1;

            //string tableNameResult = tableName1;

            //if (tableNameResult.Length + tableName2.Length <= 30)
            //{
            //string tableNameResult = $"{tableName1.ToUpper()}to{tableName2.ToUpper()}";
            //}
            //else if (tableNameResult.Length + entity2.Name.Length <= 30)
            //{
            //    tableNameResult = $"{tableNameResult.ToUpper()}to{entity2.Name.ToUpper()}";
            //}
            //else
            //{
            //    string entity2Name = entity2.Name.ToUpper();
            //    tableNameResult = $"{tableNameResult.ToUpper()}to{entity2Name}";
            //}

            if (replacesTableName is not null) tableNameResult = replacesTableName.Aggregate(tableNameResult, (text, replace) => text.Replace(replace.Key, replace.Value));
            tableNameResult = tableNameResult.Substring(0, Math.Min(tableNameResult.Length, 30));

            return tableNameResult;
        }






    }
}
