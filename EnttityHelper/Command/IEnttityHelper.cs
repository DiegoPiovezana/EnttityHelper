using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;

namespace EH.Command
{
    interface IEnttityHelper
    {
        /// <summary>
        /// Inserts data into the database, supporting multiple data formats and advanced configurations.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity or data structure being inserted.</typeparam>
        /// <param name="entity">
        /// The data to be inserted, which can be an entity, a collection of entities, a DataTable, IDataReader, or an array of DataRow.
        /// </param>
        /// <param name="setPrimaryKeyAfterInsert">
        /// If set to <c>true</c>, the primary key of the inserted entity will be set after successful insertion (default: <c>true</c>).
        /// </param>
        /// <param name="namePropUnique">
        /// (Optional) The name of a property used to ensure unique entries in the database. 
        /// If specified, the method checks for duplicates based on this property before inserting.
        /// </param>
        /// <param name="createTable">
        /// Indicates whether to create the target table if it does not exist. If set to <c>true</c>, 
        /// the table will be created automatically.
        /// </param>
        /// <param name="tableName">
        /// (Optional) The name of the target table in the database. If not provided, the table name will 
        /// be inferred based on the entity's metadata or data structure.
        /// </param>
        /// <param name="ignoreInversePropertyProperties">
        /// Determines whether inverse property relationships should be ignored during the insertion process. 
        /// If set to <c>true</c>, related properties marked with inverse relationships will be skipped.
        /// </param>
        /// <param name="timeOutSeconds">
        /// The timeout duration, in seconds, for the operation. This is particularly relevant for bulk insertions.
        /// </param>
        /// <returns>
        /// The number of rows successfully inserted into the database.
        /// </returns>
        /// <remarks>
        /// - Supports various data formats such as single entities, collections of entities, DataTable, 
        /// IDataReader, and arrays of DataRow.
        /// - Automatically validates and creates foreign key or many-to-many (MxN) relationship tables if necessary.
        /// - Ensures no duplicate entries are inserted when <paramref name="namePropUnique"/> is specified.
        /// - Suitable for both single insertions and bulk operations.
        /// </remarks>
        /// <exception cref="ArgumentNullException">Thrown if required parameters like <paramref name="tableName"/> are null in certain contexts.</exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the entity is invalid, related tables do not exist and <paramref name="createTable"/> is set to <c>false</c>, 
        /// or if duplicate entries are detected when <paramref name="namePropUnique"/> is specified.
        /// </exception>
        /// <exception cref="Exception">Thrown for other errors, such as missing insert queries or execution failures.</exception>
        public long Insert<TEntity>(TEntity entity, bool setPrimaryKeyAfterInsert = true, string? namePropUnique = null, bool createTable = true, string? tableName = null, bool ignoreInversePropertyProperties = false, int timeOutSeconds = 600) where TEntity : class;

        /// <summary>
        /// Executes a SELECT query on one database and inserts the result into another database (or in the same).
        /// </summary>
        /// <param name="selectQuery">The SELECT query to be executed.</param>
        /// <param name="db2">The instance of the EntityHelper representing the destination database.</param>
        /// <param name="tableName">The name of the table where the result will be inserted.</param>
        /// <param name="timeOutSeconds">(Optional) Maximum time (in seconds) to wait for the insertion to occur. By default, the maximum time is up to 10 minutes.</param>
        /// <returns>The number of rows inserted into the destination table.</returns>
        public long InsertLinkSelect(string selectQuery, EnttityHelper db2, string tableName, int timeOutSeconds = 600);

        /// <summary>
        /// Loads a CSV or TXT file into the database.
        /// </summary>
        /// <param name="csvFilePath">The path to the CSV/TXT file.</param>
        /// <param name="createTable">Indicates whether to create a new table if it does not exist.</param>
        /// <param name="tableName">The name of the table to load the data into. If null, a default name will be used.</param>
        /// <param name="batchSize">The maximum number of rows to load at once. Default is 100000. The bigger it is, the faster it is, but the higher the memory consumption will be.</param>
        /// <param name="timeOutSeconds">The timeout duration for the operation in seconds. Default is 600 seconds.</param>
        /// <param name="delimiter">The delimiter character used in the CSV/TXT file. Default is ';'</param>
        /// <param name="hasHeader">Indicates whether the CSV/TXT file contains headers. Default is true.</param>
        /// <param name="rowsToLoad">Enter the rows or their range. E.g.: "1:23, 34:-56, 70, 75, -1". For default, all rows will be loaded. ATENTION: Order and duplicates will not be considered!</param>
        /// <param name="encodingRead">The encoding to use when reading the CSV/TXT file. Default is UTF8. Example: Encoding.GetEncoding("ISO-8859-1")</param>
        /// <returns>The number of records inserted into the database.</returns>
        /// <exception cref="ArgumentException">Thrown when the CSV/TXT file is invalid or cannot be loaded.</exception>
        long LoadCSV(string csvFilePath, bool createTable = true, string? tableName = null, int batchSize = 100000, int timeOutSeconds = 600, char delimiter = ';', bool hasHeader = true, string? rowsToLoad = null, Encoding? encodingRead = null);

        /// <summary>
        /// Allow to update an entity in the database.
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to be manipulated.</typeparam>
        /// <param name="entity">Entity to be updated in the database.</param>
        /// <param name="nameId">(Optional) Name of the column in which the entity will be identified to be updated.</param>      
        /// <param name="tableName">(Optional) Name of the table to which the entity will be inserted. By default, the table informed in the "Table" attribute of the entity class will be considered.</param> 
        /// <param name="ignoreInversePropertyProperties">(Optional) If true, properties that are part of an inverse property will be ignored.</param>
        /// <returns>Number of entities updated in the database.</returns>
        public long Update<TEntity>(TEntity entity, string? nameId = null, string? tableName = null, bool ignoreInversePropertyProperties = false) where TEntity : class;

        /// <summary>
        /// Retrieves one or more entities from the database with optional filtering, pagination, and sorting.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity to retrieve.</typeparam>
        /// <param name="includeAll">(Optional) If true, all related entities (foreign keys and inverse properties) will be included. Default is true.</param>
        /// <param name="filter">(Optional) A filter string to specify search criteria for the entities.</param>
        /// <param name="tableName">(Optional) The name of the table where the entities are stored. If not provided, the table name will be inferred from the "Table" attribute on the entity class.</param>
        /// <param name="pageSize">(Optional) The number of entities to retrieve per page. If null, no pagination will be applied.</param>
        /// <param name="pageIndex">(Optional) The zero-based index of the page to retrieve. Default is 0.</param>
        /// <param name="sortColumn">(Optional) The name of the column by which to sort the results. If null, no sorting will be applied.</param>
        /// <param name="sortAscending">(Optional) Indicates whether the sorting should be in ascending order. Default is true.</param>
        /// <returns>A list of entities that match the specified criteria.</returns>
        public List<TEntity>? Get<TEntity>(bool includeAll = true, string? filter = null, string? tableName = null, int? pageSize = null, int pageIndex = 0, string? sortColumn = null, bool sortAscending = true) where TEntity : class;

        /// <summary>
        /// Search the specific entity by <paramref name="idPropName"/>
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to be manipulated.</typeparam>
        /// <param name="entity">Entity to be searched for in the bank.</param>
        /// <param name="includeAll">(Optional) Defines whether it will include all other FK entities (by default it will include all entities).</param>
        /// <param name="idPropName">(Optional) Entity identifier name.</param>
        /// <param name="tableName">(Optional) Name of the table to which the entity will be inserted. By default, the table informed in the "Table" attribute of the entity class will be considered.</param> 
        /// <returns>Specific entity from database.</returns>
        public TEntity? Search<TEntity>(TEntity entity, bool includeAll = true, string? idPropName = null, string? tableName = null) where TEntity : class;

        /// <summary>
        /// Checks if table exists (>= 0) and it is filled (> 0).
        /// </summary>
        /// <param name="tableName">Name of the table to check if it exists.</param>
        /// <param name="minRecords">(Optional) The minimum number of records to check for existence in the table. Enter 0 if you just want to check if the table exists.</param>
        /// <param name="filter">(Optional) Possible filter.</param>
        /// <returns>True, if table exists and (optionally) it is filled</returns>
        public bool CheckIfExist(string tableName, int minRecords = 0, string? filter = null);

        /// <summary>
        /// Checks if the specified entity exists in the database.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity to check.</typeparam>
        /// <param name="entity">The entity to check.</param>
        /// <param name="tableName">(Optional) The name of the table to query. If null, the method will attempt to identify the table name based on the entity type.</param>
        /// <param name="nameId">(Optional) The name of the primary key or identifier property for filtering. If null, the method will attempt to identify the primary key automatically.</param>
        /// <returns>True if the entity exists in the database, false otherwise.</returns>
        public bool CheckIfExist<TEntity>(TEntity entity, string? tableName = null, string? nameId = null) where TEntity : class;

        /// <summary>
        /// Checks if the specified table exists and returns the count of records for the given entity/entities in the table.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity to check.</typeparam>
        /// <param name="entity">
        /// The entity or collection of entities to check in the database. If it's a collection, it will process each entity individually.
        /// </param>
        /// <param name="tableName">
        /// (Optional) The name of the table to query. If null, the method will attempt to identify the table name based on the entity type.
        /// </param>
        /// <param name="nameId">
        /// (Optional) The name of the primary key or identifier property for filtering. If null, the method will attempt to identify the primary key automatically.
        /// </param>
        /// <returns>
        /// A long integer indicating the count of records in the specified table that match the entity/entities provided.
        /// Returns -1 if the table does not exist (i.e., database-specific exceptions for "table does not exist").
        /// </returns>
        /// <exception cref="Exception">
        /// Throws an exception if an unexpected error occurs during execution.
        /// </exception>
        public long CountEntity<TEntity>(TEntity entity, string? tableName = null, string? nameId = null) where TEntity : class;


        /// <summary>
        /// Counts the number of records in a specified table, with an optional filter for conditional counting.
        /// Returns the count of matching records or a specific error code if the table does not exist.
        ///
        /// <para>
        /// Uses the value -1 as a return to indicate the absence of the table in the database.
        /// </para>
        ///
        /// <para>
        /// Example usage:
        /// <code>
        /// int recordCount = enttityHelper.CountTable("Users", "IsActive = 1");
        /// </code>
        /// </para>
        ///
        /// </summary>
        /// <param name="tableName">The name of the table to count records in.</param>
        /// <param name="filter">An optional filter to apply to the count query. If null, all records are counted.</param>
        /// <returns>The count of records matching the criteria, or -1 if the table does not exist.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the database connection is not established.</exception>
        /// <exception cref="Exception">Rethrows any other exceptions encountered during query execution.</exception>
        public long CountTable(string tableName, string? filter = null);

        /// <summary>
        /// Asynchronously retrieves the total number of records from the database based on a base query and an optional filter.
        /// </summary>
        /// <param name="baseQuery">The base SQL query to determine the total number of records. This query should be structured to allow a COUNT operation.</param>
        /// <param name="filter">(Optional) A filter to be applied to the query to refine the count of records.</param>
        /// <returns>
        /// The total number of records as an integer. 
        /// Returns -1 if the table or view referenced in the query does not exist.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="baseQuery"/> is null or empty.</exception>
        /// <exception cref="Exception">Re-throws any unexpected exceptions that occur during execution.</exception>
        /// <remarks>
        /// This method internally constructs a count query using the base query and applies the optional filter.
        /// It handles database-specific exceptions (e.g., Oracle and SQL Server) to return a specific value for missing tables or views.
        /// </remarks>
        public Task<long> GetTotalRecordCountAsync(string baseQuery, string? filter = null);

        /// <summary>
        /// Allows you to create a table in the database according to the provided objectEntity object.
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to create the table.</typeparam>
        /// <param name="createOnlyPrimaryTable">If true, tables used for M:N relationships, for example, will not be created. Attention: Entity 2 table must already exist!</param>
        /// <param name="ignoreProps">(Optional) A collection of property names to ignore when creating the table.</param>
        /// <param name="tableName">(Optional) Name of the table to which the entity will be inserted. By default, the table informed in the "Table" attribute of the entity class will be considered.</param> 
        /// <returns>True, if table was created and false, if not created.</returns>
        /// <exception cref="InvalidOperationException">Occurs if the table should have been created but was not.</exception>      
        public bool CreateTable<TEntity>(bool createOnlyPrimaryTable, ICollection<string>? ignoreProps = null, string? tableName = null);

        /// <summary>
        /// Creates a table for the specified entity if it does not already exist in the database.
        /// </summary>
        /// <typeparam name="TEntity">The type of entity for which to create the table.</typeparam>
        /// <param name="createOnlyPrimaryTable">Specifies whether to create only the primary table or include auxiliary tables for relationships. Attention: Entity 2 table must already exist!</param>
        /// <param name="ignoreProps">A collection of property names to ignore when creating the table.</param>
        /// <param name="tableName">The name of the table. If not provided, the name will be inferred from the entity type.</param>
        /// <returns>True if the table was created or already exists, otherwise false.</returns>
        public bool CreateTableIfNotExist<TEntity>(bool createOnlyPrimaryTable, ICollection<string>? ignoreProps = null, string? tableName = null);

        /// <summary>
        /// Creates a table in the database based on the structure specified in a DataTable object.
        /// </summary>
        /// <param name="dataTable">The DataTable object containing the structure of the table to be created.</param>
        /// <param name="tableName">(Optional) The name of the table to be created. If not specified, the name from the DataTable object will be used.</param>
        /// <returns>True if the table is created successfully, otherwise False.</returns>
        /// <exception cref="InvalidOperationException">An exception is thrown if an error occurs while attempting to create the table.</exception>
        public bool CreateTable(DataTable dataTable, string? tableName = null);

        /// <summary>
        /// Creates a table for the specified DataTable if it does not already exist in the database.
        /// </summary>
        /// <param name="dataTable">The DataTable representing the table to create.</param>
        /// <param name="tableName">The name of the table. If not provided, the name will be inferred from the DataTable's TableName property.</param>
        /// <returns>True if the table was created or already exists, otherwise false.</returns>
        public bool CreateTableIfNotExist(DataTable dataTable, string? tableName = null);

        /// <summary>
        /// Allow to delete an entity in the database.
        /// </summary>
        /// <param name="entity">Entity that will have its FK entities included.</param>
        /// <param name="nameId">(Optional) Entity ID column name. By default, PK will be used.</param>
        /// <param name="tableName">(Optional) Name of the table to which the entity will be inserted. By default, the table informed in the "Table" attribute of the entity class will be considered.</param> 
        /// <returns>Number of exclusions made.</returns>
        public long Delete<TEntity>(TEntity entity, string? nameId = null, string? tableName = null) where TEntity : class;

        /// <summary>
        /// Executes the non query (Create, Alter, Drop, Insert, Update or Delete) on the database.
        /// </summary>
        /// <param name="query">Query to be executed.</param>
        /// <param name="expectedChanges">(Optional) Expected amount of changes to the database. If the amount of changes is not expected, the change will be rolled back and an exception will be thrown.</param> 
        /// <returns>Number of affected rows.</returns>
        public long ExecuteNonQuery(string? query, int expectedChanges = -1);

        /// <summary>
        /// Executes a SELECT query and retrieves a list of mapped entities, with optional support for pagination.
        /// </summary>
        /// <typeparam name="TEntity">The type of entity to map the retrieved data to.</typeparam>
        /// <param name="query">The SQL SELECT query to be executed.</param>
        /// <param name="pageSize">
        /// (Optional) The number of records to retrieve per page. If specified, the query will be paginated.
        /// </param>
        /// <param name="pageIndex">
        /// The zero-based index of the page to retrieve. Ignored if <paramref name="pageSize"/> is <c>null</c>.
        /// Defaults to 0.
        /// </param>
        /// <param name="filterPage">
        /// (Optional) Additional filtering criteria for the paginated query. Applied only if <paramref name="pageSize"/> is specified.
        /// </param>
        /// <param name="sortColumnPage">
        /// (Optional) The column name to sort the paginated query. Applied only if <paramref name="pageSize"/> is specified.
        /// </param>
        /// <param name="sortAscendingPage">
        /// Determines the sorting order for the paginated query. 
        /// <c>true</c> for ascending order; <c>false</c> for descending order. Defaults to <c>true</c>.
        /// Applied only if <paramref name="pageSize"/> is specified.
        /// </param>
        /// <returns>
        /// A list of mapped entities of type <typeparamref name="TEntity"/>, or <c>null</c> if no data is retrieved.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the <paramref name="query"/> is <c>null</c> or invalid.
        /// </exception>
        /// <remarks>
        /// - If <paramref name="pageSize"/> is not specified, the query will execute without pagination.
        /// - The method uses the specified filtering and sorting options only if <paramref name="pageSize"/> is provided.
        /// - Ensure that <paramref name="query"/> is a valid SQL SELECT statement.
        /// </remarks>
        public List<TEntity>? ExecuteSelect<TEntity>(string? query, int? pageSize = null, int pageIndex = 0, string? filterPage = null, string? sortColumnPage = null, bool sortAscendingPage = true);

        /// <summary>
        /// Executes a SELECT query and retrieves the results as a <see cref="DataTable"/>, with optional support for pagination.
        /// </summary>
        /// <param name="query">The SQL SELECT query to be executed.</param>
        /// <param name="pageSize">
        /// (Optional) The number of records to retrieve per page. If specified, the query will be paginated.
        /// </param>
        /// <param name="pageIndex">
        /// The zero-based index of the page to retrieve. Ignored if <paramref name="pageSize"/> is <c>null</c>.
        /// Defaults to 0.
        /// </param>
        /// <param name="filterPage">
        /// (Optional) Additional filtering criteria for the paginated query. Applied only if <paramref name="pageSize"/> is specified.
        /// </param>
        /// <param name="sortColumnPage">
        /// (Optional) The column name to sort the paginated query. Applied only if <paramref name="pageSize"/> is specified.
        /// </param>
        /// <param name="sortAscendingPage">
        /// Determines the sorting order for the paginated query. 
        /// <c>true</c> for ascending order; <c>false</c> for descending order. Defaults to <c>true</c>.
        /// Applied only if <paramref name="pageSize"/> is specified.
        /// </param>
        /// <returns>
        /// A <see cref="DataTable"/> containing the query results, or <c>null</c> if no data is retrieved.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the <paramref name="query"/> is <c>null</c> or invalid.
        /// </exception>
        /// <exception cref="Exception">
        /// Thrown for any other error encountered during query execution.
        /// </exception>
        /// <remarks>
        /// - If <paramref name="pageSize"/> is not specified, the query will execute without pagination.
        /// - The method uses the specified filtering and sorting options only if <paramref name="pageSize"/> is provided.
        /// - Closes the database connection after execution, regardless of success or failure.
        /// </remarks>
        public DataTable? ExecuteSelectDt(string? query, int? pageSize = null, int pageIndex = 0, string? filterPage = null, string? sortColumnPage = null, bool sortAscendingPage = true);

        /// <summary>
        /// Executes a SQL query and returns the scalar result as a string.
        /// </summary>
        /// <param name="query">The SQL query to execute.</param>
        /// <returns>
        /// A string representing the scalar result of the query. Returns an empty string if the result is null.
        /// Returns null if the connection to the database cannot be established.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when the provided query is null or empty.</exception>
        /// <exception cref="Exception">Thrown when an error occurs during query execution.</exception>
        public object? ExecuteScalar(string? query);

        /// <summary>
        /// Include all FK entities.
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to be manipulated.</typeparam>
        /// <param name="entity">Entity that will have its FK entities included.</param>
        /// <returns>True, if it's ok.</returns>
        public bool IncludeAll<TEntity>(TEntity entity);

        /// <summary>
        /// Include all FK entities.
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to be manipulated.</typeparam>
        /// <param name="entities">Entities that will have their FK entities included.</param>
        /// <returns>True, if it's ok.</returns>
        public bool IncludeAllRange<TEntity>(IEnumerable<TEntity>? entities);

        /// <summary>
        /// Includes a specific FK entity only.
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to be manipulated.</typeparam>
        /// <param name="entity">Entity that will have their FK entity included.</param>
        /// <param name="fkName">Name on the FK entity that will be included.</param>
        /// <returns>True if success.</returns>
        public bool IncludeEntityFK<TEntity>(TEntity entity, string fkName);

        /// <summary>
        /// Includes the inverse entity specified by the inverse property name for the given entity.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="entity">The entity to include the inverse entity for.</param>
        /// <param name="inversePropertyName">The name of the inverse property to include.</param>
        /// <returns>True if the inverse entity was successfully included; otherwise, false.</returns>
        public bool IncludeInverseEntity<TEntity>(TEntity entity, string inversePropertyName);

        /// <summary>
        /// Gets the table name associated with the specified entity.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <returns>The name of the table associated with the entity.</returns>
        public string? GetTableName<TEntity>();

        /// <summary>
        /// Gets the name of the table representing a many-to-many relationship between two entities.
        /// </summary>
        /// <param name="entity1">The type of the first entity.</param>
        /// <param name="propCollectionName">The name of the property present in the first entity that represents a collection linked to the second entity.</param>
        /// <returns>The name of the many-to-many table.</returns>
        public string? GetTableNameManyToMany(Type entity1, string propCollectionName);

        /// <summary>
        /// Gets the name of the primary key for the specified entity.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="entity">The instance of the entity.</param>
        /// <returns>The name of the primary key for the entity.</returns>
        public string? GetPKName<TEntity>(TEntity entity) where TEntity : class;

        /// <summary>
        /// Normalizes a given text by removing diacritical marks (accents), trimming whitespace, replacing spaces with a specified character, and optionally converting the text to lowercase.
        /// </summary>
        /// <param name="text">The input text to be normalized. If null or empty, an empty string is returned.</param>
        /// <param name="replaceSpace">The character to replace spaces with. Defaults to '_'.</param>
        /// <param name="toLower">Indicates whether the result should be converted to lowercase. Defaults to true.</param>
        /// <returns>A normalized version of the input text without diacritical marks, spaces replaced by the specified character, and optionally in lowercase.</returns>
        public string NormalizeText(string? text, char replaceSpace = '_', bool toLower = true);


        /// <summary>
        /// Validates and normalizes a given name for use as a column or table name.
        /// The name must start with a letter or an underscore, contain only valid characters, and not exceed 30 characters in length. Invalid characters can optionally be replaced 
        /// </summary>
        /// <param name="name">The name to validate and normalize.</param>
        /// <param name="replaceInvalidChars">Indicates whether invalid characters should be replaced with underscores. Defaults to <c>true</c>.</param>
        /// <returns>The normalized name, suitable for use as a column or table name.</returns>
        /// <exception cref="ArgumentException">Thrown when the name is invalid.</exception>
        string NormalizeColumnOrTableName(string? name, bool replaceInvalidChars = true);

        /// <summary>
        /// Retrieve the version of the connected database.
        /// </summary>
        /// <param name="database">
        /// An optional <see cref="Connection.Database"/> object containing the database connection. 
        /// If <paramref name="database"/> is null, the default connection <see cref="Connection.Database.IDbConnection"/> is used.
        /// </param>
        /// <returns>
        /// A string representing the database version, or "Unknown Version" if the version cannot be determined.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when no valid database connection is provided.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when an error occurs during query execution.
        /// </exception>
        /// <remarks>
        /// This method generates and executes a database-specific query to retrieve the version of the connected database.
        /// Supported databases include Oracle, SQL Server, MySQL, PostgreSQL, and SQLite.
        /// </remarks>
        /// <example>
        /// Example usage:
        /// <code>
        /// var database = new Database(new SqlConnection("Data Source=localhost;Initial Catalog=master;User ID=sa;Password=your_password"));
        /// string version = GetDatabaseVersion(database);
        /// Console.WriteLine($"Database Version: {version}");
        /// </code>
        /// </example>
        public string GetDatabaseVersion(Connection.Database? database);

    }
}
