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
    public class EnttityHelper : IEnttityHelper
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


        private readonly Features _features;


        /// <summary>
        /// Allows you to manipulate entities from a connection string.
        /// </summary>
        /// <param name="connectionString"></param>
        public EnttityHelper(string connectionString)
        {
            DbContext = new Database(connectionString);
            _features = new(this);
            _features.DefineTypesDefaultDb(DbContext);
        }

        /// <summary>
        /// Allows you to manipulate entities from a previously created database.
        /// </summary>
        /// <param name="db"></param>
        public EnttityHelper(Database db)
        {
            DbContext = db;
            _features = new(this);
            _features.DefineTypesDefaultDb(DbContext);
        }


        /// <inheritdoc/>  
        public int Insert<TEntity>(TEntity entity, string? namePropUnique = null, bool createTable = true, string? tableName = null)
        {
           return _features.Insert(entity, namePropUnique, createTable, tableName);
        }

        //public int Insert(DataTable dataTable, bool createTable = false, string? tableName = null)
        //{        
        //}

        //public int Insert<TEntity>(DataRow[] dataRow, string? tableName = null)
        //{       
        //}       

        //public bool Insert<TEntity>(IDataReader dataReader, string? tableName = null)
        //{        
        //}

        /// <inheritdoc/> 
        public int InsertLinkSelect(string selectQuery, EnttityHelper db2, string tableName)
        {
          return _features.InsertLinkSelect(selectQuery, db2, tableName);
        }

        /// <inheritdoc/> 
        public int Update<TEntity>(TEntity entity, string? nameId = null, string? tableName = null) where TEntity : class
        {
            return _features.Update(entity, nameId, tableName);
        }

        /// <inheritdoc/> 
        public List<TEntity>? Get<TEntity>(bool includeAll = true, string? filter = null, string? tableName = null)
        {
            return _features.Get<TEntity>(includeAll, filter, tableName);
        }

        /// <inheritdoc/> 
        public TEntity? Search<TEntity>(TEntity entity, bool includeAll = true, string? idPropName = null, string? tableName = null) where TEntity : class
        {
            return _features.Search(entity, includeAll, idPropName, tableName);
        }

        /// <inheritdoc/>
        public bool CheckIfExist(string tableName, string? filter = null, int quantity = 0)
        {
            return _features.CheckIfExist(tableName, filter, quantity);
        }

        /// <inheritdoc/>    
        public bool CreateTable<TEntity>(string? tableName = null, bool createOnlyPrimaryTable = false)
        {
            return _features.CreateTable<TEntity>(tableName, createOnlyPrimaryTable);
        }

        /// <inheritdoc/>
        public bool CreateTable(DataTable dataTable, string? tableName = null)
        {
            return _features.CreateTable(dataTable, tableName);
        }

        /// <inheritdoc/>
        public bool CreateTableIfNotExist<TEntity>(string? tableName = null)
        {
            return _features.CreateTableIfNotExist<TEntity>(tableName);
        }

        /// <inheritdoc/>
        public bool CreateTableIfNotExist(DataTable dataTable, string? tableName = null)
        {
            return _features.CreateTableIfNotExist(dataTable, tableName);
        }

        /// <inheritdoc/>
        public int Delete<TEntity>(TEntity entity, string? nameId = null, string? tableName = null) where TEntity : class
        {
            return _features.Delete(entity, nameId, tableName);
        }

        /// <inheritdoc/>
        public int ExecuteNonQuery(string? query, int expectedChanges = -1)
        {
            return _features.ExecuteNonQuery(query, expectedChanges);
        }

        /// <inheritdoc/>
        public List<TEntity>? ExecuteSelect<TEntity>(string? query)
        {
            return _features.ExecuteSelect<TEntity>(query);
        }

        /// <inheritdoc/>
        public DataTable? ExecuteSelectDt<TEntity>(string? query)
        {
            return _features.ExecuteSelectDt<TEntity>(query);
        }

        /// <inheritdoc/>
        public bool IncludeAll<TEntity>(TEntity entity)
        {
            return _features.IncludeAll(entity);
        }

        /// <inheritdoc/>
        public bool IncludeAll<TEntity>(List<TEntity>? entities)
        {
            return _features.IncludeAll(entities);
        }

        /// <inheritdoc/>
        public bool IncludeEntityFK<TEntity>(TEntity entity, string fkName)
        {
            return _features.IncludeEntityFK(entity, fkName);
        }


    }
}
