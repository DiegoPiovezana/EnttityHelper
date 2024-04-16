using EH.Connection;
using EH.Properties;
using System;
using System.Collections.Generic;
using System.Data;

namespace EH.Commands
{
    internal static class Execute
    {
        /// <summary>
        /// Executes a SQL command, either non-query or select, based on the provided query.
        /// </summary>
        /// <typeparam name="TEntity">The type of entities to retrieve.</typeparam>
        /// <param name="query">The SQL query to execute.</param>
        /// <param name="DbContext">Database where the entities will be manipulated.</param>
        /// <param name="isNonQuery">Flag indicating whether the command is a non-query (true) or select (false).</param>  
        /// <param name="getDataReader">If true and it is a select, it will return a dataReader filled with the result obtained.</param> 
        /// <returns>
        /// - If the command is a non-query, returns the number of affected rows.
        /// - If the command is a select, returns a list of entities retrieved from the database.
        /// </returns>
        internal static object? ExecuteCommand<TEntity>(this Database DbContext, string? query, bool isNonQuery = false, bool getDataReader = false)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query))
                {
                    throw new ArgumentNullException(nameof(query), "Query cannot be null or empty.");
                }

                if (DbContext?.IDbConnection is null)
                {
                    throw new InvalidOperationException("Connection does not exist.");
                }

                IDbConnection connection = DbContext.CreateConnection();
                connection.Open();

                using IDbCommand command = DbContext.CreateCommand(query);

                if (isNonQuery)
                {
                    int rowsAffected = command.ExecuteNonQuery();
                    connection.Close();
                    Console.WriteLine($"Rows Affected: {rowsAffected}");
                    return rowsAffected;
                }
                else // isSelect
                {
                    using var reader = command.ExecuteReader();
                    if (getDataReader) return reader;
                    if (reader != null)
                    {
                        List<TEntity> entities = ToolsEH.MapDataReaderToList<TEntity>(reader);
                        connection.Close();
                        Console.WriteLine($"{(entities?.Count) ?? 0} entities mapped!");
                        return entities;
                    }

                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }





    }
}
