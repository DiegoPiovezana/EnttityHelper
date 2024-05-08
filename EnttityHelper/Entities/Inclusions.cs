using EH.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace EH.Entities
{
    internal class Inclusions
    {
        private readonly EnttityHelper _enttityHelper;

        public Inclusions(EnttityHelper enttityHelper)
        {
            _enttityHelper = enttityHelper;
        }

        internal void IncludeForeignKeyEntities<TEntity>(TEntity entity, string? fkOnly = null)
        {
            if (entity == null) return;

            var propertiesFK = ToolsProp.GetFKProperties(entity);
            if (propertiesFK == null || propertiesFK.Count == 0)
            {
                Debug.WriteLine($"No foreign key properties found in '{entity}'.");
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
                    var pk = ToolsProp.GetPK(pair.Value);
                    if (pk == null) continue;

                    var propertyToUpdate = entity.GetType().GetProperty(pair.Key.ToString());

                    if (propertyToUpdate != null)
                    {
                        var pkValue = pk.GetValue(pair.Value, null);
                        if (pkValue == null || string.IsNullOrEmpty(pkValue.ToString().Trim()) || pk.PropertyType.IsPrimitive && pkValue.Equals(Convert.ChangeType(0, pkValue.GetType()))) continue;

                        // Get the property type of the foreign key
                        Type? fkEntityType = propertyToUpdate.PropertyType;

                        // Check if it is a generic collection type
                        bool isCollection = typeof(ICollection<>).IsAssignableFrom(fkEntityType);

                        // Get the actual type of the elements in the collection (if applicable)
                        Type elementType = isCollection ? fkEntityType.GetGenericArguments()[0] : fkEntityType;

                        // Use the correct generic type for the Get method
                        MethodInfo genericGetMethod = typeof(EnttityHelper).GetMethod("Get").MakeGenericMethod(elementType);

                        // Retrieve the foreign key entities
                        IEnumerable<object> entityFKList = (IEnumerable<object>)genericGetMethod.Invoke(_enttityHelper, new object[] { true, $"{pk.Name}='{pkValue}'", null });

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

        internal void IncludeInverseProperties<TEntity>(TEntity entity, Dictionary<string, string>? replacesTableName, string ? InversePropertyOnly = null)
        {
            if (entity == null) return;

            var propertiesInverse = ToolsProp.GetInverseProperties(entity, replacesTableName, _enttityHelper);
            if (propertiesInverse == null || propertiesInverse.Count == 0)
            {
                Debug.WriteLine($"No inverse properties found in '{entity}'.");
                return;
            }

            if (!string.IsNullOrEmpty(InversePropertyOnly)) // If not all
            {
                propertiesInverse = propertiesInverse.Where(x => x.Key.ToString() == InversePropertyOnly).ToDictionary(x => x.Key, x => x.Value);
            }

            foreach (KeyValuePair<object, object> pair in propertiesInverse)
            {
                if (pair.Value != null)
                {
                    var pk = ToolsProp.GetPK(pair.Value);
                    if (pk == null) continue;

                    var propertyToUpdate = entity.GetType().GetProperty(pair.Key.ToString());

                    if (propertyToUpdate != null)
                    {
                        var pkValue = pk.GetValue(pair.Value, null);
                        if (pkValue == null || string.IsNullOrEmpty(pkValue.ToString().Trim()) || pk.PropertyType.IsPrimitive && pkValue.Equals(Convert.ChangeType(0, pkValue.GetType()))) continue;

                        // Obtém o tipo da chave estrangeira
                        Type? fkEntityType = propertyToUpdate.PropertyType;

                        // Verifica se é um tipo de coleção genérica
                        bool isCollection = typeof(ICollection<>).IsAssignableFrom(fkEntityType);

                        // Obtém o tipo real dos elementos na coleção (se aplicável)
                        Type elementType = isCollection ? fkEntityType.GetGenericArguments()[0] : fkEntityType;

                        // Usa o tipo genérico correto para o método Get
                        MethodInfo genericGetMethod = typeof(EnttityHelper).GetMethod("Get").MakeGenericMethod(elementType);

                        // Recupera as entidades da chave estrangeira
                        IEnumerable<object> entityFKList = (IEnumerable<object>)genericGetMethod.Invoke(_enttityHelper, new object[] { true, $"{pk.Name}='{pkValue}'", null });

                        // Converte cada entidade para o tipo real
                        IEnumerable<object> castEntityFKList = entityFKList.Cast<object>();

                        // Manipula coleções e entidades individuais
                        if (isCollection)
                        {
                            // Itera pela lista de entidades convertidas e adiciona cada entidade à coleção
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
                            // Atribui o primeiro elemento da lista de entidades convertidas à propriedade
                            var entityFK = castEntityFKList.FirstOrDefault();
                            propertyToUpdate.SetValue(entity, entityFK);
                        }
                    }
                }
            }
        }



    }
}
