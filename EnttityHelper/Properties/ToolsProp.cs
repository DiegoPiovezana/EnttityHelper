using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;

namespace EH.Properties
{
    internal static class ToolsProp
    {
        internal static Dictionary<string, Property> GetProperties<T>(this T objectEntity, bool includeNotMapped = false, bool ignoreVirtual = true)
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

        internal static PropertyInfo GetPK<T>(this T obj) where T : class
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

        internal static TableAttribute? GetTableAttribute(this Type type)
        {
            //object[] attributes = type.GetCustomAttributes(true);
            //foreach (object attribute in attributes) { if (attribute is TableAttribute tbAttribute) { return tbAttribute; } }
            //return default;

            return type.GetCustomAttributes(typeof(TableAttribute), true).FirstOrDefault() as TableAttribute;
        }

        internal static string GetTableName<TEntity>(Dictionary<string, string>? replacesTableName = null)
        {
            TableAttribute? ta = GetTableAttribute(typeof(TEntity));
            string schema = ta?.Schema != null ? $"{ta.Schema}." : "";
            string tableName = ta?.Name ?? typeof(TEntity).Name;
            if (replacesTableName is not null) tableName = replacesTableName.Aggregate(tableName, (text, replace) => text.Replace(replace.Key, replace.Value));
            return $"{schema}{tableName}";
        }

        internal static string GetTableNameManyToMany(object entity1, object entity2, Dictionary<string, string>? replacesTableName = null)
        {
            var type1 = entity1.GetType();
            TableAttribute? ta1 = GetTableAttribute(type1);
            string schema1 = ta1?.Schema != null ? $"{ta1.Schema}." : "";
            string tableName1 = ta1?.Name ?? type1.Name;
            string tb = $"{schema1}{tableName1}";

            var type2 = entity2.GetType();
            TableAttribute? ta2 = GetTableAttribute(type2);
            string tableName2 = ta2?.Name ?? type2.Name;

            if (tb.Length + tableName2.Length <= 30)
            {
                tb = $"{tb.ToUpper()}to{tableName2.ToUpper()}";
            }
            else if (tb.Length + type2.Name.Length <= 30)
            {
                tb = $"{tb.ToUpper()}to{type2.Name.ToUpper()}";
            }
            else
            {
                string entity2Name = type2.Name.ToUpper();
                tb = $"{tb.ToUpper()}to{entity2Name}";
                tb = tb.Substring(0, Math.Min(tb.Length, 20));
            }

            if (replacesTableName is not null) tb = replacesTableName.Aggregate(tb, (text, replace) => text.Replace(replace.Key, replace.Value));
            
            return tb;
        }






    }
}
