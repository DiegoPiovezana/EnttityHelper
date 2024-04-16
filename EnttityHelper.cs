using EH.Connection;
using EH.Properties;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;

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

            if (dbContext.Type.Equals(Enums.DatabaseType.Oracle))
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
            else if (dbContext.Type.Equals(Enums.DatabaseType.SqlServer))
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
            else if (dbContext.Type.Equals(Enums.DatabaseType.Sqlite))
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
        /// <param name="namePropUnique">Name of the property to be considered as a uniqueness criterion (optional).</param>        
        /// <returns>
        /// True, if one or more entities are inserted into the database.
        /// <para>If the return is negative, it indicates that the insertion did not happen due to some established criteria.</para>
        /// </returns>
        public int Insert<TEntity>(TEntity entity, string? namePropUnique = null)
        {            
            if (!string.IsNullOrEmpty(namePropUnique))
            {
                var properties = ToolsEH.GetProperties(entity);

                if (CheckIfExist(ToolsEH.GetNameTable<TEntity>(ReplacesTableName), $"{namePropUnique} = '{properties[namePropUnique]}'", 1))
                {
                    Console.WriteLine($"EH-101: Entity '{namePropUnique} {properties[namePropUnique]}' already exists in table!");
                    return -101;
                }
            }

            string? insertQuery = CommandsSqlString.Insert(entity, ReplacesTableName);
            return insertQuery is null ? throw new Exception($"EH-000: Error!") : ExecuteNonQuery(insertQuery);
        }

        /// <summary>
        /// Allow to update an entity in the database.
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to be manipulated.</typeparam>
        /// <param name="entity">Entity to be updated in the database.</param>
        /// <param name="nameId">Entity Id column name.</param>      
        /// <returns>Number of entities updated in the database.</returns>
        public int Update<TEntity>(TEntity entity, string? nameId = null) where TEntity : class
        {
            string? updateQuery = CommandsSqlString.Update(entity, nameId, ReplacesTableName);
            return ExecuteNonQuery(updateQuery);
        }

        /// <summary>
        /// Gets one or more entities from the database.
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to be manipulated.</typeparam>
        /// <param name="includeAll">If true, all entities that are properties of the parent property will be included.</param>
        /// <param name="filter">Entity search criteria (optional).</param>       
        /// <returns>Entities list.</returns>
        public List<TEntity>? Get<TEntity>(bool includeAll = true, string? filter = null)
        {
            string? querySelect = CommandsSqlString.Get<TEntity>(filter, ReplacesTableName);
            var entities = ExecuteSelect<TEntity>(querySelect);
            if (includeAll) { _ = IncludeAll(entities); }
            return entities;
        }

        //public IQueryable Get<TEntity>(bool includeAll = true)
        //{
        //    string? querySelect = CommandsSqlString.Get<TEntity>(null, ReplacesTableName);
        //    var entities = ExecuteSelect<TEntity>(querySelect);
            
        //    if (includeAll) { IncludeAll(entities); }
        //    return entities;
        //}

        /// <summary>
        /// Search the specific entity by <paramref name="idPropName"/>
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to be manipulated.</typeparam>
        /// <param name="entity">Entity to be searched for in the bank.</param>
        /// <param name="includeAll">(Optional) Defines whether it will include all other FK entities (by default it will include all entities).</param>
        /// <param name="idPropName">(Optional) Entity identifier name.</param> 
        /// <returns>Specific entity from database.</returns>
        public TEntity? Search<TEntity>(TEntity entity, bool includeAll = true, string? idPropName = null) where TEntity : class
        {
            string? selectQuery = CommandsSqlString.Search(entity, idPropName, ReplacesTableName);
            var entities = ExecuteSelect<TEntity>(selectQuery);
            if (includeAll) { _ = IncludeAll(entities.FirstOrDefault()); }
            return entities.FirstOrDefault();
        }

        /// <summary>
        /// Checks if table exists (>= 0) and it is filled (> 0).
        /// </summary>
        /// <param name="nameTable">Name of the table to check if it exists.</param>
        /// <param name="filter">Possible filter.</param>
        /// <param name="quantity">The minimum number of records to check for existence in the table. Enter 0 if you just want to check if the table exists.</param>
        /// <returns></returns>
        public bool CheckIfExist(string nameTable, string? filter = null, int quantity = 0)
        {
            try
            {
                if (DbContext?.IDbConnection is null) throw new InvalidOperationException("Connection does not exist!");

                using IDbConnection dbConnection = DbContext.CreateOpenConnection();  
                using IDbCommand command = DbContext.CreateCommand($"SELECT COUNT(*) FROM {nameTable} WHERE {filter ?? "1 = 1"}");
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
                if (ex.Number == 208) return false; // Invalid object name 'NameTable'.
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
        /// <returns>True, if table was created and false, if not created.</returns>
        /// <exception cref="InvalidOperationException">Occurs if the table should have been created but was not.</exception>      
        public bool CreateTable<TEntity>()
        {
            if (DbContext?.IDbConnection is null) throw new InvalidOperationException("Connection does not exist!");

            string? createTableQuery = CommandsSqlString.CreateTable<TEntity>(TypesDefault, ReplacesTableName);

            if (ExecuteNonQuery(createTableQuery) != 0) // Return = -1
            {
                Console.WriteLine("Table created!");
                return true;
            }
            else
            {
                Console.WriteLine("Table not created!");
                return false;
            }
        }

        /// <summary>
        /// Allows you to create a table in the database according to the provided objectEntity object, if table does not exist.
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to create the table.</typeparam>    
        /// <returns>True, if table was created or already exists and false, if it was not created.</returns>
        /// <exception cref="InvalidOperationException">Occurs if the table should have been created but was not.</exception>      
        public bool CreateTableIfNotExist<TEntity>()
        {
            if (DbContext?.IDbConnection is null) throw new InvalidOperationException("Connection does not exist!");
            string table = ToolsEH.GetNameTable<TEntity>(ReplacesTableName);
            if (CheckIfExist(table)) { Console.WriteLine($"Table '{table}' already exists!"); return true; }
            return CreateTable<TEntity>();
        }

        /// <summary>
        /// Allow to delete an entity in the database.
        /// </summary>
        /// <param name="entity">Entity that will have its FK entities included.</param>
        /// <param name="nameId">Entity Id column name.</param>
        /// <returns>Number of exclusions made.</returns>
        public int Delete<TEntity>(TEntity entity, string? nameId = null) where TEntity : class
        {
            string? deleteQuery = CommandsSqlString.Delete(entity, nameId, ReplacesTableName);
            return ExecuteNonQuery(deleteQuery);
        }

        /// <summary>
        /// Executes the non query (Create, Alter, Drop, Insert, Update or Delete) on the database.
        /// </summary>   
        /// <param name="query">Query to be executed.</param>
        /// <returns>Number of affected rows.</returns>
        public int ExecuteNonQuery(string? query)
        {
            return (int?)Commands.Execute.ExecuteCommand<object>(DbContext, query, true) ?? 0;
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

        //public IDataReader? ExecuteSelect<TEntity>(string? query)
        //{
        //    return (IDataReader)ExecuteCommand<TEntity>(query);
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
            foreach (TEntity entity in entities) { Entities.Inclusions.IncludeForeignKeyEntities(entity); }            
            return true;
        }

        /// <summary>
        /// Include FK entity.
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to be manipulated.</typeparam>
        /// <param name="entity">Entity that will have their FK entity included.</param>
        /// <param name="fkName">Name on the FK entity that will be included.</param>
        /// <returns>True if success.</returns>
        public bool IncludeEntityFK<TEntity>(TEntity entity, string fkName)
        {
            if (entity == null) return false;
            Entities.Inclusions.IncludeForeignKeyEntities(entity, fkName);
            return true;
        }





    }
}
