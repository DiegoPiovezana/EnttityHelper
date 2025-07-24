using EH.Command;
using EH.Properties;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using EH.Connection;

namespace EH.Entities
{
    internal class Inclusions
    {
        private readonly EnttityHelper _enttityHelper;

        public Inclusions(EnttityHelper enttityHelper)
        {
            _enttityHelper = enttityHelper;
        }

        internal void IncludeForeignKeyEntities<TEntity>(TEntity entity, string? fkNameOnly = null)
        {
            if (entity == null) return;

            var propertiesFK = ToolsProp.GetFKProperties(entity);
            if (propertiesFK == null || propertiesFK.Count == 0)
            {
                Debug.WriteLine($"No foreign key properties found in '{entity}'.");
                return;
            }

            if (!string.IsNullOrEmpty(fkNameOnly)) // If not all
            {
                propertiesFK = propertiesFK.Where(x => x.Key.ToString() == fkNameOnly).ToDictionary(x => x.Key, x => x.Value);
            }

            foreach (KeyValuePair<object, object> pair in propertiesFK)
            {
                if (pair.Value != null)
                {
                    var pk = ToolsProp.GetPK(pair.Value);
                    if (pk == null) continue;

                    var propertyToModify = entity.GetType().GetProperty(pair.Key.ToString());

                    if (propertyToModify != null)
                    {
                        var pkValue = pk.GetValue(pair.Value, null);
                        if (pkValue == null || string.IsNullOrEmpty(pkValue.ToString().Trim()) || pk.PropertyType.IsPrimitive && pkValue.Equals(Convert.ChangeType(0, pkValue.GetType()))) continue;

                        // Get the property type of the foreign key
                        Type? fkEntityType = propertyToModify.PropertyType;

                        // Check if it is a generic collection type
                        bool isCollection = typeof(ICollection<>).IsAssignableFrom(fkEntityType);

                        // Get the actual type of the elements in the collection (if applicable)
                        Type elementType = isCollection ? fkEntityType.GetGenericArguments()[0] : fkEntityType;

                        // Use the correct generic type for the Get method
                        MethodInfo genericGetMethod = typeof(EnttityHelper).GetMethod("Get").MakeGenericMethod(elementType);

                        // Retrieve the foreign key entities
                        IEnumerable<object> entityFKList = (IEnumerable<object>)genericGetMethod.Invoke(_enttityHelper, new object[] { false, $"{pk.Name}='{pkValue}'", null, null, 0, null, true });

                        // Cast each entity to the actual type
                        IEnumerable<object> castEntityFKList = entityFKList.Cast<object>();

                        // Handle collections and single entities
                        if (isCollection)
                        {
                            // Iterate through the casted entity list and add each entity to the collection
                            foreach (var entityFK in castEntityFKList)
                            {
                                if (propertyToModify.GetValue(entity) is ICollection<object> collection)
                                {
                                    collection.Add(entityFK);
                                }
                            }
                        }
                        else
                        {
                            // Assign the first element of the casted entity list to the property
                            var entityFK = castEntityFKList.FirstOrDefault();
                            propertyToModify.SetValue(entity, entityFK);
                        }
                    }
                }
            }
        }

        internal void IncludeInverseProperties<T>(
            T objectEntity, 
            Dictionary<string, string>? replacesTableName, 
            EnttityHelper enttityHelper, 
            string? inversePropertyNameOnly = null
            ) where T : class
        {
            if (objectEntity == null) return;

            List<PropertyInfo>? propertiesInverse = ToolsProp.GetInverseProperties(objectEntity);
            if (propertiesInverse == null || propertiesInverse.Count == 0)
            {
                Debug.WriteLine($"No inverse properties found in '{objectEntity}'.");
                return;
            }

            if (!string.IsNullOrEmpty(inversePropertyNameOnly)) // If not all
            {
                propertiesInverse = propertiesInverse.Where(p => p.Name == inversePropertyNameOnly).ToList();
            }

            foreach (var prop in propertiesInverse)
            {
                Type collectionType = prop.PropertyType;

                if (collectionType.IsGenericType && collectionType.GetGenericTypeDefinition() == typeof(ICollection<>))
                {
                    Type entity2Type = collectionType.GetGenericArguments()[0];

                    if (entity2Type.IsClass && !entity2Type.IsAbstract)
                    {                        
                        Features features = new(enttityHelper);
                        // SqlQueryString sqlQueryString = new SqlQueryString(enttityHelper);
                        SqlQueryString sqlQueryString = enttityHelper.GetQuery;
                        
                        // var selectMethod = features.GetType().GetMethod("ExecuteSelectDt");

                        string nameTable1 = ToolsProp.GetTableNameManyToMany(objectEntity.GetType(), prop, replacesTableName);
                        string nameTable2 = ToolsProp.GetTableName(entity2Type, replacesTableName);
                        string columnName1 = ToolsProp.GetTableName(objectEntity.GetType(), replacesTableName).Split('.').Last();
                        string columnName2 = nameTable2.Split('.').Last();
                        string nameTable1Escaped = sqlQueryString.EscapeIdentifier(nameTable1);

                        string idName1 = ToolsProp.GetPK(objectEntity.GetType()).Name;
                        string idName2 = ToolsProp.GetPK(entity2Type).Name;
                        PropertyInfo idProp1 = objectEntity.GetType().GetProperty(idName1);
                        object idValue1 = idProp1.GetValue(objectEntity);

                        // var entitiesToAdd = (DataTable)selectMethod.Invoke(features, new object[] { $"SELECT ID_{columnName2} FROM {nameTable} WHERE ID_{columnName1}='{idValue1}'" , null, 0, null, null, true });
                        var entitiesToAdd = features.ExecuteSelectDt($"SELECT ID_{columnName2} FROM {nameTable1Escaped} WHERE ID_{columnName1} = '{idValue1}'", null, 0, null, null, true);
                        
                        var getMethod = typeof(Features).GetMethod("Get").MakeGenericMethod(entity2Type);
                        
                        Type typeCollection = typeof(List<>).MakeGenericType(entity2Type);
                        var collectionInstance = Activator.CreateInstance(typeCollection);

                        for (int i = 0; i < entitiesToAdd.Rows.Count; i++)
                        {
                            object idValue2 = entitiesToAdd.Rows[i][0];
                            var entity2ToAddList = (IEnumerable)getMethod.Invoke(features, new object[] { false, $"{idName2} = '{idValue2}'", nameTable2, null, 0, null, true });
                            // var entity2ToAddList = features.Get(entity2Type.GetType(), false, $"{idName2}='{idValue2}'", nameTable2, null, 0, null, true);
                            foreach (var entity in entity2ToAddList) { ((IList)collectionInstance).Add(entity); }
                        }

                        prop.SetValue(objectEntity, collectionInstance);
                        
                    }
                }
            }
        }

        internal void IncludeCollectionProperties<T>(
            T objectEntity,
            Dictionary<string, string>? replacesTableName,
            EnttityHelper enttityHelper,
            string? collectionPropertyNameOnly = null)
        {
            if (objectEntity == null)
                throw new ArgumentNullException(nameof(objectEntity));
            if (enttityHelper == null)
                throw new ArgumentNullException(nameof(enttityHelper));

            List<PropertyInfo>? propertiesCollection = ToolsProp.GetCollecionProperties(objectEntity);
            if (propertiesCollection == null || propertiesCollection.Count == 0)
            {
                Debug.WriteLine($"No collection properties found in '{typeof(T).Name}'.");
                return;
            }

            // Filter to only one property if specified
            if (!string.IsNullOrEmpty(collectionPropertyNameOnly))
            {
                propertiesCollection = propertiesCollection
                    .Where(p => p.Name == collectionPropertyNameOnly)
                    .ToList();
            }

            foreach (var collectionProp in propertiesCollection)
            {
                Type? entityType = collectionProp.PropertyType.GetGenericArguments().FirstOrDefault();
                if (entityType == null)
                {
                    Debug.WriteLine($"Unable to determine element type of collection '{collectionProp.Name}'.");
                    continue;
                }
            
                var foreignKeyProp = ToolsProp.GetForeignKeyPropertyToEntity(entityType, objectEntity.GetType());
                if (foreignKeyProp == null)
                {
                    Debug.WriteLine($"No foreign key property found for entity '{entityType.Name}' in '{objectEntity.GetType().Name}'.");
                    continue;
                }
            
                string? tableName = ToolsProp.GetTableName(entityType, replacesTableName);
                string tableNameEscaped = enttityHelper.GetQuery.EscapeIdentifier(tableName);
                
                // Call EnttityHelper.Get<TEntity>(...) using reflection
                var getMethod = typeof(EnttityHelper).GetMethod(nameof(EnttityHelper.Get));
                if (getMethod == null)
                    throw new InvalidOperationException("The method 'Get' was not found in EnttityHelper.");

                var genericMethod = getMethod.MakeGenericMethod(entityType);
                
                try
                {
                    var propPk = ToolsProp.GetPK(objectEntity.GetType());
                    var pkValue = propPk?.GetValue(objectEntity, null);
                    string filter = $"{foreignKeyProp.Name} = '{pkValue}'";
                    
                    // List<TEntity>? Get<TEntity>(bool includeAll = true, string? filter = null, string? tableName = null
                    object? result = genericMethod.Invoke(enttityHelper, new object[]
                    {
                        true,                   // includeAll
                        filter,                 // filter
                        tableNameEscaped,       // tableName
                        null,                   // pageSize
                        0,                      // pageIndex
                        null,                   // sortColumn
                        true                    // sortAscending
                    });

                    if (result == null)
                    {
                        Debug.WriteLine($"No records returned for collection '{collectionProp.Name}'.");
                        continue;
                    }

                    collectionProp.SetValue(objectEntity, result);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Failed to load data for collection property '{collectionProp.Name}' of type '{entityType.Name}'.", ex);
                }
            }
        }



    }
}
