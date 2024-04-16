using EH.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace EH.Entities
{
    internal static class Inclusions
    {
        internal static void IncludeForeignKeyEntities<TEntity>(this TEntity entity, string? fkOnly = null)
        {
            if (entity == null) return;

            var propertiesFK = ToolsEH.GetFKProperties(entity);
            if (propertiesFK == null || propertiesFK.Count == 0)
            {
                Console.WriteLine("No foreign key properties found!");
                return;
            }

            if (!string.IsNullOrEmpty(fkOnly)) // If not all
            {
                propertiesFK = propertiesFK.Where(x => x.Key.ToString() == fkOnly).ToDictionary(x => x.Key, x => x.Value);
            }

            foreach (KeyValuePair<object, object> pair in propertiesFK)
            {
                if (pair.Value != null)
                {
                    var pk = ToolsEH.GetPK(pair.Value);
                    if (pk == null) continue;

                    var propertyToUpdate = entity.GetType().GetProperty(pair.Key.ToString());

                    if (propertyToUpdate != null)
                    {
                        var pkValue = pk.GetValue(pair.Value, null);
                        if (pkValue == null || string.IsNullOrEmpty(pkValue.ToString().Trim()) || (pk.PropertyType.IsPrimitive && pkValue.Equals(Convert.ChangeType(0, pkValue.GetType())))) continue;

                        // Get the property type of the foreign key
                        Type? fkEntityType = propertyToUpdate.PropertyType;

                        // Check if it is a generic collection type
                        bool isCollection = typeof(ICollection<>).IsAssignableFrom(fkEntityType);

                        // Get the actual type of the elements in the collection (if applicable)
                        Type elementType = isCollection ? fkEntityType.GetGenericArguments()[0] : fkEntityType;

                        // Use the correct generic type for the Get method
                        MethodInfo genericGetMethod = typeof(EnttityHelper).GetMethod("Get").MakeGenericMethod(elementType);

                        // Retrieve the foreign key entities
                        IEnumerable<object> entityFKList = (IEnumerable<object>)genericGetMethod.Invoke(typeof(EnttityHelper), new object[] { true, $"{pk.Name}='{pkValue}'" });

                        // Cast each entity to the actual type
                        IEnumerable<object> castEntityFKList = entityFKList.Cast<object>();

                        // Handle collections and single entities
                        if (isCollection)
                        {
                            // Iterate through the casted entity list and add each entity to the collection
                            foreach (var entityFK in castEntityFKList)
                            {
                                if (propertyToUpdate.GetValue(entity) is ICollection<object> collection)
                                {
                                    collection.Add(entityFK);
                                }
                            }
                        }
                        else
                        {
                            // Assign the first element of the casted entity list to the property
                            var entityFK = castEntityFKList.FirstOrDefault();
                            propertyToUpdate.SetValue(entity, entityFK);
                        }
                    }
                }
            }
        }




    }
}
