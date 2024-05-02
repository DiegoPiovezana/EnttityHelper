using EH.Connection;
using System;
using System.Collections.Generic;
using System.Data;

namespace EH.Command
{
    interface IEnttityHelper
    {
        /// <summary>
        /// Allow to insert an entity in the database.
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to be manipulated.</typeparam>
        /// <param name="entity">Entity to be inserted into the database.</param>      
        /// <param name="namePropUnique">(Optional) Name of the property to be considered as a uniqueness criterion.</param> 
        /// <param name="createTable">(Optional) If the table that will receive the insertion does not exist, it can be created.</param> 
        /// <param name="tableName">(Optional) Name of the table to which the entity will be inserted. By default, the table informed in the "Table" attribute of the entity class will be considered.</param> 
        /// <returns>
        /// True, if one or more entities are inserted into the database.
        /// <para>If the return is negative, it indicates that the insertion did not happen due to some established criteria.</para>
        /// </returns>
        public int Insert<TEntity>(TEntity entity, string? namePropUnique = null, bool createTable = true, string? tableName = null);

        ///// <summary>
        ///// Inserts data from a DataTable into the specified database table.
        ///// </summary>
        ///// <typeparam name="TEntity">The type of entity corresponding to the table.</typeparam>
        ///// <param name="dataTable">The DataTable containing the data to be inserted.</param>
        ///// <param name="createTable">(Optional) If true and the destination table does not exist, it will be created based on the structure of the DataTable.</param>
        ///// <param name="tableName">Optional. The name of the table where the data will be inserted. If not specified, the name of the DataTable will be used.</param>
        ///// <returns>The number of rows successfully inserted into the database table.</returns>

        //public int Insert(DataTable dataTable, bool createTable = false, string? tableName = null)
        //{
        //    if (dataTable.Rows.Count == 0) return 0;
        //    tableName ??= dataTable.TableName;

        //    if (!CheckIfExist(tableName) && createTable)
        //    {
        //        CreateTable(dataTable, tableName);
        //    }

        //    return Commands.Execute.PerformBulkCopyOperation(DbContext, dataTable, tableName) ? dataTable.Rows.Count : 0;
        //}

        ///// <summary>
        ///// Inserts data from an array of DataRow into the specified database table.
        ///// </summary>
        ///// <typeparam name="TEntity">The type of entity corresponding to the table.</typeparam>
        ///// <param name="dataRow">An array of DataRow objects containing the data to be inserted.</param>
        ///// <param name="tableName">Optional. The name of the table where the data will be inserted. If not specified, the name of the entity type will be used.</param>
        ///// <returns>The number of rows successfully inserted into the database table.</returns>
        ///// <exception cref="ArgumentNullException">Thrown if the tableName parameter is null.</exception>
        //public int Insert<TEntity>(DataRow[] dataRow, string? tableName = null);     

        ///// <summary>
        ///// Inserts data from an IDataReader into the specified database table.
        ///// </summary>
        ///// <typeparam name="TEntity">The type of entity corresponding to the table.</typeparam>
        ///// <param name="dataReader">The IDataReader containing the data to be inserted.</param>
        ///// <param name="tableName">Optional. The name of the table where the data will be inserted. If not specified, the name of the entity type will be used.</param>
        ///// <returns>True if the data is inserted successfully, otherwise False.</returns>
        ///// <exception cref="ArgumentNullException">Thrown if the tableName parameter is null.</exception>
        //public bool Insert<TEntity>(IDataReader dataReader, string? tableName = null);        

        /// <summary>
        /// Executes a SELECT query on one database and inserts the result into another database (or in the same).
        /// </summary>
        /// <param name="selectQuery">The SELECT query to be executed.</param>
        /// <param name="db2">The instance of the EntityHelper representing the destination database.</param>
        /// <param name="tableName">The name of the table where the result will be inserted.</param>
        /// <returns>The number of rows inserted into the destination table.</returns>
        public int InsertLinkSelect(string selectQuery, EnttityHelper db2, string tableName);


        /// <summary>
        /// Allow to update an entity in the database.
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to be manipulated.</typeparam>
        /// <param name="entity">Entity to be updated in the database.</param>
        /// <param name="nameId">(Optional) Entity Id column name.</param>      
        /// <param name="tableName">(Optional) Name of the table to which the entity will be inserted. By default, the table informed in the "Table" attribute of the entity class will be considered.</param> 
        /// <returns>Number of entities updated in the database.</returns>
        public int Update<TEntity>(TEntity entity, string? nameId = null, string? tableName = null) where TEntity : class;


        /// <summary>
        /// Gets one or more entities from the database.
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to be manipulated.</typeparam>
        /// <param name="includeAll">(Optional) If true, all entities that are properties of the parent property will be included (this is the default behavior).</param>
        /// <param name="filter">(Optional) Entity search criteria.</param>     
        /// <param name="tableName">(Optional) Name of the table where the entities are inserted. By default, the table informed in the "Table" attribute of the entity class will be considered.</param> 
        /// <returns>Entities list.</returns>
        public List<TEntity>? Get<TEntity>(bool includeAll = true, string? filter = null, string? tableName = null);

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
        /// <param name="filter">(Optional) Possible filter.</param>
        /// <param name="quantity">(Optional) The minimum number of records to check for existence in the table. Enter 0 if you just want to check if the table exists.</param>
        /// <returns>True, if table exists and (optionally) it is filled</returns>
        public bool CheckIfExist(string tableName, string? filter = null, int quantity = 0);

        /// <summary>
        /// Allows you to create a table in the database according to the provided objectEntity object.
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to create the table.</typeparam>
        /// <param name="tableName">(Optional) Name of the table to which the entity will be inserted. By default, the table informed in the "Table" attribute of the entity class will be considered.</param> 
        /// <param name="createOnlyPrimaryTable">(Optional) If true, tables used for M:N relationships, for example, will not be created. By default they are created too.</param>
        /// <returns>True, if table was created and false, if not created.</returns>
        /// <exception cref="InvalidOperationException">Occurs if the table should have been created but was not.</exception>      
        public bool CreateTable<TEntity>(string? tableName = null, bool createOnlyPrimaryTable = false);

        /// <summary>
        /// Creates a table in the database based on the structure specified in a DataTable object.
        /// </summary>
        /// <param name="dataTable">The DataTable object containing the structure of the table to be created.</param>
        /// <param name="tableName">(Optional) The name of the table to be created. If not specified, the name from the DataTable object will be used.</param>
        /// <returns>True if the table is created successfully, otherwise False.</returns>
        /// <exception cref="InvalidOperationException">An exception is thrown if an error occurs while attempting to create the table.</exception>
        public bool CreateTable(DataTable dataTable, string? tableName = null);

        /// <summary>
        /// Allows you to create a table in the database according to the provided Entity object, if table does not exist.
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to create the table.</typeparam>  
        /// <param name="tableName">(Optional) Name of the table to which the entity will be inserted. By default, the table informed in the "Table" attribute of the entity class will be considered.</param> 
        /// <returns>True, if table was created or already exists and false, if it was not created.</returns>
        /// <exception cref="InvalidOperationException">Occurs if the table should have been created but was not.</exception>      
        public bool CreateTableIfNotExist<TEntity>(string? tableName = null);

        public bool CreateTableIfNotExist(DataTable dataTable, string? tableName = null);

        /// <summary>
        /// Allow to delete an entity in the database.
        /// </summary>
        /// <param name="entity">Entity that will have its FK entities included.</param>
        /// <param name="nameId">(Optional) Entity Id column name. By default, PK will be used.</param>
        /// <param name="tableName">(Optional) Name of the table to which the entity will be inserted. By default, the table informed in the "Table" attribute of the entity class will be considered.</param> 
        /// <returns>Number of exclusions made.</returns>
        public int Delete<TEntity>(TEntity entity, string? nameId = null, string? tableName = null) where TEntity : class;

        /// <summary>
        /// Executes the non query (Create, Alter, Drop, Insert, Update or Delete) on the database.
        /// </summary>   
        /// <param name="query">Query to be executed.</param>
        /// <param name="expectedChanges">(Optional) Expected amount of changes to the database. If the amount of changes is not expected, the change will be rolled back and an exception will be thrown.</param> 
        /// <returns>Number of affected rows.</returns>
        public int ExecuteNonQuery(string? query, int expectedChanges = -1);

        /// <summary>
        /// Executes a SELECT query in the database and returns a list of entities obtained from the query result.
        /// </summary>
        /// <typeparam name="TEntity">The type of entities to be obtained.</typeparam>
        /// <param name="query">The SELECT query to be executed.</param>
        /// <returns>A list of entities retrieved from the database, or null if the query execution fails.</returns>
        public List<TEntity>? ExecuteSelect<TEntity>(string? query);

        /// <summary>
        /// Executes a SELECT query in the database and returns the result as a DataTable.
        /// </summary>
        /// <typeparam name="TEntity">The type of entities to be obtained.</typeparam>
        /// <param name="query">The SELECT query to be executed.</param>
        /// <returns>
        /// A DataTable containing the result of the query, or null if the query execution fails.
        /// </returns>
        public DataTable? ExecuteSelectDt<TEntity>(string? query);

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
        public bool IncludeAll<TEntity>(List<TEntity>? entities);

        /// <summary>
        /// Includes a specific FK entity only.
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to be manipulated.</typeparam>
        /// <param name="entity">Entity that will have their FK entity included.</param>
        /// <param name="fkName">Name on the FK entity that will be included.</param>
        /// <returns>True if success.</returns>
        public bool IncludeEntityFK<TEntity>(TEntity entity, string fkName);




    }
}
