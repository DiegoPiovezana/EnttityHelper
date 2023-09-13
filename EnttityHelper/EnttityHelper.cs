using EH.Connection;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Linq;
using System.Text;

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
        /// <param name="entity"></param>
        /// <param name="namePropUnique"></param>
        /// <returns></returns>
        public bool Insert<TEntity>(TEntity entity, string? namePropUnique = null)
        {
            var properties = ToolsEH.GetProperties(entity);
            string tableName = ToolsEH.GetTable<TEntity>();

            if (!string.IsNullOrEmpty(namePropUnique) && CheckIfExist(tableName, $"{namePropUnique} = {properties[namePropUnique]}"))
            {
                Console.WriteLine("Entity already exists in table!");
                return false;
            }

            string columns = string.Join(", ", properties.Keys);
            string values = string.Join("', '", properties.Values);

            string insertQuery = $"INSERT INTO {tableName} ({columns}) VALUES ('{values}')";
            
            IDbConnection connection = DbContext.IDbConnection;
            connection.Open();

            using (var command = DbContext.CreateCommand(insertQuery))
            {
                //command.Parameters.AddWithValue("@Nome", "John Doe");
                //command.Parameters.AddWithValue("@Email", "john.doe@example.com");

                int rowsAffected = command.ExecuteNonQuery();
                connection.Close();
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
        }

        /// <summary>
        /// Allow to update an entity in the database.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="nameId">Entity Id column name.</param>
        /// <returns></returns>
        public int Update<TEntity>(TEntity entity, string nameId)
        {
            StringBuilder queryBuilder = new ();
            queryBuilder.Append($"UPDATE {ToolsEH.GetTable<TEntity>()} SET ");

            var properties = ToolsEH.GetProperties(entity);

            foreach (KeyValuePair<string, object> pair in properties)
            {
                if (pair.Key != nameId)
                {
                    queryBuilder.Append($"{pair.Key} = '{pair.Value}', ");
                }
            }

            queryBuilder.Length -= 2; // Remove the last comma and space
            queryBuilder.Append($" WHERE {nameId} = '{properties[nameId]}'");
            string updateQuery = queryBuilder.ToString();

            IDbConnection connection = DbContext.IDbConnection;
            connection.Open();

            using (var command = DbContext.CreateCommand(updateQuery))
            {
                int rowsAffected = command.ExecuteNonQuery();
                connection.Close();
                Console.WriteLine($"Rows Affected: {rowsAffected}");

                return rowsAffected;

                //if (rowsAffected > 0)
                //{
                //    Console.WriteLine("Successful update!");
                //    return true;
                //}
                //else
                //{
                //    Console.WriteLine("No records updated!");
                //    return false;
                //}
            }

        }

        /// <summary>
        /// Search the entity by <paramref name="idPropName"/>
        /// </summary>
        /// <param name="entity">Entity to be searched for in the bank.</param>
        /// <param name="idPropName">Entity identifier name.</param>
        /// <returns></returns>
        public TEntity? Search<TEntity>(TEntity entity, string idPropName)
        {
            //var properties = ToolsEH.GetProperties(entity);

            IDbConnection connection = DbContext.IDbConnection;
            //string nameProperty = ToolsEH.GetPropertyName(() => idProp);
            //object valueProperty = ToolsEH.GetPropertyValue(entity, nameProperty);

            // $"SELECT * FROM {typeof(TEntity).Name} WHERE {idProp.name)} = {idProp.value}"
            //string selectQuery = $"SELECT * FROM {tableName} WHERE {idProp.GetType().Name} = {idProp}"; 
            string selectQuery = $"SELECT * FROM {ToolsEH.GetTable<TEntity>()} WHERE {idPropName} = {typeof(TEntity).GetProperty(idPropName).GetValue(entity, null)}";
            //string selectQuery = $"SELECT * FROM {tableName} WHERE {idPropName} = {typeof(TEntity).GetProperty(idPropName)}";

            connection.Open();

            using (IDbCommand command = DbContext.CreateCommand(selectQuery))
            {
                //var idParameter = command.CreateParameter();
                //idParameter.ParameterName = "@Id";
                //idParameter.Value = idProp;
                //command.Parameters.Add(idParameter);

                // linha.Field<DateTime>("DT_ALTERACAO")

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        //TEntity entityBd = Activator.CreateInstance<TEntity>();

                        //foreach (PropertyInfo propInfo in typeof(TEntity).GetProperties())
                        //{
                        //    if (propInfo.Name != idPropName)
                        //    {
                        //        propInfo.SetValue(entityBd, Convert.ChangeType(reader[propInfo.Name], propInfo.PropertyType));
                        //    }
                        //}

                        var entities = ToolsEH.MapDataReaderToList1<TEntity>(reader);

                        connection.Close();
                        return entities.FirstOrDefault();
                    }
                }
            }

            connection.Close();
            return default;
        }

        /// <summary>
        /// Gets one or more entities from the database.
        /// </summary>
        /// <param name="objectEntity"></param>
        /// <returns></returns>
        public List<TEntity> Get<TEntity>(string? filter = null)
        {
            //TableAttribute ta = ToolsEH.GetTableAttribute(typeof(TEntity));
            ////string tableName = ta?.Name != null ? ta.Name : typeof(TEntity).Name;
            //string tableName = ta?.Name ?? typeof(TEntity).Name;

            filter = string.IsNullOrEmpty(filter?.Trim()) ? "1 = 1" : filter;

            IDbConnection connection = DbContext.IDbConnection;
            connection.Open();

            using (IDbCommand command = DbContext.CreateCommand($"SELECT * FROM {ToolsEH.GetTable<TEntity>()} WHERE ({filter})"))
            {
                using (var reader = command.ExecuteReader())
                {
                    List<TEntity> entities = ToolsEH.MapDataReaderToList2<TEntity>(reader);
                    connection.Close();
                    return entities;
                }
            }

        }

        /// <summary>
        /// Checks if table exists.
        /// </summary>
        /// <param name="nameTable"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
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
                //IDbConnection connection = DbContext.IDbConnection;
                //connection.Close();
                return false;
            }            
        }

        /// <summary>
        /// Execute the query.
        /// </summary>
        /// <param name="query">Custom query to be executed.</param>
        /// <returns>DataReader.</returns>
        public IDataReader? CustomCommand(string query)
        {
            IDbConnection connection = DbContext.IDbConnection;
            connection.Open();

            using (IDbCommand command = DbContext.CreateCommand(query))
            {
                IDataReader? result = command.ExecuteReader();
                connection.Close();
                return result;
            }
        }   



    }

}
