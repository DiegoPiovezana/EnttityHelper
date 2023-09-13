using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
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
            if(objectEntity == null) { throw new ArgumentNullException(nameof(objectEntity)); }

            PropertyInfo[] properties = objectEntity.GetType().GetProperties();
            Dictionary<string, object> propertiesDictionary = new ();

            foreach (PropertyInfo prop in properties)
            {
                //Console.WriteLine($"Propriedade: {prop.Name}");
                //Console.WriteLine($"Valor: {prop.GetValue(objectEntity, null)}");

                if (!includeNotMapped && prop.GetCustomAttribute<NotMappedAttribute>() != null) { continue; }
                if (ignoreVirtual && prop.GetGetMethod().IsVirtual) { continue; }

                var value = prop.GetValue(objectEntity, null);

                if (prop.PropertyType == typeof(bool)) { value = (bool)value ? 1 : 0; }
                propertiesDictionary.Add(prop.Name, value);
            }

            return propertiesDictionary;
        }

        internal static TAttribute? GetAttributeByName<TAttribute>(Type type, string attributeName) where TAttribute : Attribute
        {
            var attributes = type.GetCustomAttributes(typeof(TAttribute), true);

            return type.GetCustomAttributes(typeof(TAttribute), true)
                .FirstOrDefault(attr => attr.GetType().Name == attributeName) as TAttribute;
        }

        public static TValue? GetAttributeValue<TAttribute, TValue>(this Type type, Func<TAttribute, TValue> valueSelector) where TAttribute : Attribute
        {
            if (type.GetCustomAttributes(typeof(TAttribute), true).FirstOrDefault() is TAttribute att)
            {
                return valueSelector(att);
            }

            return default;
        }

        public static TableAttribute? GetTableAttribute(Type type)
        {
            object[] attributes = type.GetCustomAttributes(true);
            foreach (object attribute in attributes) { if (attribute is TableAttribute tbAttribute) { return tbAttribute; } }
            return default;
        }

        public static string GetPropertyName<T>(Expression<Func<T>> expression)
        {
            var memberExpression = (MemberExpression)expression.Body;
            return memberExpression.Member.Name;
        }

        public static object GetPropertyValue(object obj, string propertyName)
        {
            PropertyInfo propertyInfo = obj.GetType().GetProperty(propertyName);
            return propertyInfo.GetValue(obj);
        }

        public static string GetTable<TEntity>()
        {
            TableAttribute ta = GetTableAttribute(typeof(TEntity));

            string schema = ta?.Schema != null ? $"{ta.Schema}." : "";
            string tableName = ta?.Name ?? typeof(TEntity).Name; // entity.GetType().Name;

            return $"{schema}{tableName}";
        }

        internal static List<T> MapDataReaderToList1<T>(IDataReader reader)
        {
            List<T> list = new ();

            while (reader.Read())
            {
                T obj = Activator.CreateInstance<T>();

                for (int i = 0; i < reader.FieldCount; i++)
                {
                    string propertyName = reader.GetName(i);
                    object value = reader[i];

                    var property = typeof(T).GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    if (property != null && value != DBNull.Value)
                    {
                        property.SetValue(obj, Convert.ChangeType(value, property.PropertyType), null);
                    }
                }

                list.Add(obj);
            }

            return list;
        }

        internal static List<T> MapDataReaderToList2<T>(IDataReader reader)
        {
            List<T> list = new (reader.FieldCount);

            while (reader.Read())
            {
                T obj = Activator.CreateInstance<T>();

                foreach (PropertyInfo propInfo in typeof(T).GetProperties())
                {
                    if (!reader.IsDBNull(reader.GetOrdinal(propInfo.Name)))
                    {
                        object value = reader[propInfo.Name];
                        propInfo.SetValue(obj, Convert.ChangeType(value, propInfo.PropertyType));
                    }
                }

                list.Add(obj);
            }

            return list;
        }



    }
}
