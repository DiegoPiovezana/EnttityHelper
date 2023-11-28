using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Linq;
using System.Reflection;

namespace EH
{
    /// <summary>
    /// Secondary functionality for manipulating list.
    /// </summary>
    internal static class ToolsEH
    {
        internal static Dictionary<string, object> GetProperties<T>(T objectEntity, bool includeNotMapped = false, bool ignoreVirtual = true, bool includeFormat = false)
        {
            if (objectEntity == null) { throw new ArgumentNullException(nameof(objectEntity)); }

            PropertyInfo[] properties = objectEntity.GetType().GetProperties();
            Dictionary<string, object> propertiesDictionary = new();

            foreach (PropertyInfo prop in properties)
            {
                if (!includeNotMapped && prop.GetCustomAttribute<NotMappedAttribute>() != null) { continue; }
                if (ignoreVirtual && prop.GetGetMethod().IsVirtual) { continue; }

                var value = prop.GetValue(objectEntity, null);

                if (prop.PropertyType == typeof(bool)) { value = (bool)value ? 1 : 0; }
                propertiesDictionary.Add(prop.Name, value);
            }

            return propertiesDictionary;
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
    }
}
