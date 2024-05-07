using EH.Connection;
using EH.Properties;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;

namespace EH.Command
{
    internal class Features : IEnttityHelper
    {
        private readonly EnttityHelper _enttityHelper;

        public Features(EnttityHelper enttityHelper)
        {
            _enttityHelper = enttityHelper;

        }

        internal void DefineTypesDefaultDb(Database? dbContext)
        {
            if (dbContext is null) throw new InvalidOperationException("DbContext cannot be null.");
            if (dbContext.Type is null) throw new InvalidOperationException("DbContext Type cannot be null.");

            if (dbContext.Type.Equals(Enums.DbType.Oracle))
            {
                _enttityHelper.TypesDefault = new Dictionary<string, string> {
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
                _enttityHelper.TypesDefault = new Dictionary<string, string>
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
                _enttityHelper.TypesDefault = new Dictionary<string, string>
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

        public int Insert<TEntity>(TEntity entity, string? namePropUnique = null, bool createTable = true, string? tableName = null, bool ignoreInversePropertyProperties = false)
        {
            if (entity is DataTable dataTable)
            {
                if (dataTable.Rows.Count == 0) return 0;
                tableName ??= Define.NameTableFromDataTable(dataTable.TableName, _enttityHelper.ReplacesTableName);

                if (!CheckIfExist(tableName) && createTable)
                {
                    CreateTable(dataTable, tableName);
                }

                return Commands.Execute.PerformBulkCopyOperation(_enttityHelper.DbContext, dataTable, tableName) ? dataTable.Rows.Count : 0;
            }
            else if (entity is IDataReader dataReader)
            {
                if (tableName is null) throw new ArgumentNullException(nameof(tableName), "Table name cannot be null.");

                if (!CheckIfExist(tableName) && createTable)
                {
                    _enttityHelper.DbContext.CreateOpenConnection();
                    CreateTable(dataReader.GetFirstRows(10), tableName);
                }

                return Commands.Execute.PerformBulkCopyOperation(_enttityHelper.DbContext, dataReader, tableName) ? 1 : 0;
            }
            else if (entity is DataRow[] dataRow)
            {
                if (dataRow.Length == 0) return 0;
                if (tableName is null) throw new ArgumentNullException(nameof(tableName), "Table name cannot be null.");
                return Commands.Execute.PerformBulkCopyOperation(_enttityHelper.DbContext, dataRow, tableName) ? dataRow.Length : 0;
            }
            else // Entity
            {
                if (!string.IsNullOrEmpty(namePropUnique))
                {
                    var properties = ToolsProp.GetProperties(entity);
                    tableName ??= ToolsProp.GetTableName<TEntity>(_enttityHelper.ReplacesTableName);

                    if (CheckIfExist(tableName, $"{namePropUnique} = '{properties[namePropUnique]}'", 1))
                    {
                        Debug.WriteLine($"EH-101: Entity '{namePropUnique} {properties[namePropUnique]}' already exists in table!");
                        return -101;
                    }

                    if (!CheckIfExist(tableName) && createTable)
                    {
                        CreateTableIfNotExist<TEntity>(false, null, tableName);
                    }
                }

                ICollection<string?> insertsQuery = _enttityHelper.GetQuery.Insert(entity, _enttityHelper.ReplacesTableName, tableName, ignoreInversePropertyProperties);

                //int inserts = 0;
                //foreach (string? insertQuery in insertsQuery)
                //{
                //    inserts += insertQuery is null ? throw new Exception($"EH-000: Error!") : ExecuteNonQuery(insertQuery, 1);
                //}
                int inserts = insertsQuery.Sum(insertQuery => insertQuery is null ? throw new Exception($"EH-000: Error!") : ExecuteNonQuery(insertQuery, 1));

                return inserts;
            }
        }

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

        //public int Insert<TEntity>(DataRow[] dataRow, string? tableName = null)
        //{
        //    if (dataRow.Length == 0) return 0;
        //    if (tableName is null) throw new ArgumentNullException(nameof(tableName), "Table name cannot be null.");
        //    return Commands.Execute.PerformBulkCopyOperation(DbContext, dataRow, tableName) ? dataRow.Length : 0;
        //}

        //public bool Insert<TEntity>(IDataReader dataReader, string? tableName = null)
        //{
        //    if (tableName is null) throw new ArgumentNullException(nameof(tableName), "Table name cannot be null.");
        //    return Commands.Execute.PerformBulkCopyOperation(DbContext, dataReader, tableName);
        //}

        public int InsertLinkSelect(string selectQuery, EnttityHelper db2, string tableName)
        {
            var dataReaderSelect = (IDataReader?)Commands.Execute.ExecuteCommand<IDataReader>(_enttityHelper.DbContext, selectQuery, false, true);
            if (dataReaderSelect is null) return 0;

            int inserts = db2.Insert(dataReaderSelect, tableName, true, tableName);

            dataReaderSelect.Close();
            _enttityHelper.DbContext.CloseConnection();

            return inserts;
        }

        public int Update<TEntity>(TEntity entity, string? nameId = null, string? tableName = null) where TEntity : class
        {
            string? updateQuery = _enttityHelper.GetQuery.Update(entity, nameId, _enttityHelper.ReplacesTableName, tableName);
            return ExecuteNonQuery(updateQuery, 1);
        }

        public List<TEntity>? Get<TEntity>(bool includeAll = true, string? filter = null, string? tableName = null)
        {
            string? querySelect = _enttityHelper.GetQuery.Get<TEntity>(filter, _enttityHelper.ReplacesTableName, tableName);
            var entities = ExecuteSelect<TEntity>(querySelect);
            if (includeAll) { _ = IncludeAll(entities); }
            return entities;
        }

        public TEntity? Search<TEntity>(TEntity entity, bool includeAll = true, string? idPropName = null, string? tableName = null) where TEntity : class
        {
            string? selectQuery = _enttityHelper.GetQuery.Search(entity, idPropName, _enttityHelper.ReplacesTableName, tableName);
            var entities = ExecuteSelect<TEntity>(selectQuery);
            if (includeAll) { _ = IncludeAll(entities.FirstOrDefault()); }
            return entities.FirstOrDefault();
        }

        public bool CheckIfExist(string tableName, string? filter = null, int quantity = 0)
        {
            try
            {
                if (_enttityHelper.DbContext?.IDbConnection is null) throw new InvalidOperationException("Connection does not exist!");

                using IDbConnection dbConnection = _enttityHelper.DbContext.CreateOpenConnection();
                using IDbCommand command = _enttityHelper.DbContext.CreateCommand($"SELECT COUNT(*) FROM {tableName} WHERE {filter ?? "1 = 1"}");
                object result = command.ExecuteScalar(); // >= 0

                if (result != null && result != DBNull.Value) { return Convert.ToInt32(result) >= quantity; }
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
                if (_enttityHelper.DbContext?.IDbConnection is not null && _enttityHelper.DbContext.IDbConnection.State == ConnectionState.Open)
                    _enttityHelper.DbContext.IDbConnection.Close();
            }
        }

        public bool CreateTable<TEntity>(bool createOnlyPrimaryTable = false, ICollection<string>? ignoreProps = null, string? tableName = null)
        {
            if (_enttityHelper.DbContext?.IDbConnection is null) throw new InvalidOperationException("Connection does not exist!");

            var createsTableQuery = _enttityHelper.GetQuery.CreateTable<TEntity>(_enttityHelper.TypesDefault, ignoreProps, createOnlyPrimaryTable, _enttityHelper.ReplacesTableName, tableName);

            foreach (string? createTableQuery in createsTableQuery.Reverse()) // The last table is the main table
            {
                if (ExecuteNonQuery(createTableQuery) != 0) // Return = -1
                {
                    Debug.WriteLine("Table created!");
                }
                else
                {
                    throw new InvalidOperationException("Table not created!");
                }
                //if (createOnlyPrimaryTable) { break; }
            }
            return true;
        }

        public bool CreateTableIfNotExist<TEntity>(bool createOnlyPrimaryTable = false, ICollection<string>? ignoreProps = null, string? tableName = null)
        {
            if (_enttityHelper.DbContext?.IDbConnection is null) throw new InvalidOperationException("Connection does not exist!");
            tableName ??= ToolsProp.GetTableName<TEntity>(_enttityHelper.ReplacesTableName);
            if (CheckIfExist(tableName)) { Debug.WriteLine($"Table '{tableName}' already exists!"); return true; }
            return CreateTable<TEntity>(createOnlyPrimaryTable, ignoreProps, tableName);
        }

        public bool CreateTable(DataTable dataTable, string? tableName = null)
        {
            if (_enttityHelper.DbContext?.IDbConnection is null) throw new InvalidOperationException("Connection does not exist!");

            string? createTableQuery = _enttityHelper.GetQuery.CreateTableFromDataTable(dataTable, _enttityHelper.TypesDefault, _enttityHelper.ReplacesTableName, tableName);

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

        public bool CreateTableIfNotExist(DataTable dataTable, string? tableName = null)
        {
            if (_enttityHelper.DbContext?.IDbConnection is null) throw new InvalidOperationException("Connection does not exist!");
            tableName ??= Define.NameTableFromDataTable(dataTable.TableName, _enttityHelper.ReplacesTableName);
            if (CheckIfExist(tableName)) { Debug.WriteLine($"Table '{tableName}' already exists!"); return true; }
            return CreateTable(dataTable, tableName);
        }

        public int Delete<TEntity>(TEntity entity, string? nameId = null, string? tableName = null) where TEntity : class
        {
            string? deleteQuery = _enttityHelper.GetQuery.Delete(entity, nameId, _enttityHelper.ReplacesTableName, tableName);
            return ExecuteNonQuery(deleteQuery, 1);
        }

        public int ExecuteNonQuery(string? query, int expectedChanges = -1)
        {
            return (int?)Commands.Execute.ExecuteCommand<object>(_enttityHelper.DbContext, query, true, false, expectedChanges) ?? 0;
        }

        public List<TEntity>? ExecuteSelect<TEntity>(string? query)
        {
            return (List<TEntity>?)Commands.Execute.ExecuteCommand<TEntity>(_enttityHelper.DbContext, query);
        }

        public DataTable? ExecuteSelectDt(string? query)
        {
            if (Commands.Execute.ExecuteCommand<IDataReader>(_enttityHelper.DbContext, query, false, true) is not IDataReader resultSelect) return null;
            DataTable dtResult = resultSelect.ToDataTable();
            resultSelect.Close();
            _enttityHelper.DbContext.CloseConnection();
            return dtResult;
        }

        public string? ExecuteSelectScalar(Database database, string query)
        {
           var connection = database.CreateOpenConnection();

            if (connection != null)
            {
                var commando = connection.CreateCommand();
                commando.CommandText = query;
                var result = commando.ExecuteScalar();
                connection.Close();
                return result?.ToString() ?? "";              
            }
            return null;
        }

        public bool IncludeAll<TEntity>(TEntity entity)
        {
            return IncludeAll(new List<TEntity> { entity });
        }

        public bool IncludeAll<TEntity>(List<TEntity>? entities)
        {
            if (entities == null || entities.Count == 0) return false;
            Entities.Inclusions? inclusions = new(_enttityHelper);
            foreach (TEntity entity in entities)
            {
                inclusions.IncludeForeignKeyEntities(entity);
                inclusions.IncludeInverseProperties(entity);
            }
            return true;
        }

        public bool IncludeEntityFK<TEntity>(TEntity entity, string fkName)
        {
            if (entity == null) return false;
            new Entities.Inclusions(_enttityHelper).IncludeForeignKeyEntities(entity, fkName);
            return true;
        }

        public bool IncludeInverseEntity<TEntity>(TEntity entity, string inversePropertyOnly)
        {
            if (entity == null) return false;
            new Entities.Inclusions(_enttityHelper).IncludeInverseProperties(entity, inversePropertyOnly);
            return true;
        }

        public string? GetTableName<TEntity>() => ToolsProp.GetTableName<TEntity>(_enttityHelper.ReplacesTableName);

        public string? GetPKName<TEntity>(TEntity entity) where TEntity : class => ToolsProp.GetPK(entity)?.Name;




    }
}


