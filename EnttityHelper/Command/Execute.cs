using EH.Command;
using EH.Connection;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;

namespace EH.Commands
{
    internal static class Execute
    {
        /// <summary>
        /// Executes a SQL command, either non-query or select, based on the provided query.
        /// </summary>
        /// <typeparam name="TEntity">The type of entities to retrieve.</typeparam>
        /// <param name="DbContext">Database where the entities will be manipulated.</param>
        /// <param name="getDataReader">(Optional) If true and it is a select, it will return a dataReader filled with the result obtained.</param>       
        /// <returns>
        /// If the command is a select, returns a list of entities retrieved from the database.
        /// </returns>
        internal static object? ExecuteReader<TEntity>(this Database DbContext, string? query, bool getDataReader = false)
        {
            if (DbContext?.IDbConnection is null) { throw new InvalidOperationException("Connection does not exist."); }
            if (query is null) { throw new InvalidOperationException("Query does not exist."); }

            IDbConnection connection = DbContext.CreateOpenConnection();
            using IDbCommand command = DbContext.CreateCommand(query);
            IDataReader? reader = command.ExecuteReader();

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

        /// <summary>
        /// Executes a SQL command, either non-query or select, based on the provided query.
        /// </summary>
        /// <typeparam name="TEntity">The type of entities to retrieve.</typeparam>
        /// <param name="queries">The SQL query collection to execute.</param>
        /// <param name="DbContext">Database where the entities will be manipulated.</param>        
        /// <param name="expectedChanges">(Optional) Expected amount of changes to the database. If the amount of changes is not expected, the change will be rolled back and an exception will be thrown.</param> 
        /// <returns>
        /// Returns the number of affected rows.       
        /// </returns>
        internal static ICollection<int> ExecuteNonQuery<TEntity>(this Database DbContext, ICollection<string?> queries, int expectedChanges = -1)
        {
            try
            {
                if (DbContext?.IDbConnection is null) { throw new InvalidOperationException("Connection does not exist."); }
                if (queries is null) { throw new InvalidOperationException("Queries do not exist."); }

                IDbConnection connection = DbContext.CreateOpenConnection();
                IDbTransaction? transaction = DbContext.CreateTransaction() ?? throw new InvalidOperationException("Transaction is null.");

                try
                {
                    ICollection<int> result = new List<int>();

                    foreach (var query in queries)
                    {
                        if (string.IsNullOrEmpty(query?.Trim())) { throw new ArgumentNullException(nameof(query), "Query cannot be null or empty."); }

                        using IDbCommand command = DbContext.CreateCommand(query);
                        command.Transaction = transaction;

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
                        result.Add(rowsAffected);
                    }

                    return result;
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

        internal static ICollection<object?> ExecuteScalar(this Database DbContext, ICollection<string?> queries)
        {
            if (DbContext?.IDbConnection is null) { throw new InvalidOperationException("Connection does not exist."); }
            if (queries is null) throw new ArgumentNullException(nameof(queries), "Queries do not exist.");

            IDbConnection connection = DbContext.CreateOpenConnection();
            IDbTransaction? transaction = DbContext.CreateTransaction() ?? throw new InvalidOperationException("Transaction is null.");

            try
            {
                ICollection<object?> results = new List<object?>();

                foreach (var query in queries)
                {
                    if (string.IsNullOrEmpty(query?.Trim())) throw new ArgumentNullException(nameof(query), "Query cannot be null or empty.");

                    using IDbCommand command = DbContext.CreateCommand(query);
                    command.Transaction = transaction;

                    var resultParam = new OracleParameter(":Result", OracleDbType.Int32) { Direction = ParameterDirection.Output };
                    command.Parameters.Add(resultParam);

                    command.ExecuteScalar();                    

                    Debug.WriteLine($"Result: {resultParam.Value}");
                    results.Add(resultParam.Value);
                }

                transaction.Commit();
                connection.Close();

                return results;
            }
            catch (Exception)
            {
                transaction.Rollback();
                throw;
            }
            finally
            {
                if (DbContext.IDbConnection is not null && DbContext.IDbConnection.State == ConnectionState.Open)
                    DbContext.IDbConnection.Close();
            }
        }

        internal static bool PerformBulkCopyOperation(this Database dbContext, object inputDataToCopy, string tableName, int bulkCopyTimeout)
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
                        oracleBulkCopy.BulkCopyTimeout = bulkCopyTimeout;
                        if (inputDataToCopy is DataRow[] dataRowsOracle) { oracleBulkCopy.WriteToServer(dataRowsOracle); }
                        else if (inputDataToCopy is DataTable dataTable) { oracleBulkCopy.WriteToServer(dataTable); }
                        else if (inputDataToCopy is IDataReader dataReader) { oracleBulkCopy.WriteToServer(dataReader); }
                        else { throw new NotSupportedException("Bulk Copy operation is not yet supported for this data type."); }
                        return true;

                    case SqlBulkCopy sqlBulkCopy:
                        sqlBulkCopy.DestinationTableName = tableName;
                        sqlBulkCopy.BulkCopyTimeout = bulkCopyTimeout;
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
