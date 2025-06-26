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

            // 1. Verifica se existe InverseProperty definido
            var inverseProperty = GetInverseProperty(entity1, entity2Type, collectionProperty);

            // 2. Se existe propriedade inversa que é coleção = M:N
            if (inverseProperty != null && IsCollectionProperty(inverseProperty))
            {
                return RelationshipType.ManyToMany;
            }

            // 3. Verifica se a entidade relacionada tem FK para a primeira entidade
            var foreignKeyProperty = GetForeignKeyPropertyToEntity(entity2Type, entity1);

            // 4. Se tem FK direta = 1:N, se não tem = M:N
            return foreignKeyProperty != null ? RelationshipType.OneToMany : RelationshipType.ManyToMany;
        }

        /// <summary>
        /// Cria tabela auxiliar para relacionamento M:N
        /// </summary>
        public static (string, QueryCommand?) CreateManyToManyTable(Type entity1, Property collectionProperty,
            Dictionary<string, string>? replacesTableName = null)
        {
            var relationshipType = ValidateRelationshipType(entity1, collectionProperty);

            if (relationshipType != RelationshipType.ManyToMany)
            {
                return (null, null); // Não precisa de tabela auxiliar
            }

            var entity2Type = GetCollectionElementType(collectionProperty.PropertyInfo.PropertyType);

            // Gera nome da tabela auxiliar
            var tableName = GenerateJunctionTableName(entity1, entity2Type, replacesTableName);

            // Obtém as chaves primárias das duas entidades
            var pkEntity1 = GetPrimaryKeyProperty(entity1);
            var pkEntity2 = GetPrimaryKeyProperty(entity2Type);

            if (pkEntity1 == null || pkEntity2 == null)
            {
                throw new InvalidOperationException("Ambas as entidades devem ter chave primária definida");
            }

            // Cria o comando SQL para a tabela auxiliar
            return (tableName, CreateJunctionTableCommand(tableName, entity1, entity2Type, pkEntity1, pkEntity2));
        }

        private static PropertyInfo? GetInverseProperty(Type entity1, Type entity2, Property originalProperty)
        {
            var inversePropertyAttr = originalProperty.InverseProperty;

            if (inversePropertyAttr != null)
            {
                return entity2.GetProperty(inversePropertyAttr.Property);
            }

            // Busca por convenção
            return entity2.GetProperties()
                .FirstOrDefault(p => IsCollectionProperty(p) &&
                                     GetCollectionElementType(p.PropertyType) == entity1);
        }

        private static PropertyInfo? GetForeignKeyPropertyToEntity(Type sourceType, Type targetType)
        {
            return sourceType.GetProperties()
                .FirstOrDefault(p =>
                {
                    // Verifica se é FK por convenção (Id + nome da entidade)
                    if (p.Name.Equals($"Id{targetType.Name}", StringComparison.OrdinalIgnoreCase) ||
                        p.Name.Equals($"{targetType.Name}Id", StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }

                    // Verifica se tem ForeignKeyAttribute
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

            // Para IEnumerable não genérico
            var enumerableInterface = collectionType.GetInterfaces()
                .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));

            return enumerableInterface?.GetGenericArguments()[0]
                   ?? throw new ArgumentException("Tipo de coleção não suportado");
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
            var table1Name = GetTableName(entity1, replacesTableName);
            var table2Name = GetTableName(entity2, replacesTableName);

            // Ordena alfabeticamente para consistência
            var orderedNames = new[] { table1Name, table2Name }.OrderBy(n => n).ToArray();

            return $"{orderedNames[0]}_{orderedNames[1]}";
        }

        private static string GetTableName(Type entityType, Dictionary<string, string>? replacesTableName)
        {
            if (replacesTableName?.ContainsKey(entityType.Name) == true)
            {
                return replacesTableName[entityType.Name];
            }

            var tableAttr = entityType.GetCustomAttribute<TableAttribute>();
            return tableAttr?.Name ?? entityType.Name;
        }

        private static QueryCommand CreateJunctionTableCommand(string tableName, Type entity1, Type entity2,
            PropertyInfo pkEntity1, PropertyInfo pkEntity2)
        {
            var sql = $@"
            CREATE TABLE {tableName} (
                {GetTableName(entity1, null)}Id {GetSqlType(pkEntity1.PropertyType)} NOT NULL,
                {GetTableName(entity2, null)}Id {GetSqlType(pkEntity2.PropertyType)} NOT NULL,
                PRIMARY KEY ({GetTableName(entity1, null)}Id, {GetTableName(entity2, null)}Id),
                FOREIGN KEY ({GetTableName(entity1, null)}Id) REFERENCES {GetTableName(entity1, null)}({pkEntity1.Name}),
                FOREIGN KEY ({GetTableName(entity2, null)}Id) REFERENCES {GetTableName(entity2, null)}({pkEntity2.Name})
            )";

            return new QueryCommand(sql, null, null);
        }

        private static string GetSqlType(Type type)
        {
            // Simplificado - você deve usar sua lógica de mapeamento de tipos
            return type.Name switch
            {
                "Int64" => "BIGINT",
                "Int32" => "INT",
                "String" => "NVARCHAR(450)",
                "Guid" => "UNIQUEIDENTIFIER",
                _ => "NVARCHAR(450)"
            };
        }
    }

    public enum RelationshipType
    {
        OneToMany,
        ManyToMany
    }
}