using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Linq;
using System.Reflection;
using EH.Connection;

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
            Dictionary<object, object> propertiesIdFk = new();
            Dictionary<object, object> propertiesEntityFk = new();
            Dictionary<object, object> propertiesObj = new();

            foreach (PropertyInfo prop in properties)
            {
                if (prop.GetCustomAttribute<NotMappedAttribute>() != null)
                {
                    continue;
                }

                if (prop.GetCustomAttribute<InversePropertyAttribute>() != null)
                {
                    continue;
                }

                if (prop.PropertyType != typeof(string)
                    && typeof(System.Collections.IEnumerable).IsAssignableFrom(prop.PropertyType))
                {
                    continue; // If is collection, skip
                }

                // If is FK Id
                string? entityNameFk = prop.GetFkEntityNameById();
                if (entityNameFk is not null)
                {
                    object? idFk = prop.GetValue(objectEntity, null);
                    propertiesIdFk.Add(entityNameFk, idFk);
                }

                if (prop.IsFkEntity())
                {
                    object? obj = Activator.CreateInstance(prop.PropertyType);
                    if (obj != null) propertiesEntityFk.Add(prop.Name, obj);
                }
            }

            foreach (var propFkKey in propertiesIdFk.Keys.ToList())
            {
                if(propertiesIdFk[propFkKey] == null) { continue; } // If PropFk not assigned
                var propFk = propertiesEntityFk[propFkKey];
                propFk.GetType().GetProperty(GetPK(propFk).Name).SetValue(propFk, propertiesIdFk[propFkKey]);
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
                throw new InvalidOperationException("Error getting InverseProperty entities.");
            }
        }
        
        internal static List<PropertyInfo>? GetCollecionProperties<T>(this T objectEntity)
        {
            try
            {
                if (objectEntity == null) { throw new ArgumentNullException(nameof(objectEntity)); }

                PropertyInfo[] properties = objectEntity.GetType().GetProperties();
                List<PropertyInfo> propertiesCollection = new();

                foreach (PropertyInfo prop in properties)
                {
                    // If is a collection property
                    if (prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(ICollection<>))
                    {
                        Type entityType = prop.PropertyType.GetGenericArguments()[0];
                        if (entityType.IsClass && !entityType.IsAbstract) // If is a class and not abstract
                        {
                            propertiesCollection.Add(prop);
                        }
                    }
                }

                return propertiesCollection;
            }
            catch (Exception)
            {
                 throw new InvalidOperationException("Error getting Collection entities.");
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
                    propPk = objType.GetProperties().FirstOrDefault();
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

        internal static string GetTableName<TEntity>(Dictionary<string, string>? replacesTableName, bool includeSchema = true)
        {
            return GetTableName(typeof(TEntity), replacesTableName, includeSchema);
        }

        internal static string GetTableName(Type entityType, Dictionary<string, string>? replacesTableName, bool includeSchema = true)
        {
            if (entityType is null) return null;
            TableAttribute? ta = GetTableAttribute(entityType);
            string schema = includeSchema && !string.IsNullOrWhiteSpace(ta?.Schema) ? $"{ta.Schema}." : string.Empty;
            string tableName = ta?.Name ?? entityType.Name;
            if (replacesTableName is not null) tableName = replacesTableName.Aggregate(tableName, (text, replace) => text.Replace(replace.Key, replace.Value));
            return $"{schema}{tableName}";
        }

        internal static string GetTableNameManyToMany(Type entity1Type, PropertyInfo prop2Collection, Dictionary<string, string>? replacesTableName)
        {
            Type collection2Type = prop2Collection.PropertyType;
            Type entity2Type = collection2Type.GetGenericArguments()[0];

            IEnumerable<CustomAttributeData>? attributeProp1Collection = prop2Collection.CustomAttributes.Where(a => a.AttributeType.Name == "InversePropertyAttribute");
            CustomAttributeTypedArgument customAttribute = attributeProp1Collection.FirstOrDefault().ConstructorArguments[0];
            string nameProp2Collection = customAttribute.Value.ToString();

            PropertyInfo? prop1Collection = null; // Entity 1 in entity 2
            if (!string.IsNullOrEmpty(nameProp2Collection)) { prop1Collection = entity2Type.GetProperty(nameProp2Collection); }

            string nameProp1Collection = prop2Collection.Name;

            string tableName1 = GetTableName(entity1Type, replacesTableName);
            string tableName2 = GetTableName(entity2Type, replacesTableName);

            if (string.Compare(nameProp1Collection, nameProp2Collection) < 0)
            {
                (entity2Type, entity1Type) = (entity1Type, entity2Type);
                (tableName2, tableName1) = (tableName1, tableName2);
                (nameProp2Collection, nameProp1Collection) = (nameProp1Collection, nameProp2Collection);
            }

            string tableNameResult = $"{tableName1.ToUpper()}to{nameProp2Collection.ToUpper()}";            

            if (replacesTableName is not null) tableNameResult = replacesTableName.Aggregate(tableNameResult, (text, replace) => text.Replace(replace.Key, replace.Value));
            return tableNameResult.Substring(0, Math.Min(tableNameResult.Length, 30));
        }
        
        internal static string GetTypeSql(Type realType, Database dbContext)
        {
            return dbContext.TypesDefault?.FirstOrDefault(x => x.Key == realType.Name).Value;
        }

        /// <summary>
        /// If is a foreign key property, returns the name of the entity it refers to.
        /// </summary>
        internal static string? GetFkEntityNameById(this PropertyInfo propertyInfo)
        {
            if (propertyInfo.GetCustomAttribute<ForeignKeyAttribute>() != null)
                return propertyInfo.GetCustomAttribute<ForeignKeyAttribute>().Name;

            if (propertyInfo.Name.Length > 2)
            {
                if (propertyInfo.Name.EndsWith("Id")) // Ex: OrderId, ItemId, etc.
                    return propertyInfo.Name.Substring(0, propertyInfo.Name.Length - 2);

                if (propertyInfo.Name.StartsWith("Id")) // Ex: IdOrder, IdItem, etc.
                    return propertyInfo.Name.Substring(2);
            }

            return null;
        }
        
        internal static string? GetNameIdFk1(Type entityType, string navigationPropertyName)
        {
            if (entityType == null)
                throw new ArgumentNullException(nameof(entityType));
            
            if (string.IsNullOrEmpty(navigationPropertyName))
                throw new ArgumentException("Navigation property name cannot be null or empty",
                    nameof(navigationPropertyName));
            
            // #1: search directly for a property named "{NavigationPropertyName}Id" or "Id{NavigationPropertyName}"
            var directMatch = entityType.GetProperties()
                .FirstOrDefault(p => 
                    p.Name.Equals($"{navigationPropertyName}Id", StringComparison.OrdinalIgnoreCase) ||
                    p.Name.Equals($"Id{navigationPropertyName}", StringComparison.OrdinalIgnoreCase)
                    );
    
            if (directMatch != null)
                return directMatch.Name;
            
            // #2: search for a ForeignKey attribute on the navigation property
            var navigationProperty = entityType.GetProperty(navigationPropertyName);
            if (navigationProperty != null)
            {
                var foreignKeyAttr = navigationProperty.GetCustomAttribute<ForeignKeyAttribute>();
                if (foreignKeyAttr != null)
                    return foreignKeyAttr.Name;
            }

            // #3: search for properties that end with "Id" or start with "Id"
            var fkProperties = entityType.GetProperties()
                .Where(p => (p.Name.EndsWith("Id") || p.Name.StartsWith("Id")) 
                            && (p.PropertyType.IsValueType || p.PropertyType == typeof(string))
                            )
                .ToList();

            // #3.1: check if any FK property matches the navigation property name
            var partialMatch = fkProperties.FirstOrDefault(p =>
                p.Name.StartsWith(navigationPropertyName, StringComparison.OrdinalIgnoreCase));

            if (partialMatch != null)
                return partialMatch.Name;

            // #3.2: check if any FK property has a ForeignKey attribute that matches the navigation property name
            foreach (var fkProp in fkProperties)
            {
                var fkAttr = fkProp.GetCustomAttribute<ForeignKeyAttribute>();
                if (fkAttr != null && fkAttr.Name.Equals(navigationPropertyName, StringComparison.OrdinalIgnoreCase))
                    return fkProp.Name;
            }

            // If no FK property found, return null
            return null;
        }

      
        internal static bool IsFkEntity(this PropertyInfo propertyInfo)
        {
            return propertyInfo.Name.Equals(propertyInfo.PropertyType.Name);
            // || (propertyInfo.GetGetMethod()?.IsVirtual ?? false) == true;
        }
        
        internal static DbType MapToDbType(this Type type)
        {
            type = Nullable.GetUnderlyingType(type) ?? type;

            if (type == typeof(string)) return DbType.String;
            if (type == typeof(int)) return DbType.Int32;
            if (type == typeof(long)) return DbType.Int64;
            if (type == typeof(Guid)) return DbType.Guid;
            if (type == typeof(Byte[])) return DbType.Binary;
            if (type == typeof(DateTime)) return DbType.DateTime;
            if (type == typeof(bool)) return DbType.Boolean;
            if (type == typeof(double)) return DbType.Double;
            if (type == typeof(float)) return DbType.Single;
            if (type.IsEnum) return DbType.Int32;

            throw new NotSupportedException($"Type not supported for map with DbType: {type.FullName}");
        }


    }
}
