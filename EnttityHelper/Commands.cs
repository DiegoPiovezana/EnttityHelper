using System;
using System.Collections.Generic;
using System.Text;

namespace EH
{
    /// <summary>
    /// Allows you to obtain the main commands to be executed on the database.
    /// </summary>
    public static class Commands
    {
        /// <summary>
        /// Gets the insert command.
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to be manipulated.</typeparam>
        /// <param name="eh">EnttityHelper object.</param>
        /// <param name="entity">Entity to be inserted into the database.</param>
        /// <param name="namePropUnique">Name of the property to be considered as a uniqueness criterion (optional).</param>
        /// <returns>String command.</returns>
        public static string? Insert<TEntity>(EnttityHelper eh, TEntity entity, string? namePropUnique = null)
        {
            var properties = ToolsEH.GetProperties(entity);
            string tableName = ToolsEH.GetTable<TEntity>();

            if (!string.IsNullOrEmpty(namePropUnique) && eh.CheckIfExist(tableName, $"{namePropUnique} = {properties[namePropUnique]}"))
            {
                Console.WriteLine("Entity already exists in table!");
                return null;
            }

            string columns = string.Join(", ", properties.Keys);
            string values = string.Join("', '", properties.Values);

            return $"INSERT INTO {tableName} ({columns}) VALUES ('{values}')";

        }

        /// <summary>
        /// Gets the update command.
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to be manipulated.</typeparam>
        /// <param name="entity">Entity to be updated in the database.</param>
        /// <param name="nameId">Entity Id column name.</param>
        /// <returns>String command.</returns>
        public static string? Update<TEntity>(TEntity entity, string? nameId = null) where TEntity : class
        {
            StringBuilder queryBuilder = new();
            queryBuilder.Append($"UPDATE {ToolsEH.GetTable<TEntity>()} SET ");

            nameId ??= ToolsEH.GetPK(entity)?.Name;

            if (nameId is null)
            {
                Console.WriteLine("No primary key found!");
                return null;
            }

            var properties = ToolsEH.GetProperties(entity);

            foreach (KeyValuePair<string, object> pair in properties)
            {
                if (pair.Key != nameId)
                {
                    queryBuilder.Append($"{pair.Key} = '{pair.Value}', ");
                }
            }

            queryBuilder.Length -= 2; // Remove the last comma and space
            queryBuilder.Append($" WHERE {nameId} = '{properties[nameId]}'");
            return queryBuilder.ToString();
        }

        /// <summary>
        /// Generates a SQL SELECT query for a specified entity based on the ID property.
        /// </summary>
        /// <typeparam name="TEntity">The type of entity.</typeparam>
        /// <param name="entity">The entity object.</param>
        /// <param name="idPropName">The name of the ID property.</param>
        /// <param name="includeAll">Whether to include all related entities.</param>
        /// <returns>A SELECT SQL query string.</returns>
        public static string? Search<TEntity>(TEntity entity, string? idPropName = null, bool includeAll = true) where TEntity : class
        {
            idPropName ??= ToolsEH.GetPK(entity)?.Name;
            if (idPropName is null) { return null; }
            return $"SELECT * FROM {ToolsEH.GetTable<TEntity>()} WHERE ({idPropName} = {typeof(TEntity).GetProperty(idPropName).GetValue(entity, null)})";
        }

        /// <summary>
        /// Generates a SQL SELECT query for a specified entity with optional filters.
        /// </summary>
        /// <typeparam name="TEntity">The type of entity.</typeparam>
        /// <param name="includeAll">Whether to include all related entities.</param>
        /// <param name="filter">The filter criteria.</param>
        /// <returns>A SELECT SQL query string.</returns>
        public static string? Get<TEntity>(bool includeAll = true, string? filter = null)
        {
            filter = string.IsNullOrEmpty(filter?.Trim()) ? "1 = 1" : filter;
            return $"SELECT * FROM {ToolsEH.GetTable<TEntity>()} WHERE ({filter})";
        }

        /// <summary>
        /// Generates a SQL DELETE query for a specified entity based on the ID property.
        /// </summary>
        /// <typeparam name="TEntity">The type of entity.</typeparam>
        /// <param name="entity">The entity object.</param>
        /// <param name="nameId">The name of the ID property.</param>
        /// <returns>A DELETE SQL query string.</returns>
        public static string? Delete<TEntity>(TEntity entity, string? nameId = null) where TEntity : class
        {
            nameId ??= ToolsEH.GetPK(entity)?.Name;

            if (nameId is null)
            {
                Console.WriteLine("No primary key found!");
                return null;
            }

            var properties = ToolsEH.GetProperties(entity);

            return $"DELETE FROM {ToolsEH.GetTable<TEntity>()} WHERE {nameId} = '{properties[nameId]}'";
        }

        /// <summary>
        /// Generates a SQL SELECT query for a specified entity based on the ID property.
        /// </summary>
        /// <typeparam name="TEntity">The type of entity.</typeparam>
        /// <param name="entity">The entity object.</param>
        /// <param name="nameId">The name of the ID property.</param>
        /// <returns>A SELECT SQL query string.</returns>
        public static string? Select<TEntity>(TEntity entity, string? nameId = null) where TEntity : class
        {
            nameId ??= ToolsEH.GetPK(entity)?.Name;

            if (nameId is null)
            {
                Console.WriteLine("No primary key found!");
                return null;
            }

            var properties = ToolsEH.GetProperties(entity);

            return $"SELECT * FROM {ToolsEH.GetTable<TEntity>()} WHERE {nameId} = '{properties[nameId]}'";
        }

    }
}
