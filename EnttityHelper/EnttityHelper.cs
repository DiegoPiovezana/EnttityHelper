using EH.Connection;
using System;
using System.Collections.Generic;
using System.Data;
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
        /// Allows you to manipulate entities from a connection string.
        /// </summary>
        /// <param name="connectionString"></param>
        public EnttityHelper(string connectionString)
        {
            DbContext = new Database(connectionString);
        }

        /// <summary>
        /// Allows you to manipulate entities from a previously created database.
        /// </summary>
        /// <param name="db"></param>
        public EnttityHelper(Database db)
        {
            DbContext = db;
        }



        /// <summary>
        /// Allow to insert an entity in the database.
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to be manipulated.</typeparam>
        /// <param name="entity">Entity to be inserted into the database.</param>
        /// <param name="namePropUnique">Name of the property to be considered as a uniqueness criterion (optional).</param>
        /// <returns>True, if one or more entities are inserted into the database.</returns>
        public bool Insert<TEntity>(TEntity entity, string? namePropUnique = null)
        {
            string? insertQuery = Commands.Insert(this, entity, namePropUnique);

            int rowsAffected = ExecuteNonQuery(insertQuery);
            Console.WriteLine($"Rows Affected: {rowsAffected}");

            if (rowsAffected > 0)
            {
                Console.WriteLine("Insertion successful!");
                return true;
            }
            else
            {
                Console.WriteLine("No records entered!");
                return false;
            }
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
            string? updateQuery = Commands.Update(entity, nameId);

            return ExecuteNonQuery(updateQuery);
        }

        /// <summary>
        /// Search the entity by <paramref name="idPropName"/>
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to be manipulated.</typeparam>
        /// <param name="entity">Entity to be searched for in the bank.</param>
        /// <param name="idPropName">Entity identifier name.</param>
        /// <param name="includeAll">Defines whether it will include all other FK entities.</param>
        /// <returns></returns>
        public TEntity? Search<TEntity>(TEntity entity, string? idPropName = null, bool includeAll = true) where TEntity : class
        {
            string? selectQuery = Commands.Search(entity, idPropName, includeAll);
            if (string.IsNullOrEmpty(selectQuery))
            //if (selectQuery is null)
            {
                Console.WriteLine("Search command not exists!");
                return default;
            }

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
            string? query = Commands.Get<TEntity>(includeAll, filter);
            if (string.IsNullOrEmpty(query))
            {
                Console.WriteLine("Get command not exists!");
                return new List<TEntity>();
            }

            var entities = ExecuteSelect<TEntity>(query);
            if (includeAll) { _ = IncludeAll(entities); }
            return entities;
        }

        /// <summary>
        /// Execute the query.
        /// </summary>
        /// <param name="query">Custom query to be executed.</param>
        /// <returns>Object.</returns>
        public object? CustomCommand(string query)
        {
            //if (string.IsNullOrEmpty(query)) { Console.WriteLine("Query not exists!"); return null; }
            //if (DbContext?.IDbConnection is null) { Console.WriteLine("Connection not exists!"); return null; }

            //IDbConnection connection = DbContext.IDbConnection;
            //connection.Open();
            //using IDbCommand command = DbContext.CreateCommand(query);
            //using var result = command.ExecuteReader();
            //connection.Close();
            //return result;

            return ExecuteCommand<object>(query);
        }

        /// <summary>
        /// Executes the non query (Create, Alter, Drop, Insert, Update or Delete) on the database.
        /// </summary>    
        private int ExecuteNonQuery(string? query)
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

        private List<TEntity>? ExecuteSelect<TEntity>(string query)
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

            using IDbConnection connection = DbContext.IDbConnection;
            connection.Open();

            using IDbCommand command = DbContext.CreateCommand(query);

            if (isNonQuery)
            {
                int rowsAffected = command.ExecuteNonQuery();
                //connection.Close();
                Console.WriteLine($"Rows Affected: {rowsAffected}");
                return rowsAffected;
            }
            else // isSelect
            {
                using var reader = command.ExecuteReader();
                if (reader != null)
                {
                    List<TEntity> entities = ToolsEH.MapDataReaderToList<TEntity>(reader);
                    //connection.Close();
                    Console.WriteLine($"{(entities?.Count) ?? 0} entities mapped!");
                    return entities;
                }

                return null;
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

        /// <summary>
        /// Include all FK entities.
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to be manipulated.</typeparam>
        /// <param name="entity">Entity that will have its FK entities included.</param>
        /// <returns></returns>
        public bool IncludeAll<TEntity>(TEntity entity)
        {
            return IncludeAll(new List<TEntity> { entity });
        }

        /// <summary>
        /// Include all FK entities.
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to be manipulated.</typeparam>
        /// <param name="entities">Entities that will have their FK entities included.</param>
        /// <returns></returns>
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


        /// <summary>
        /// Checks if table exists.
        /// </summary>
        /// <param name="nameTable">Name of the table to check if it exists.</param>
        /// <param name="filter"></param>
        /// <returns>Possible filter.</returns>
        public bool CheckIfExist(string nameTable, string? filter = null)
        {
            try
            {
                IDbConnection connection = DbContext.IDbConnection;
                connection.Open();

                using (IDbCommand command = DbContext.CreateCommand($"SELECT COUNT(*) FROM {nameTable} WHERE {filter ?? "1 = 1"}"))
                {
                    object result = command.ExecuteScalar();
                    if (result != null && result != DBNull.Value) { connection.Close(); return Convert.ToInt32(result) > 0; }
                }

                connection.Close();
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

    }

}
