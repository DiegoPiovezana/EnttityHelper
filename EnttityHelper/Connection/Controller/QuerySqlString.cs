﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using EH.Properties;

namespace EH.Connection
{
    /// <summary>
    /// Allows you to obtain the main commands to be executed on the database.
    /// </summary>
    public class QuerySqlString
    {
        /// <summary>
        /// Gets the insert command.
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to be manipulated.</typeparam>
        /// <param name="entity">Entity to be inserted into the database.</param>  
        /// <param name="replacesTableName">(Optional) Terms that can be replaced in table names.</param>
        /// <param name="tableName">(Optional) Name of the table to which the entity will be inserted. By default, the table informed in the "Table" attribute of the entity class will be considered.</param> 
        /// <returns>String command.</returns>
        public string? Insert<TEntity>(TEntity entity, Dictionary<string, string>? replacesTableName = null, string? tableName = null)
        {
            var properties = ToolsEH.GetProperties(entity);
            string columns = string.Join(", ", properties.Keys);
            string values = string.Join("', '", properties.Values);

            tableName ??= ToolsEH.GetTableName<TEntity>(replacesTableName);

            return $"INSERT INTO {tableName} ({columns}) VALUES ('{values}')";

        }

        /// <summary>
        /// Gets the update command.
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to be manipulated.</typeparam>
        /// <param name="entity">Entity to be updated in the database.</param>
        /// <param name="nameId">(Optional) Entity Id column name.</param>
        /// <param name="replacesTableName">(Optional) Terms that can be replaced in table names.</param>
        /// <param name="tableName">(Optional) Name of the table to which the entity will be inserted. By default, the table informed in the "Table" attribute of the entity class will be considered.</param> 
        /// <returns>String command.</returns>
        public string? Update<TEntity>(TEntity entity, string? nameId = null, Dictionary<string, string>? replacesTableName = null, string? tableName = null) where TEntity : class
        {
            StringBuilder queryBuilder = new();
            tableName ??= ToolsEH.GetTableName<TEntity>(replacesTableName);

            queryBuilder.Append($"UPDATE {tableName} SET ");

            nameId ??= ToolsEH.GetPK(entity)?.Name;

            if (nameId is null)
            {
                Console.WriteLine("No primary key found!");
                return null;
            }

            var properties = ToolsEH.GetProperties(entity);

            foreach (KeyValuePair<string, Property> pair in properties)
            {
                if (pair.Key != nameId)
                {
                    queryBuilder.Append($"{pair.Key} = '{pair.Value.ValueSql}', ");
                }
            }

            queryBuilder.Length -= 2; // Remove the last comma and space
            queryBuilder.Append($" WHERE {nameId} = '{properties[nameId]}'");
            return queryBuilder.ToString();
        }

        /// <summary>
        /// Generates a SQL SELECT query for a specified entity with optional filters.
        /// </summary>
        /// <typeparam name="TEntity">The type of entity.</typeparam>     
        /// <param name="filter">(Optional) The filter criteria.</param>
        /// <param name="replacesTableName">(Optional) Terms that can be replaced in table names.</param>
        /// <param name="tableName">(Optional) Name of the table to which the entity will be inserted. By default, the table informed in the "Table" attribute of the entity class will be considered.</param> 
        /// <returns>A SELECT SQL query string.</returns>
        public string? Get<TEntity>(string? filter = null, Dictionary<string, string>? replacesTableName = null, string? tableName = null)
        {
            filter = string.IsNullOrEmpty(filter?.Trim()) ? "1 = 1" : filter;
            tableName ??= ToolsEH.GetTableName<TEntity>(replacesTableName);
            return $"SELECT * FROM {tableName} WHERE ({filter})";
        }

        /// <summary>
        /// Generates a SQL SELECT query for a specified entity based on the ID property.
        /// </summary>
        /// <typeparam name="TEntity">The type of entity.</typeparam>
        /// <param name="entity">The entity object.</param>    
        /// <param name="idPropName">(Optional) The name of the ID property.</param>
        /// <param name="replacesTableName">(Optional) Terms that can be replaced in table names.</param> 
        /// <param name="tableName">(Optional) Name of the table to which the entity will be inserted. By default, the table informed in the "Table" attribute of the entity class will be considered.</param> 
        /// <returns>A select SQL query string.</returns>
        public string? Search<TEntity>(TEntity entity, string? idPropName = null, Dictionary<string, string>? replacesTableName = null, string? tableName = null) where TEntity : class
        {
            idPropName ??= ToolsEH.GetPK(entity)?.Name;
            if (idPropName is null) { return null; }
            tableName ??= ToolsEH.GetTableName<TEntity>(replacesTableName);
            return $"SELECT * FROM {tableName} WHERE ({idPropName} = '{typeof(TEntity).GetProperty(idPropName).GetValue(entity, null)}')";
        }

        /// <summary>
        /// Generates a SQL DELETE query for a specified entity based on the ID property.
        /// </summary>
        /// <typeparam name="TEntity">The type of entity.</typeparam>
        /// <param name="entity">The entity object.</param>
        /// <param name="idPropName">(Optional) The name of the ID property.</param>
        /// <param name="replacesTableName">(Optional) Terms that can be replaced in table names.</param>
        /// <param name="tableName">(Optional) Name of the table to which the entity will be inserted. By default, the table informed in the "Table" attribute of the entity class will be considered.</param> 
        /// <returns>A delete SQL query string.</returns>
        public string? Delete<TEntity>(TEntity entity, string? idPropName = null, Dictionary<string, string>? replacesTableName = null, string? tableName = null) where TEntity : class
        {
            idPropName ??= ToolsEH.GetPK(entity)?.Name;

            if (idPropName is null)
            {
                Console.WriteLine("No primary key found!");
                return null;
            }

            tableName ??= ToolsEH.GetTableName<TEntity>(replacesTableName);

            return $"DELETE FROM {tableName} WHERE ({idPropName} = '{typeof(TEntity).GetProperty(idPropName).GetValue(entity, null)}')";
        }

        /// <summary>
        /// Allows you to obtain the table creation query for TEntity./>.
        /// </summary>
        /// <typeparam name="TEntity">The type of entity.</typeparam>
        /// <param name="typesSql">Dictionary containing types related to C# code and database data.</param>
        /// <param name="replacesTableName">(Optional) Terms that can be replaced in table names.</param>  
        /// <param name="tableName">(Optional) Name of the table to which the entity will be inserted. By default, the table informed in the "Table" attribute of the entity class will be considered.</param> 
        /// <returns>Table creation query.</returns>
        public string? CreateTable<TEntity>(Dictionary<string, string>? typesSql, Dictionary<string, string>? replacesTableName = null, string? tableName = null)
        {
            if (typesSql is null) { throw new ArgumentNullException(nameof(typesSql)); }

            StringBuilder queryBuilder = new();
            tableName ??= ToolsEH.GetTableName<TEntity>(replacesTableName);
            queryBuilder.Append($"CREATE TABLE {tableName} (");

            TEntity entity = Activator.CreateInstance<TEntity>() ?? throw new ArgumentNullException(nameof(entity));
            var properties = ToolsEH.GetProperties(entity);

            foreach (KeyValuePair<string, Property> pair in properties)
            {
                if (pair.Value?.Type is null) { throw new InvalidOperationException($"Error mapping entity '{nameof(entity)}' property types!"); }

                typesSql.TryGetValue(pair.Value.Type.Name.Trim(), out string value);

                if (value is null)
                {
                    Console.WriteLine($"Type default not found in Dictionary TypesDefault for '{pair.Value.Type.Name}'!");
                    throw new InvalidOperationException($"Type default not found in Dictionary TypesDefault for '{pair.Value.Type.Name}'! Please enter it into the dictionary or consider changing the type.");
                }

                // MaxLength?
                if (pair.Value.MaxLength > 0)
                {
                    value = Regex.Replace(value, @"\([^()]*\)", "");
                    value += $"({pair.Value.MaxLength})";
                }

                // PK?
                var pk = ToolsEH.GetPK((object)entity);
                if (pair.Key == pk?.Name || pair.Key == pk?.GetCustomAttribute<ColumnAttribute>()?.Name)
                {
                    queryBuilder.Append($"{pair.Key} {value} PRIMARY KEY, ");
                }
                else
                {
                    queryBuilder.Append($"{pair.Key} {value}, ");
                }

                // MinimumLength?
                if (pair.Value.MinLength > 0)
                {
                    queryBuilder.Append($"CHECK(LENGTH({pair.Key}) >= {pair.Value.MinLength}), ");
                }
            }

            queryBuilder.Length -= 2; // Remove the last comma and space
            queryBuilder.Append(")");
            return queryBuilder.ToString();
        }

    }
}
