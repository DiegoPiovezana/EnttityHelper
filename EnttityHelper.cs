using EH.Command;
using EH.Connection;
using System;
using System.Collections.Generic;
using System.Data;

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
        /// (Optional) Terms (or full names) that can be replaced in table names.
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
        public int Insert<TEntity>(TEntity entity, string? namePropUnique = null, bool createTable = true, string? tableName = null, bool ignoreInversePropertyProperties = false, int timeOutSeconds = 600) where TEntity : class
        {
            try
            {
                return _features.Insert(entity, namePropUnique, createTable, tableName, ignoreInversePropertyProperties);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <inheritdoc/> 
        public int InsertLinkSelect(string selectQuery, EnttityHelper db2, string tableName, int timeOutSeconds = 600)
        {
            try
            {
                return _features.InsertLinkSelect(selectQuery, db2, tableName);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <inheritdoc/> 
        public int LoadCSV(string csvFilePath, bool createTable = true, string? tableName = null, int batchSize = 10000, int timeOutSeconds = 600, char separator = ';')
        {
            try
            {
                return _features.LoadCSV(csvFilePath, createTable, tableName, batchSize, timeOutSeconds, separator);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <inheritdoc/> 
        public int Update<TEntity>(TEntity entity, string? nameId = null, string? tableName = null, bool ignoreInversePropertyProperties = false) where TEntity : class
        {
            try
            {
                return _features.Update(entity, nameId, tableName);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <inheritdoc/> 
        public List<TEntity>? Get<TEntity>(bool includeAll = true, string? filter = null, string? tableName = null) where TEntity : class
        {
            try
            {
                return _features.Get<TEntity>(includeAll, filter, tableName);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <inheritdoc/> 
        public TEntity? Search<TEntity>(TEntity entity, bool includeAll = true, string? idPropName = null, string? tableName = null) where TEntity : class
        {
            try
            {
                return _features.Search(entity, includeAll, idPropName, tableName);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <inheritdoc/>
        public bool CheckIfExist(string tableName, string? filter = null, int quantity = 0)
        {
            try
            {
                return _features.CheckIfExist(tableName, filter, quantity);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <inheritdoc/>    
        public bool CreateTable<TEntity>(bool createOnlyPrimaryTable, ICollection<string>? ignoreProps = null, string? tableName = null)
        {
            try
            {
                return _features.CreateTable<TEntity>(createOnlyPrimaryTable, ignoreProps, tableName);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <inheritdoc/>
        public bool CreateTableIfNotExist<TEntity>(bool createOnlyPrimaryTable, ICollection<string>? ignoreProps = null, string? tableName = null)
        {
            try
            {
                return _features.CreateTableIfNotExist<TEntity>(createOnlyPrimaryTable, ignoreProps, tableName);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <inheritdoc/>
        public bool CreateTable(DataTable dataTable, string? tableName = null)
        {
            try
            {
                return _features.CreateTable(dataTable, tableName);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <inheritdoc/>
        public bool CreateTableIfNotExist(DataTable dataTable, string? tableName = null)
        {
            try
            {
                return _features.CreateTableIfNotExist(dataTable, tableName);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <inheritdoc/>
        public int Delete<TEntity>(TEntity entity, string? nameId = null, string? tableName = null) where TEntity : class
        {
            try
            {
                return _features.Delete(entity, nameId, tableName);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <inheritdoc/>
        public int ExecuteNonQuery(string? query, int expectedChanges = -1)
        {
            try
            {
                return _features.ExecuteNonQuery(query, expectedChanges);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <inheritdoc/>
        public List<TEntity>? ExecuteSelect<TEntity>(string? query)
        {
            try
            {
                return _features.ExecuteSelect<TEntity>(query);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <inheritdoc/>
        public DataTable? ExecuteSelectDt(string? query)
        {
            try
            {
                return _features.ExecuteSelectDt(query);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <inheritdoc/>
        public object? ExecuteScalar(string? query)
        {
            try
            {
                return _features.ExecuteScalar(query);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <inheritdoc/>
        public bool IncludeAll<TEntity>(TEntity entity)
        {
            try
            {
                return _features.IncludeAll(entity);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <inheritdoc/>
        public bool IncludeAll<TEntity>(List<TEntity>? entities)
        {
            try
            {
                return _features.IncludeAll(entities);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <inheritdoc/>
        public bool IncludeEntityFK<TEntity>(TEntity entity, string fkName)
        {
            try
            {
                return _features.IncludeEntityFK(entity, fkName);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <inheritdoc/>
        public bool IncludeInverseEntity<TEntity>(TEntity entity, string inversePropertyName)
        {
            try
            {
                return _features.IncludeInverseEntity(entity, inversePropertyName);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <inheritdoc/>
        public string? GetTableName<TEntity>()
        {
            try
            {
                return _features.GetTableName<TEntity>();
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <inheritdoc/>
        public string? GetTableNameManyToMany(Type entity1, string propCollectionName)
        {
            try
            {
                return _features.GetTableNameManyToMany(entity1, propCollectionName);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <inheritdoc/>
        public string? GetPKName<TEntity>(TEntity entity) where TEntity : class
        {
            try
            {
                return _features.GetPKName(entity);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <inheritdoc/>
        public string NormalizeText(string? text, char replaceSpace = '_', bool toLower = true)
        {
            try
            {
                return _features.NormalizeText(text, replaceSpace, toLower);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <inheritdoc/>
        public string NormalizeColumnOrTableName(string? name, bool replaceInvalidChars = true)
        {
            try
            {
                return _features.NormalizeColumnOrTableName(name, replaceInvalidChars);
            }
            catch (Exception)
            {
                throw;
            }
        }

    }
}
