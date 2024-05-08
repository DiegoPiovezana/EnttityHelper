using EH.Command;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace EH.Properties
{
    internal static class ToolsProp
    {
        internal static Dictionary<string, Property> GetProperties<T>(this T objectEntity, bool ignoreVirtual = true, bool includeNotMapped = false)
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
        internal static Dictionary<object, object>? GetInverseProperties<T>(this T objectEntity, EnttityHelper eh)
        {
            try
            {
                if (objectEntity == null) { throw new ArgumentNullException(nameof(objectEntity)); }

                PropertyInfo[] properties = objectEntity.GetType().GetProperties();
                Dictionary<object, object> propertiesInverseProperty = new();

                foreach (PropertyInfo prop in properties)
                {
                    if (prop.GetCustomAttribute<InversePropertyAttribute>() is null) { continue; }

                    Type collectionType = prop.PropertyType;
                    if (collectionType.IsGenericType && collectionType.GetGenericTypeDefinition() == typeof(ICollection<>))
                    {
                        Type entityType = collectionType.GetGenericArguments()[0];

                        if (entityType.IsClass && !entityType.IsAbstract)
                        {
                            Type listType = typeof(List<>).MakeGenericType(entityType);
                            var listInstance = Activator.CreateInstance(listType);

                            Features features = new(eh);
                            var getMethod = typeof(Features).GetMethod("Get").MakeGenericMethod(entityType);
                            //var entitiesToAdd = (IEnumerable<object>)getMethod.Invoke(features, new object[] { true, $"{prop.GetCustomAttribute<InversePropertyAttribute>().Property}='{prop.GetValue(objectEntity)}'", null });

                            //var selectMethod = features.GetType().GetMethod("ExecuteSelect").MakeGenericMethod(entityType);

                            string nameTable = GetTableNameManyToMany(GetTableName<T>(), entityType);
                            string columnName1 = GetTableName(objectEntity.GetType());

                            string idName1 = GetPK((object)objectEntity).Name;
                            PropertyInfo idProp1 = objectEntity.GetType().GetProperty(idName1);
                            object idValue1 = idProp1.GetValue(objectEntity);

                            var entitiesToAdd = (IEnumerable<object>)getMethod.Invoke(features, new object[] { false, $"ID_{columnName1}='{idValue1}'", nameTable }); // ID_{pkEntity1}1

                            var addMethod = collectionType.GetMethod("Add");

                            foreach (var item in entitiesToAdd)
                            {
                                addMethod.Invoke(listInstance, new object[] { item });
                            }

                            //var collectionInstance = (ICollection<object>)prop.GetValue(objectEntity);
                            //var collectionInstance = Activator.CreateInstance(entityType);
                            //var collectionGroupInstance = (ICollection<Group>)listInstance;
                            var collectionInstance = (ICollection<object>)listInstance;

                            propertiesInverseProperty.Add(prop.Name, collectionInstance);
                        }
                    }
                }

                return propertiesInverseProperty;


                //if (prop.GetCustomAttribute<ForeignKeyAttribute>() is null)
                //    {
                //        var entityNameFk = prop.GetCustomAttribute<ForeignKeyAttribute>().Name;
                //        var idFk = prop.GetValue(objectEntity, null);
                //        propertiesInverseProperty.Add(entityNameFk, idFk);
                //    }

                //    if (prop.GetGetMethod().IsVirtual)
                //    {
                //        var obj = Activator.CreateInstance(prop.PropertyType);
                //        if (obj != null) propertiesInverseProperty.Add(prop.Name, obj);
                //    }
                //}

                //foreach (var propFkKey in propertiesInverseProperty.Keys.ToList())
                //{
                //    var propFk = propertiesVirtual[propFkKey];
                //    propFk.GetType().GetProperty(GetPK(propFk).Name).SetValue(propFk, propertiesInverseProperty[propFkKey]);
                //    propertiesObj.Add(propFkKey, propFk);
                //}

                //return propertiesObj;
            }
            catch (Exception)
            {
                //throw;
                return null;
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

        internal static string GetTableName<TEntity>(Dictionary<string, string>? replacesTableName = null)
        {
            TableAttribute? ta = GetTableAttribute(typeof(TEntity));
            string schema = ta?.Schema != null ? $"{ta.Schema}." : "";
            string tableName = ta?.Name ?? typeof(TEntity).Name;
            if (replacesTableName is not null) tableName = replacesTableName.Aggregate(tableName, (text, replace) => text.Replace(replace.Key, replace.Value));
            return $"{schema}{tableName}";
        }

        internal static string GetTableName(Type entity, Dictionary<string, string>? replacesTableName = null)
        {
            if (entity is null) return null;
            TableAttribute? ta = GetTableAttribute(entity);
            string schema = ta?.Schema != null ? $"{ta.Schema}." : "";
            string tableName = ta?.Name ?? entity.Name;
            if (replacesTableName is not null) tableName = replacesTableName.Aggregate(tableName, (text, replace) => text.Replace(replace.Key, replace.Value));
            return $"{schema}{tableName}";
        }

        internal static string GetTableNameManyToMany(string nameTbEntity1, Type entity2, Dictionary<string, string>? replacesTableName = null)
        {
            string tb = nameTbEntity1;
            //TableAttribute? ta2 = GetTableAttribute(entity2);
            //string tableName2 = ta2?.Name ?? entity2.Name;

            string tableName2 = GetTableName(entity2);

            if (tb.Length + tableName2.Length <= 30)
            {
                tb = $"{tb.ToUpper()}to{tableName2.ToUpper()}";
            }
            else if (tb.Length + entity2.Name.Length <= 30)
            {
                tb = $"{tb.ToUpper()}to{entity2.Name.ToUpper()}";
            }
            else
            {
                string entity2Name = entity2.Name.ToUpper();
                tb = $"{tb.ToUpper()}to{entity2Name}";
            }

            if (replacesTableName is not null) tb = replacesTableName.Aggregate(tb, (text, replace) => text.Replace(replace.Key, replace.Value));
            tb = tb.Substring(0, Math.Min(tb.Length, 30));

            return tb;
        }






    }
}
