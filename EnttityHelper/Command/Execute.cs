using EH.Command;
using EH.Connection;
using EH.Properties;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Text;

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

                IDbConnection connection = DbContext.CreateConnection();
                connection.Open();

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
                        Debug.WriteLine($"Rows Affected: {rowsAffected}");
                        return rowsAffected;
                    }
                    else // isSelect
                    {
                        IDataReader? reader = command.ExecuteReader();
                        transaction.Commit();

                        if (getDataReader) { return reader; }

                        if (reader != null)
                        {
                            List<TEntity> entities = Tools.ToListEntity<TEntity>(reader);
                            reader.Close();
                            connection.Close();
                            Debug.WriteLine($"{(entities?.Count) ?? 0} entities mapped!");
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
                Debug.WriteLine(ex.Message);
                throw;
            }
        }

        internal static bool PerformBulkCopyOperation(this Database dbContext, object inputDataToCopy, string tableName)
        {
            // dataToCopy: DataRow[], DataTable, IDataReader

            if (dbContext is null)
            {
                throw new ArgumentNullException(nameof(dbContext), "Database context cannot be null.");
            }

            if (inputDataToCopy is null)
            {
                throw new ArgumentNullException(nameof(inputDataToCopy), "Data to copy cannot be null.");
            }

            if (string.IsNullOrEmpty(tableName?.Trim()))
            {
                throw new ArgumentNullException(nameof(tableName), "Table name cannot be null or empty.");
            }

            try
            {
                using IDbConnection connection = dbContext.CreateConnection();
                connection.Open();

                var bulkCopyObject = dbContext.CreateBulkCopy();

                switch (bulkCopyObject)
                {
                    case OracleBulkCopy oracleBulkCopy:
                        oracleBulkCopy.DestinationTableName = tableName;
                        if (inputDataToCopy is DataRow[] dataRowsOracle) { oracleBulkCopy.WriteToServer(dataRowsOracle); }
                        else if (inputDataToCopy is DataTable dataTable) { oracleBulkCopy.WriteToServer(dataTable); }
                        else if (inputDataToCopy is IDataReader dataReader) { oracleBulkCopy.WriteToServer(dataReader); }
                        else { throw new NotSupportedException("Bulk Copy operation is not yet supported for this data type."); }
                        return true;

                    case SqlBulkCopy sqlBulkCopy:
                        sqlBulkCopy.DestinationTableName = tableName;
                        if (inputDataToCopy is DataRow[] dataRowsSql) { sqlBulkCopy.WriteToServer(dataRowsSql); }
                        else if (inputDataToCopy is DataTable dataTable) { sqlBulkCopy.WriteToServer(dataTable); }
                        else if (inputDataToCopy is IDataReader dataReader) { sqlBulkCopy.WriteToServer(dataReader); }
                        else { throw new NotSupportedException("Bulk Copy operation is not yet supported for this data type."); }
                        return true;

                    default:
                        throw new NotSupportedException("Bulk Copy operation is not yet supported for this database type.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error when performing the Bulk Copy operation: " + ex.Message);
                dbContext.CloseConnection();
                throw;
            }
        }





    }
}
