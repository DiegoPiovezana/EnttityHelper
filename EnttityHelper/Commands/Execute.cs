﻿using EH.Connection;
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
        /// <param name="isNonQuery">(Optional) Flag indicating whether the command is a non-query (true) or select (false).</param>  
        /// <param name="getDataReader">(Optional) If true and it is a select, it will return a dataReader filled with the result obtained.</param> 
        /// <param name="expectedChanges">(Optional) Expected amount of changes to the database. If the amount of changes is not expected, the change will be rolled back and an exception will be thrown.</param> 
        /// <returns>
        /// - If the command is a non-query, returns the number of affected rows.
        /// - If the command is a select, returns a list of entities retrieved from the database.
        /// </returns>
        internal static object? ExecuteCommand<TEntity>(this Database DbContext, string? query, bool isNonQuery = false, bool getDataReader = false, int expectedChanges = -1)
        {
            try
            {
                if (string.IsNullOrEmpty(query?.Trim()))
                {
                    throw new ArgumentNullException(nameof(query), "Query cannot be null or empty.");
                }

                if (DbContext?.IDbConnection is null)
                {
                    throw new InvalidOperationException("Connection does not exist.");
                }

                using IDbConnection connection = DbContext.CreateConnection();
                DbContext.OpenConnection();

                IDbTransaction? transaction = DbContext.CreateTransaction() ?? throw new InvalidOperationException("Transaction is null.");

                try
                {
                    using IDbCommand command = DbContext.CreateCommand(query);
                    command.Transaction = transaction;

                    if (isNonQuery)
                    {
                        int rowsAffected = command.ExecuteNonQuery();

                        if (expectedChanges != -1 && rowsAffected != expectedChanges)
                        {
                            transaction.Rollback();
                            connection.Close();
                            throw new InvalidOperationException($"Expected {expectedChanges} changes, but {rowsAffected} were made.");
                        }

                        transaction.Commit();
                        connection.Close();
                        Console.WriteLine($"Rows Affected: {rowsAffected}");
                        return rowsAffected;
                    }
                    else // isSelect
                    {
                        using var reader = command.ExecuteReader();
                        transaction.Commit();

                        if (getDataReader)
                        {
                            return reader;
                        }

                        if (reader != null)
                        {
                            List<TEntity> entities = ToolsEH.MapDataReaderToList<TEntity>(reader);
                            reader.Close();
                            connection.Close();
                            Console.WriteLine($"{(entities?.Count) ?? 0} entities mapped!");
                            return entities;
                        }

                        return null;
                    }
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
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
