using EH.Command;
using EH.Connection;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;

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
        /// Executes a collection of SQL non-query commands (e.g., INSERT, UPDATE, DELETE).
        /// </summary>
        /// <param name="DbContext">The database context where the commands will be executed.</param>
        /// <param name="queries">The collection of SQL queries to execute.</param>
        /// <param name="expectedChanges">(Optional) The expected number of rows affected by each command. If the actual number of affected rows does not match, the transaction is rolled back, and an exception is thrown.</param>
        /// <returns>
        /// A collection of integers representing the number of rows affected by each executed command.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the database connection is null, or when the queries collection is null or contains an empty query.
        /// </exception>
        /// <exception cref="Exception">
        /// Thrown when a SQL error occurs, including specific handling for the 'ORA-00942' error.
        /// </exception>
        internal static ICollection<int> ExecuteNonQuery(this Database DbContext, ICollection<string?> queries, int expectedChanges = -1)
        {
            if (DbContext?.IDbConnection is null) { throw new InvalidOperationException("Connection does not exist."); }
            if (queries is null || queries.Count == 0) { throw new InvalidOperationException("Queries do not exist."); }

            IDbConnection connection = DbContext.CreateOpenConnection();
            IDbTransaction? transaction = DbContext.CreateTransaction() ?? throw new InvalidOperationException("Transaction is null.");
            string tableName = string.Empty;

            try
            {
                ICollection<int> result = new List<int>();

                foreach (var query in queries)
                {
                    if (string.IsNullOrEmpty(query?.Trim())) { throw new ArgumentNullException(nameof(query), "Query cannot be null or empty."); }

                    var tables = GetTbDependence(query);
                    tableName = string.Join(" and ", tables);

                    using IDbCommand command = DbContext.CreateCommand(query);
                    command.Transaction = transaction;

                    int rowsAffected = command.ExecuteNonQuery();
                    //Debug.WriteLine(query);                   

                    result.Add(rowsAffected);
                }

                int changes = result.Sum();
                if (expectedChanges != -1 && changes != expectedChanges)
                {
                    throw new InvalidOperationException($"Expected {expectedChanges} changes, but {changes} were made.");
                }

                transaction.Commit(); // Possible only for DML
                return result;
            }
            catch (Exception ex)
            {
                transaction.Rollback(); // Not applicable to DDL
                //connection.Close();

                if (ex.Message.Contains("ORA-00942") && !string.IsNullOrEmpty(tableName))
                {
                    throw new Exception($"E-942-EH: Table or view '{tableName}' must exist!", ex);
                }
                throw;
            }
            finally
            {
                if (connection.State == ConnectionState.Open) connection.Close();
            }

            static List<string> GetTbDependence(string query)
            {
                var tables = new List<string>();

                // 'FROM' e 'JOIN'
                var fromMatches = Regex.Matches(query, @"\b(?:FROM|JOIN)\s+([^\s,]+)", RegexOptions.IgnoreCase);
                foreach (Match match in fromMatches)
                {
                    if (match.Success)
                    {
                        tables.Add(match.Groups[1].Value);
                    }
                }

                // 'FOREIGN KEY REFERENCES'
                var foreignKeyMatches = Regex.Matches(query, @"\bFOREIGN\s+KEY\s+\([^\)]+\)\s+REFERENCES\s+([^\s\(]+)", RegexOptions.IgnoreCase);
                foreach (Match match in foreignKeyMatches)
                {
                    if (match.Success)
                    {
                        tables.Add(match.Groups[1].Value);
                    }
                }

                return tables;
            }
        }

        /// <summary>
        /// Executes a collection of SQL queries within a transaction using the provided database context,
        /// and returns the results of the executed scalar queries as a collection of objects.
        /// Each query must include an output parameter named ":Result" to capture the return value.
        /// </summary>
        /// <param name="DbContext">The database context containing the connection and transaction information.</param>
        /// <param name="queries">A collection of SQL query strings to be executed as scalar commands. 
        /// Each query must contain an output parameter named ":Result" for capturing the result.</param>
        /// <returns>A collection of objects representing the results of each scalar query executed.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the database connection or transaction is null.</exception>
        /// <exception cref="ArgumentNullException">Thrown if the queries collection or any individual query is null or empty.</exception>
        /// <exception cref="Exception">Thrown if an error occurs during the execution of the queries, triggering a transaction rollback.</exception>
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

                    //var parameter = new OracleParameter(":Result", OracleDbType.Int32) { Direction = ParameterDirection.Output }; // Result can be varchar
                    //var parameter = new OracleParameter("Result", OracleDbType.Varchar2,4000) { Direction = ParameterDirection.Output }; // Result OK
                    //command.Parameters.Add(parameter);

                    // Generic Parameter
                    var parameter = command.CreateParameter();
                    parameter.ParameterName = "Result";
                    parameter.DbType = DbType.String; // Int32, String, etc
                    parameter.Size = 4000; // Mandatory
                    parameter.Direction = ParameterDirection.Output;
                    command.Parameters.Add(parameter);

                    var result = command.ExecuteScalar();
                    //Debug.WriteLine(query);

                    //Debug.WriteLine($"Result: {resultParam.Value}");
                    results.Add(parameter.Value ?? result);
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

        /// <summary>
        /// Executes a bulk copy operation to transfer data from the provided input source to a specified table in the database.
        /// The input data can be of type DataRow[], DataTable, or IDataReader, and the appropriate bulk copy mechanism will be used
        /// based on the underlying database provider (e.g., OracleBulkCopy, SqlBulkCopy).
        /// </summary>
        /// <param name="dbContext">The database context used to establish the connection and perform the bulk copy operation.</param>
        /// <param name="inputDataToCopy">The data to be copied, which can be of type DataRow[], DataTable, or IDataReader.</param>
        /// <param name="tableName">The name of the destination table where the data will be copied.</param>
        /// <param name="bulkCopyTimeout">The timeout duration (in seconds) for the bulk copy operation.</param>
        /// <returns>Returns true if the bulk copy operation is successful; otherwise, throws an exception.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the database context, input data, or table name is null or empty.</exception>
        /// <exception cref="NotSupportedException">Thrown if the bulk copy operation is not supported for the provided data type or database provider.</exception>
        /// <exception cref="Exception">Thrown if an error occurs during the bulk copy operation, causing the connection to close and the exception to be rethrown.</exception>
        internal static int PerformBulkCopyOperation(this Database dbContext, object inputDataToCopy, string tableName, int bulkCopyTimeout)
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
                        if (inputDataToCopy is DataRow[] dataRowsOracle) { oracleBulkCopy.WriteToServer(dataRowsOracle); return dataRowsOracle.Length; }
                        else if (inputDataToCopy is DataTable dataTable) { oracleBulkCopy.WriteToServer(dataTable); return dataTable.Rows.Count; }
                        else if (inputDataToCopy is IDataReader dataReader) { oracleBulkCopy.WriteToServer(dataReader); return dataReader.RecordsAffected; }
                        else { throw new NotSupportedException("Bulk Copy operation is not yet supported for this data type."); }

                    case SqlBulkCopy sqlBulkCopy:
                        sqlBulkCopy.DestinationTableName = tableName;
                        sqlBulkCopy.BulkCopyTimeout = bulkCopyTimeout;
                        if (inputDataToCopy is DataRow[] dataRowsSql) { sqlBulkCopy.WriteToServer(dataRowsSql); return dataRowsSql.Length; }
                        else if (inputDataToCopy is DataTable dataTable) { sqlBulkCopy.WriteToServer(dataTable); return dataTable.Rows.Count; }
                        else if (inputDataToCopy is IDataReader dataReader) { sqlBulkCopy.WriteToServer(dataReader); return dataReader.RecordsAffected; }
                        else { throw new NotSupportedException("Bulk Copy operation is not yet supported for this data type."); }

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
