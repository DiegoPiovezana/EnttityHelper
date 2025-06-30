using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using EH.Properties;

namespace EH.Connection
{
    public class RelationshipValidator
    {
        /// <summary>
        /// Determines whether a collection relationship is Many-to-Many or One-to-Many.
        /// </summary>
        public static RelationshipType ValidateRelationshipType(Type entity1, Property collectionProperty)
        {
            if (!collectionProperty.IsCollection.GetValueOrDefault())
            {
                throw new ArgumentException("The property must be a collection", nameof(collectionProperty));
            }

            var entity2Type = GetCollectionElementType(collectionProperty.PropertyInfo.PropertyType);

            // 1. Checks if there is InverseProperty defined
            var inverseProperty = GetInverseProperty(entity1, entity2Type, collectionProperty);

            // 2. If there is an inverse property that is collection = M:N
            if (inverseProperty != null && IsCollectionProperty(inverseProperty))
            {
                return RelationshipType.ManyToMany;
            }

            // 3. Checks if related entity has FK to first entity
            var foreignKeyProperty = GetForeignKeyPropertyToEntity(entity2Type, entity1);

            // 4. If it has direct FK = 1:N, if it doesn't = M:N
            return foreignKeyProperty != null ? RelationshipType.OneToMany : RelationshipType.ManyToMany;
        }

        /// <summary>
        /// Creates a junction table for a Many-to-Many relationship between two entities.
        /// </summary>
        public static (string, QueryCommand?) CreateManyToManyTable(Type entity1, Property collectionProperty, Database database,
            Dictionary<string, string>? replacesTableName = null)
        {
            var relationshipType = ValidateRelationshipType(entity1, collectionProperty);

            if (relationshipType != RelationshipType.ManyToMany)
            {
                return (null, null);
            }

            var entity2Type = GetCollectionElementType(collectionProperty.PropertyInfo.PropertyType);
            
            var tableName = GenerateJunctionTableName(entity1, entity2Type, replacesTableName);
            
            var pkEntity1 = GetPrimaryKeyProperty(entity1);
            var pkEntity2 = GetPrimaryKeyProperty(entity2Type);

            if (pkEntity1 == null || pkEntity2 == null)
            {
                throw new InvalidOperationException("Both entities must have primary key defined");
            }
            
            return (tableName, CreateJunctionTableCommand(tableName, entity1, entity2Type, pkEntity1, pkEntity2, database));
        }

        private static PropertyInfo? GetInverseProperty(Type entity1, Type entity2, Property originalProperty)
        {
            var inversePropertyAttr = originalProperty.InverseProperty;

            if (inversePropertyAttr != null)
            {
                return entity2.GetProperty(inversePropertyAttr.Property);
            }

            // Search by convention
            return entity2.GetProperties()
                .FirstOrDefault(p => IsCollectionProperty(p) &&
                                     GetCollectionElementType(p.PropertyType) == entity1);
        }

        private static PropertyInfo? GetForeignKeyPropertyToEntity(Type sourceType, Type targetType)
        {
            return sourceType.GetProperties()
                .FirstOrDefault(p =>
                {
                    // Checks if it is FK by convention (Id + entity name)
                    if (p.Name.Equals($"Id{targetType.Name}", StringComparison.OrdinalIgnoreCase) ||
                        p.Name.Equals($"{targetType.Name}Id", StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                   
                    // Check for ForeignKeyAttribute
                    var fkAttr = p.GetCustomAttribute<ForeignKeyAttribute>();
                    return fkAttr != null &&
                           sourceType.GetProperties().Any(prop =>
                               prop.Name == fkAttr.Name && prop.PropertyType == targetType);
                });
        }

        private static Type GetCollectionElementType(Type collectionType)
        {
            if (collectionType.IsGenericType)
            {
                return collectionType.GetGenericArguments()[0];
            }
           
            var enumerableInterface = collectionType.GetInterfaces()
                .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));

            return enumerableInterface?.GetGenericArguments()[0]
                   ?? throw new ArgumentException("Unsupported collection type");
        }

        private static bool IsCollectionProperty(PropertyInfo property)
        {
            return property.PropertyType != typeof(string) &&
                   typeof(System.Collections.IEnumerable).IsAssignableFrom(property.PropertyType);
        }

        private static PropertyInfo? GetPrimaryKeyProperty(Type entityType)
        {
            return entityType.GetProperties()
                .FirstOrDefault(p =>
                    p.GetCustomAttribute<System.ComponentModel.DataAnnotations.KeyAttribute>() != null);
        }

        private static string GenerateJunctionTableName(Type entity1, Type entity2,
            Dictionary<string, string>? replacesTableName)
        {
            var table1Name = ToolsProp.GetTableName(entity1, replacesTableName);
            var table2Name = ToolsProp.GetTableName(entity2, replacesTableName);

            // Ordened alfabetically to ensure consistent naming
            var orderedNames = new[] { table1Name, table2Name }.OrderBy(n => n).ToArray();

            return $"{orderedNames[0]}_{orderedNames[1]}";
        }

        private static QueryCommand CreateJunctionTableCommand(string tableName, Type entity1, Type entity2,
            PropertyInfo pkEntity1, PropertyInfo pkEntity2, Database database)
        {
            var sqlQueryString = new SqlQueryString(database);
            
            string tb1 = ToolsProp.GetTableName(entity1, null);
            string tb2 = ToolsProp.GetTableName(entity2, null);
            string idTb1 = tb1.Substring(0, Math.Min(tb1.Length, 27));
            string idTb2 = tb2.Substring(0, Math.Min(tb2.Length, 27));
            string idTb1Escaped = sqlQueryString.EscapeIdentifier(idTb1);
            string idTb2Escaped = sqlQueryString.EscapeIdentifier(idTb2);
            idTb1 = idTb1.Contains('.') ? idTb1.Split('.').Last(): idTb1;
            idTb2 = idTb2.Contains('.') ? idTb2.Split('.').Last(): idTb2;
            
            string type1 = database.TypesDefault[pkEntity1.PropertyType.Name];
            string type2 = database.TypesDefault[pkEntity2.PropertyType.Name];
            
            var sql = $@"
            CREATE TABLE {sqlQueryString.EscapeIdentifier(tableName)} (
                ID_{idTb1} {type1} NOT NULL,
                ID_{idTb2} {type2} NOT NULL,
                PRIMARY KEY (ID_{idTb1}, ID_{tb2}),
                FOREIGN KEY (ID_{idTb1}) REFERENCES {idTb1Escaped}({pkEntity1.Name}),
                FOREIGN KEY (ID_{idTb2}) REFERENCES {idTb2Escaped}({pkEntity2.Name})
            )";

            return new QueryCommand(sql, null, null);
        }
        
    }

    public enum RelationshipType
    {
        OneToMany,
        ManyToMany
    }
}