using EH.Connection;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Xml;

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

            if (dbContext.Type.ToLower().Equals("oracle"))
            {
                TypesDefault = new Dictionary<string, string> {
                { "String", "NVARCHAR2(1000)" },
                { "Boolean", "NUMBER(1)" },
                { "DateTime", "TIMESTAMP" },
                { "Decimal", "NUMBER(10)" },
                { "Double", "NUMBER(10)" },
                { "Int16", "NUMBER(10)" },
                { "Int32", "NUMBER(10)" },
                { "Int64", "NUMBER(10)" },
                { "Single", "NUMBER(10)" },
                { "TimeSpan", "DATE" }
                };
            }
            else if (dbContext.Type.ToLower().Equals("SqlServer"))
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
            else if (dbContext.Type.ToLower().Equals("Sqlite"))
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
            string? insertQuery = CommandsString.Insert(this, entity, namePropUnique);

            if (insertQuery is not null && insertQuery.Equals("EH-101")) { return -101; }


            //int rowsAffected = ExecuteNonQuery(insertQuery);
            //Console.WriteLine($"Rows Affected: {rowsAffected}");

            //if (rowsAffected > 0)
            //{
            //    Console.WriteLine("Insertion successful!");
            //    return true;
            //}
            //else
            //{
            //    Console.WriteLine("No records entered!");
            //    return false;
            //}

            return ExecuteNonQuery(insertQuery);
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
            string? updateQuery = CommandsString.Update(entity, nameId);
            return ExecuteNonQuery(updateQuery);
        }

        /// <summary>
        /// Search the specific entity by <paramref name="idPropName"/>
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to be manipulated.</typeparam>
        /// <param name="entity">Entity to be searched for in the bank.</param>
        /// <param name="idPropName">Entity identifier name.</param>
        /// <param name="includeAll">Defines whether it will include all other FK entities.</param>
        /// <returns></returns>
        public TEntity? Search<TEntity>(TEntity entity, string? idPropName = null, bool includeAll = true) where TEntity : class
        {
            string? selectQuery = CommandsString.Search(entity, idPropName, includeAll);
            //if (string.IsNullOrEmpty(selectQuery))
            ////if (selectQuery is null)
            //{
            //    Console.WriteLine("Search command not exists!");
            //    return default;
            //}

            var entities = ExecuteSelect<TEntity>(selectQuery);
            if (includeAll) { _ = IncludeAll(entities.FirstOrDefault()); }
            return entities.FirstOrDefault();
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
            string? querySelect = CommandsString.Get<TEntity>(filter);
            //if (string.IsNullOrEmpty(query))
            //{
            //    //Console.WriteLine("Get command not exists!");
            //    //return new List<TEntity>();

            //    throw new ArgumentNullException(nameof(query), "Query Get cannot be null or empty.");
            //}

            var entities = ExecuteSelect<TEntity>(querySelect);
            if (includeAll) { _ = IncludeAll(entities); }
            return entities;
        }

        /// <summary>
        /// Checks if table exists (>= 0) and it is filled (> 0).
        /// </summary>
        /// <param name="nameTable">Name of the table to check if it exists.</param>
        /// <param name="filter">Possible filter.</param>
        /// <param name="quantity">Minimum quantity to be checked if it exists. Enter 0 if you just want to check if the table exists.</param>
        /// <returns></returns>
        public bool CheckIfExist(string nameTable, string? filter = null, int quantity = 0)
        {
            try
            {
                if (DbContext?.IDbConnection is null) throw new InvalidOperationException("Connection does not exist!");

                DbContext.IDbConnection.Open();

                using IDbCommand command = DbContext.CreateCommand($"SELECT COUNT(*) FROM {nameTable} WHERE {filter ?? "1 = 1"}");
                object result = command.ExecuteScalar(); // >= 0

                if (result != null && result != DBNull.Value)
                {
                    //DbContext.IDbConnection.Close();
                    return Convert.ToInt32(result) >= quantity;
                }

                //DbContext.IDbConnection.Close();
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

            //string createTableQuery = @"CREATE TABLE Exemplo (Id INT PRIMARY KEY, Nome NVARCHAR(50), DataCadastro DATE)";
            string? createTableQuery = CommandsString.CreateTable<TEntity>(TypesDefault);

            //IDbConnection connection = DbContext.IDbConnection;
            //connection.Open();

            //using (IDbCommand command = DbContext.CreateCommand(createTableQuery))
            //{
            //    command.ExecuteNonQuery();
            //    Console.WriteLine("Table created!");
            //}
            if (ExecuteNonQuery(createTableQuery) != 0)
            {
                Console.WriteLine("Table created!");
                return true;
            }
            else
            {
                //throw new InvalidOperationException("Table not created!");
                return false;
            }
            //connection.Close();
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
            if (CheckIfExist(ToolsEH.GetTable<TEntity>())) { return true; }
            return CreateTable<TEntity>();
        }

        /// <summary>
        /// Allow to delete an entity in the database.
        /// </summary>
        /// <returns>Number of exclusions made.</returns>
        public int Delete<TEntity>(TEntity entity, string? nameId = null) where TEntity : class
        {
            string? deleteQuery = CommandsString.Delete(entity, nameId);
            return ExecuteNonQuery(deleteQuery);
        }


        ///// <summary>
        ///// Execute the query.
        ///// </summary>
        ///// <param name="query">Custom query to be executed.</param>
        ///// <returns>Object.</returns>
        //public object? CustomCommand(string query)
        //{
        //    //if (string.IsNullOrEmpty(query)) { Console.WriteLine("Query not exists!"); return null; }
        //    //if (DbContext?.IDbConnection is null) { Console.WriteLine("Connection not exists!"); return null; }

        //    //IDbConnection connection = DbContext.IDbConnection;
        //    //connection.Open();
        //    //using IDbCommand command = DbContext.CreateCommand(query);
        //    //using var result = command.ExecuteReader();
        //    //connection.Close();
        //    //return result;

        //    if (query is null) throw new ArgumentNullException(nameof(query), "Query cannot be null or empty.");
        //    return ExecuteCommand<object>(query, !query.Contains("SELECT"));
        //}

        /// <summary>
        /// Executes the non query (Create, Alter, Drop, Insert, Update or Delete) on the database.
        /// </summary>   
        /// <param name="query">Query to be executed.</param>
        /// <returns>Number of affected rows.</returns>
        public int ExecuteNonQuery(string? query)
        {
            //if (string.IsNullOrEmpty(query)) { Console.WriteLine("Query not exists!"); return 0; }

            //IDbConnection connection = DbContext.IDbConnection;
            //connection.Open();
            //using IDbCommand command = DbContext.CreateCommand(query);
            //int rowsAffected = command.ExecuteNonQuery();
            //connection.Close();
            //Console.WriteLine($"Rows Affected: {rowsAffected}");
            //return rowsAffected;          

            return (int?)ExecuteCommand<object>(query, true) ?? 0;
        }

        /// <summary>
        /// Executes the section in the database and returns the list with the obtained entities.
        /// </summary>
        /// <typeparam name="TEntity">Type of entities to be obtained.</typeparam>
        /// <param name="query">Query to be executed.</param>
        /// <returns>List of entities retrieved from the database.</returns>
        public List<TEntity>? ExecuteSelect<TEntity>(string? query)
        {
            //if (string.IsNullOrEmpty(query)) { Console.WriteLine("Query not exists!"); return null; }
            //if (DbContext?.IDbConnection is null) { Console.WriteLine("Connection not exists!"); return null; }

            //IDbConnection connection = DbContext.IDbConnection;
            //connection.Open();
            //using IDbCommand command = DbContext.CreateCommand(query);
            //using var reader = command.ExecuteReader();
            //List<TEntity> entities = ToolsEH.MapDataReaderToList<TEntity>(reader);
            //connection.Close();
            //return entities;

            return (List<TEntity>?)ExecuteCommand<TEntity>(query);
        }

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
            foreach (TEntity entity in entities) { IncludeForeignKeyEntities(entity); }
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
            IncludeForeignKeyEntities(entity, fkName);
            return true;
        }


        ///////////////////////////////// PRIVATE METHODS ///////////////////////////////// 


        /// <summary>
        /// Executes a SQL command, either non-query or select, based on the provided query.
        /// </summary>
        /// <typeparam name="TEntity">The type of entities to retrieve.</typeparam>
        /// <param name="query">The SQL query to execute.</param>
        /// <param name="isNonQuery">Flag indicating whether the command is a non-query (true) or select (false).</param>        
        /// <returns>
        /// - If the command is a non-query, returns the number of affected rows.
        /// - If the command is a select, returns a list of entities retrieved from the database.
        /// </returns>
        private object? ExecuteCommand<TEntity>(string? query, bool isNonQuery = false)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query))
                {
                    //Console.WriteLine("Query does not exist!");
                    throw new ArgumentNullException(nameof(query), "Query cannot be null or empty.");
                    //return isNonQuery ? 0 : null;
                }

                if (DbContext?.IDbConnection is null)
                {
                    //Console.WriteLine("Connection does not exist!");
                    throw new InvalidOperationException("Connection does not exist.");
                    //return isNonQuery ? 0 : null;
                }

                IDbConnection connection = DbContext.IDbConnection;
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

        private void IncludeForeignKeyEntities<TEntity>(TEntity entity, string? fkOnly = null)
        {
            if (entity == null) return;

            var propertiesFK = ToolsEH.GetFKProperties(entity);
            if (propertiesFK == null || propertiesFK.Count == 0)
            {
                Console.WriteLine("No foreign key properties found!");
                return;
            }

            if (!string.IsNullOrEmpty(fkOnly)) // If not all
            {
                propertiesFK = propertiesFK.Where(x => x.Key.ToString() == fkOnly).ToDictionary(x => x.Key, x => x.Value);
            }

            foreach (KeyValuePair<object, object> pair in propertiesFK)
            {
                if (pair.Value != null)
                {
                    var pk = ToolsEH.GetPK(pair.Value);
                    if (pk == null) continue;

                    var propertyToUpdate = entity.GetType().GetProperty(pair.Key.ToString());

                    MethodInfo genericGetMethod = typeof(EnttityHelper).GetMethod("Get").MakeGenericMethod(propertyToUpdate.PropertyType);

                    if (propertyToUpdate != null)
                    {
                        if (genericGetMethod.Invoke(this, new object[] { true, $"{pk.Name}='{pk.GetValue(pair.Value, null)}'" }) is IEnumerable<TEntity> entityFKList)
                        {
                            // Checks if the property is a collection before assigning
                            if (typeof(ICollection<TEntity>).IsAssignableFrom(propertyToUpdate.PropertyType))
                            {
                                if (propertyToUpdate.GetValue(entity) is ICollection<TEntity> collection)
                                {
                                    foreach (var entityFK in entityFKList)
                                    {
                                        collection.Add(entityFK);
                                    }
                                }
                            }
                            else
                            {
                                // If isnt a collection, assign the first entity
                                var entityFK = entityFKList.FirstOrDefault();
                                propertyToUpdate.SetValue(entity, entityFK);
                            }

                        }
                    }
                }
            }
        }





    }



}
