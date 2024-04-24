using EH.Command;
using EH.Connection;
using EH.Properties;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;

namespace EH
{
    /// <summary>
    /// Allows easy manipulation of entities in different databases.
    /// </summary>
    public class EnttityHelper
    {
        /// <summary>
        /// Database where the entities will be manipulated.
        /// </summary>
        public Database DbContext { get; set; }

        /// <summary>
        /// Common reserved type for database data. Example: "Boolean" => "NUMBER(1)".
        /// <para>Note: the size of a string (informed in parentheses), for example, can be changed via the property attribute.</para>
        /// </summary>
        public Dictionary<string, string>? TypesDefault { get; set; }

        /// <summary>
        /// (Optional) Terms that can be replaced in table names.
        /// </summary>
        public Dictionary<string, string>? ReplacesTableName { get; set; }

        /// <summary>
        /// Allows you to obtain the main commands to be executed on the database.
        /// </summary>
        public SqlQueryString GetQuery = new();


        /// <summary>
        /// Allows you to manipulate entities from a connection string.
        /// </summary>
        /// <param name="connectionString"></param>
        public EnttityHelper(string connectionString)
        {
            DbContext = new Database(connectionString);
            DefineTypesDefaultDb(DbContext);
        }

        /// <summary>
        /// Allows you to manipulate entities from a previously created database.
        /// </summary>
        /// <param name="db"></param>
        public EnttityHelper(Database db)
        {
            DbContext = db;
            DefineTypesDefaultDb(DbContext);
        }


        private void DefineTypesDefaultDb(Database? dbContext)
        {
            if (dbContext is null) throw new InvalidOperationException("DbContext cannot be null.");
            if (dbContext.Type is null) throw new InvalidOperationException("DbContext Type cannot be null.");

            if (dbContext.Type.Equals(Enums.DbType.Oracle))
            {
                TypesDefault = new Dictionary<string, string> {
                { "String", "NVARCHAR2(1000)" },
                { "Boolean", "NUMBER(1)" },
                { "DateTime", "TIMESTAMP" },
                { "Decimal", "NUMBER" },
                { "Double", "NUMBER" },
                { "Int16", "NUMBER" },
                { "Int32", "NUMBER" },
                { "Int64", "NUMBER" },
                { "Single", "NUMBER" },
                { "TimeSpan", "DATE" }
                };
            }
            else if (dbContext.Type.Equals(Enums.DbType.SQLServer))
            {
                TypesDefault = new Dictionary<string, string>
                {
                { "String", "NVARCHAR(1000)" },
                { "Boolean", "BIT" },
                { "DateTime", "DATETIME" },
                { "Decimal", "DECIMAL" },
                { "Double", "FLOAT" },
                { "Int16", "SMALLINT" },
                { "Int32", "INT" },
                { "Int64", "BIGINT" },
                { "Single", "REAL" },
                { "TimeSpan", "TIME" }
                };
            }
            else if (dbContext.Type.Equals(Enums.DbType.SQLite))
            {
                TypesDefault = new Dictionary<string, string>
                {
                { "String", "TEXT" },
                { "Boolean", "INTEGER" },
                { "DateTime", "TEXT" },
                { "Decimal", "REAL" },
                { "Double", "REAL" },
                { "Int16", "INTEGER" },
                { "Int32", "INTEGER" },
                { "Int64", "INTEGER" },
                { "Single", "REAL" },
                { "TimeSpan", "TEXT" }
                };
            }
            else
            {
                throw new InvalidOperationException("Database type not supported.");
            }
        }


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
        public int Insert<TEntity>(TEntity entity, string? namePropUnique = null, bool createTable = false, string? tableName = null)
        {
            if (!string.IsNullOrEmpty(namePropUnique))
            {
                var properties = ToolsProp.GetProperties(entity);
                tableName ??= ToolsProp.GetTableName<TEntity>(ReplacesTableName);

                if (CheckIfExist(tableName, $"{namePropUnique} = '{properties[namePropUnique]}'", 1))
                {
                    Debug.WriteLine($"EH-101: Entity '{namePropUnique} {properties[namePropUnique]}' already exists in table!");
                    return -101;
                }

                if (!CheckIfExist(tableName) && createTable)
                {
                    CreateTable<TEntity>(tableName);
                }
            }

            string? insertQuery = GetQuery.Insert(entity, ReplacesTableName, tableName);
            return insertQuery is null ? throw new Exception($"EH-000: Error!") : ExecuteNonQuery(insertQuery, 1);
        }

        /// <summary>
        /// Inserts data from a DataTable into the specified database table.
        /// </summary>
        /// <typeparam name="TEntity">The type of entity corresponding to the table.</typeparam>
        /// <param name="dataTable">The DataTable containing the data to be inserted.</param>
        /// <param name="createTable">(Optional) If true and the destination table does not exist, it will be created based on the structure of the DataTable.</param>
        /// <param name="tableName">Optional. The name of the table where the data will be inserted. If not specified, the name of the DataTable will be used.</param>
        /// <returns>The number of rows successfully inserted into the database table.</returns>

        public int Insert<TEntity>(DataTable dataTable, bool createTable = false, string? tableName = null)
        {
            if (dataTable.Rows.Count == 0) return 0;
            tableName ??= dataTable.TableName;

            if (!CheckIfExist(tableName) && createTable)
            {
                CreateTable(dataTable, tableName);
            }

            return Commands.Execute.PerformBulkCopyOperation(DbContext, dataTable, tableName) ? dataTable.Rows.Count : 0;
        }

        /// <summary>
        /// Inserts data from an array of DataRow into the specified database table.
        /// </summary>
        /// <typeparam name="TEntity">The type of entity corresponding to the table.</typeparam>
        /// <param name="dataRow">An array of DataRow objects containing the data to be inserted.</param>
        /// <param name="tableName">Optional. The name of the table where the data will be inserted. If not specified, the name of the entity type will be used.</param>
        /// <returns>The number of rows successfully inserted into the database table.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the tableName parameter is null.</exception>

        public int Insert<TEntity>(DataRow[] dataRow, string? tableName = null)
        {
            if (dataRow.Length == 0) return 0;
            if (tableName is null) throw new ArgumentNullException(nameof(tableName), "Table name cannot be null.");
            return Commands.Execute.PerformBulkCopyOperation(DbContext, dataRow, tableName) ? dataRow.Length : 0;
        }

        /// <summary>
        /// Inserts data from an IDataReader into the specified database table.
        /// </summary>
        /// <typeparam name="TEntity">The type of entity corresponding to the table.</typeparam>
        /// <param name="dataReader">The IDataReader containing the data to be inserted.</param>
        /// <param name="tableName">Optional. The name of the table where the data will be inserted. If not specified, the name of the entity type will be used.</param>
        /// <returns>True if the data is inserted successfully, otherwise False.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the tableName parameter is null.</exception>

        public bool Insert<TEntity>(IDataReader dataReader, string? tableName = null)
        {
            if (tableName is null) throw new ArgumentNullException(nameof(tableName), "Table name cannot be null.");
            return Commands.Execute.PerformBulkCopyOperation(DbContext, dataReader, tableName);
        }

        /// <summary>
        /// Allow to update an entity in the database.
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to be manipulated.</typeparam>
        /// <param name="entity">Entity to be updated in the database.</param>
        /// <param name="nameId">(Optional) Entity Id column name.</param>      
        /// <param name="tableName">(Optional) Name of the table to which the entity will be inserted. By default, the table informed in the "Table" attribute of the entity class will be considered.</param> 
        /// <returns>Number of entities updated in the database.</returns>
        public int Update<TEntity>(TEntity entity, string? nameId = null, string? tableName = null) where TEntity : class
        {
            string? updateQuery = GetQuery.Update(entity, nameId, ReplacesTableName, tableName);
            return ExecuteNonQuery(updateQuery, 1);
        }

        /// <summary>
        /// Gets one or more entities from the database.
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to be manipulated.</typeparam>
        /// <param name="includeAll">(Optional) If true, all entities that are properties of the parent property will be included (this is the default behavior).</param>
        /// <param name="filter">(Optional) Entity search criteria.</param>     
        /// <param name="tableName">(Optional) Name of the table where the entities are inserted. By default, the table informed in the "Table" attribute of the entity class will be considered.</param> 
        /// <returns>Entities list.</returns>
        public List<TEntity>? Get<TEntity>(bool includeAll = true, string? filter = null, string? tableName = null)
        {
            string? querySelect = GetQuery.Get<TEntity>(filter, ReplacesTableName, tableName);
            var entities = ExecuteSelect<TEntity>(querySelect);
            if (includeAll) { _ = IncludeAll(entities); }
            return entities;
        }

        /// <summary>
        /// Search the specific entity by <paramref name="idPropName"/>
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to be manipulated.</typeparam>
        /// <param name="entity">Entity to be searched for in the bank.</param>
        /// <param name="includeAll">(Optional) Defines whether it will include all other FK entities (by default it will include all entities).</param>
        /// <param name="idPropName">(Optional) Entity identifier name.</param>
        /// <param name="tableName">(Optional) Name of the table to which the entity will be inserted. By default, the table informed in the "Table" attribute of the entity class will be considered.</param> 
        /// <returns>Specific entity from database.</returns>
        public TEntity? Search<TEntity>(TEntity entity, bool includeAll = true, string? idPropName = null, string? tableName = null) where TEntity : class
        {
            string? selectQuery = GetQuery.Search(entity, idPropName, ReplacesTableName, tableName);
            var entities = ExecuteSelect<TEntity>(selectQuery);
            if (includeAll) { _ = IncludeAll(entities.FirstOrDefault()); }
            return entities.FirstOrDefault();
        }

        /// <summary>
        /// Checks if table exists (>= 0) and it is filled (> 0).
        /// </summary>
        /// <param name="tableName">Name of the table to check if it exists.</param>
        /// <param name="filter">(Optional) Possible filter.</param>
        /// <param name="quantity">(Optional) The minimum number of records to check for existence in the table. Enter 0 if you just want to check if the table exists.</param>
        /// <returns>True, if table exists and (optionally) it is filled</returns>
        public bool CheckIfExist(string tableName, string? filter = null, int quantity = 0)
        {
            try
            {
                if (DbContext?.IDbConnection is null) throw new InvalidOperationException("Connection does not exist!");

                using IDbConnection dbConnection = DbContext.CreateOpenConnection();
                using IDbCommand command = DbContext.CreateCommand($"SELECT COUNT(*) FROM {tableName} WHERE {filter ?? "1 = 1"}");
                object result = command.ExecuteScalar(); // >= 0

                if (result != null && result != DBNull.Value)
                {
                    return Convert.ToInt32(result) >= quantity;
                }

                return false;
            }
            catch (OracleException ex)
            {
                if (ex.Number == 942) return false; // ORA-00942: table or view does not exist
                else throw;
            }
            catch (SqlException ex)
            {
                if (ex.Number == 208) return false; // Invalid object name 'tableName'.
                else throw;
            }
            //catch (SQLiteException ex) when (ex.ErrorCode == SQLiteErrorCode.Table)
            //{
            //    return false;
            //}
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (DbContext?.IDbConnection is not null && DbContext.IDbConnection.State == ConnectionState.Open)
                    DbContext.IDbConnection.Close();
            }
        }

        /// <summary>
        /// Allows you to create a table in the database according to the provided objectEntity object.
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to create the table.</typeparam>
        /// <param name="tableName">(Optional) Name of the table to which the entity will be inserted. By default, the table informed in the "Table" attribute of the entity class will be considered.</param> 
        /// <returns>True, if table was created and false, if not created.</returns>
        /// <exception cref="InvalidOperationException">Occurs if the table should have been created but was not.</exception>      
        public bool CreateTable<TEntity>(string? tableName = null)
        {
            if (DbContext?.IDbConnection is null) throw new InvalidOperationException("Connection does not exist!");

            var createsTableQuery = GetQuery.CreateTable<TEntity>(TypesDefault, ReplacesTableName, tableName);

            foreach (string? createTableQuery in createsTableQuery)
            {
                if (ExecuteNonQuery(createTableQuery) != 0) // Return = -1
                {
                    Debug.WriteLine("Table created!");
                }
                else
                {
                    throw new InvalidOperationException("Table not created!");
                }
            }
            return true;            
        }

        /// <summary>
        /// Creates a table in the database based on the structure specified in a DataTable object.
        /// </summary>
        /// <param name="dataTable">The DataTable object containing the structure of the table to be created.</param>
        /// <param name="tableName">(Optional) The name of the table to be created. If not specified, the name from the DataTable object will be used.</param>
        /// <returns>True if the table is created successfully, otherwise False.</returns>
        /// <exception cref="InvalidOperationException">An exception is thrown if an error occurs while attempting to create the table.</exception>
        public bool CreateTable(DataTable dataTable, string? tableName = null)
        {
            if (DbContext?.IDbConnection is null) throw new InvalidOperationException("Connection does not exist!");

            string? createTableQuery = GetQuery.CreateTableFromDataTable(dataTable, TypesDefault, ReplacesTableName, tableName);

            if (ExecuteNonQuery(createTableQuery) != 0) // Return = -1
            {
                Debug.WriteLine("Table created!");
                return true;
            }
            else
            {
                throw new InvalidOperationException("Table not created!");
            }
        }

        /// <summary>
        /// Allows you to create a table in the database according to the provided Entity object, if table does not exist.
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to create the table.</typeparam>  
        /// <param name="tableName">(Optional) Name of the table to which the entity will be inserted. By default, the table informed in the "Table" attribute of the entity class will be considered.</param> 
        /// <returns>True, if table was created or already exists and false, if it was not created.</returns>
        /// <exception cref="InvalidOperationException">Occurs if the table should have been created but was not.</exception>      
        public bool CreateTableIfNotExist<TEntity>(string? tableName = null)
        {
            if (DbContext?.IDbConnection is null) throw new InvalidOperationException("Connection does not exist!");
            tableName ??= ToolsProp.GetTableName<TEntity>(ReplacesTableName);
            if (CheckIfExist(tableName)) { Debug.WriteLine($"Table '{tableName}' already exists!"); return true; }
            return CreateTable<TEntity>();
        }

        public bool CreateTableIfNotExist(DataTable dataTable, string? tableName = null)
        {
            if (DbContext?.IDbConnection is null) throw new InvalidOperationException("Connection does not exist!");
            tableName ??= Define.NameTableFromDataTable(dataTable, ReplacesTableName);
            if (CheckIfExist(tableName)) { Debug.WriteLine($"Table '{tableName}' already exists!"); return true; }
            return CreateTable(dataTable, tableName);
        }

        /// <summary>
        /// Allow to delete an entity in the database.
        /// </summary>
        /// <param name="entity">Entity that will have its FK entities included.</param>
        /// <param name="nameId">(Optional) Entity Id column name. By default, PK will be used.</param>
        /// <param name="tableName">(Optional) Name of the table to which the entity will be inserted. By default, the table informed in the "Table" attribute of the entity class will be considered.</param> 
        /// <returns>Number of exclusions made.</returns>
        public int Delete<TEntity>(TEntity entity, string? nameId = null, string? tableName = null) where TEntity : class
        {
            string? deleteQuery = GetQuery.Delete(entity, nameId, ReplacesTableName, tableName);
            return ExecuteNonQuery(deleteQuery, 1);
        }

        /// <summary>
        /// Executes the non query (Create, Alter, Drop, Insert, Update or Delete) on the database.
        /// </summary>   
        /// <param name="query">Query to be executed.</param>
        /// <param name="expectedChanges">(Optional) Expected amount of changes to the database. If the amount of changes is not expected, the change will be rolled back and an exception will be thrown.</param> 
        /// <returns>Number of affected rows.</returns>
        public int ExecuteNonQuery(string? query, int expectedChanges = -1)
        {
            return (int?)Commands.Execute.ExecuteCommand<object>(DbContext, query, true, false, expectedChanges) ?? 0;
        }

        /// <summary>
        /// Executes the section in the database and returns the list with the obtained entities.
        /// </summary>
        /// <typeparam name="TEntity">Type of entities to be obtained.</typeparam>
        /// <param name="query">Query to be executed.</param>
        /// <returns>List of entities retrieved from the database.</returns>
        public List<TEntity>? ExecuteSelect<TEntity>(string? query)
        {
            return (List<TEntity>?)Commands.Execute.ExecuteCommand<TEntity>(DbContext, query);
        }

        //public IDataReader? GetDataReader<TEntity>(string? query)
        //{
        //    return (IDataReader)Commands.Execute.ExecuteCommand<TEntity>(DbContext, query);
        //}

        /// <summary>
        /// Include all FK entities.
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to be manipulated.</typeparam>
        /// <param name="entity">Entity that will have its FK entities included.</param>
        /// <returns>True, if it's ok.</returns>
        public bool IncludeAll<TEntity>(TEntity entity)
        {
            return IncludeAll(new List<TEntity> { entity });
        }

        /// <summary>
        /// Include all FK entities.
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to be manipulated.</typeparam>
        /// <param name="entities">Entities that will have their FK entities included.</param>
        /// <returns>True, if it's ok.</returns>
        public bool IncludeAll<TEntity>(List<TEntity>? entities)
        {
            if (entities == null || entities.Count == 0) return false;           
            foreach (TEntity entity in entities) { new Entities.Inclusions(this).IncludeForeignKeyEntities(entity); }
            return true;
        }

        /// <summary>
        /// Includes a specific FK entity only.
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to be manipulated.</typeparam>
        /// <param name="entity">Entity that will have their FK entity included.</param>
        /// <param name="fkName">Name on the FK entity that will be included.</param>
        /// <returns>True if success.</returns>
        public bool IncludeEntityFK<TEntity>(TEntity entity, string fkName)
        {
            if (entity == null) return false;
            new Entities.Inclusions(this).IncludeForeignKeyEntities(entity, fkName);
            return true;
        }





    }
}
