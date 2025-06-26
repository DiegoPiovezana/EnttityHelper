using EH.Commands;
using EH.Connection;
using EH.Properties;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EH.Command
{
    internal class Features : IEnttityHelper
    {
        private readonly EnttityHelper _enttityHelper;

        public Features(EnttityHelper enttityHelper)
        {
            _enttityHelper = enttityHelper;
        }

        public long Insert<TEntity>(TEntity entity, bool setPrimaryKeyAfterInsert, string? namePropUnique, bool createTable, string? tableName, bool ignoreInversePropertyProperties, int timeOutSeconds) where TEntity : class
        {
            if (entity is DataTable dataTable) return InsertDataTable(createTable, ref tableName, timeOutSeconds, dataTable);
            if (entity is IDataReader dataReader) return InsertIDataReader(createTable, tableName, timeOutSeconds, dataReader);
            if (entity is DataRow[] dataRows) return InsertDataRows(tableName, timeOutSeconds, dataRows);
            return InsertEntities(entity, setPrimaryKeyAfterInsert, namePropUnique, createTable, ref tableName, ignoreInversePropertyProperties); // Entity or IEnumerable<Entity>


            long InsertDataTable(bool createTable, ref string? tableName, int timeOutSeconds, DataTable dataTable)
            {
                if (dataTable.Rows.Count == 0) return 0;

                tableName ??= Definitions.NameTableFromDataTable(dataTable.TableName, _enttityHelper.ReplacesTableName);

                if (!CheckIfExist(tableName, 0, null) && createTable)
                {
                    CreateTable(dataTable, tableName);
                }

                return Execute.PerformBulkCopyOperation(_enttityHelper.DbContext, dataTable, tableName, timeOutSeconds);
            }

            long InsertIDataReader(bool createTable, string? tableName, int timeOutSeconds, IDataReader dataReader)
            {
                if (tableName is null) throw new ArgumentNullException(nameof(tableName), "Table name cannot be null.");

                if (!CheckIfExist(tableName, 0, null) && createTable)
                {
                    _enttityHelper.DbContext.CreateOpenConnection();
                    var dt = dataReader.GetFirstRows(10);
                    CreateTable(dt, tableName);
                    return -942; // Because IDataReader                   
                }

                var result = Execute.PerformBulkCopyOperation(_enttityHelper.DbContext, dataReader, tableName, timeOutSeconds);
                if (result == -1) result = CountTable(tableName, null);
                return result;
            }

            long InsertDataRows(string? tableName, int timeOutSeconds, DataRow[] dataRows)
            {
                if (dataRows.Length == 0) return 0;

                if (tableName is null) throw new ArgumentNullException(nameof(tableName), "Table name cannot be null.");

                return Execute.PerformBulkCopyOperation(_enttityHelper.DbContext, dataRows, tableName, timeOutSeconds);
            }

            long InsertEntities<TEntity>(TEntity entity, bool setPrimaryKeyAfterInsert, string? namePropUnique, bool createTable, ref string? tableName, bool ignoreInversePropertyProperties) where TEntity : class
            {
                if (entity is null) throw new InvalidOperationException($"$'{nameof(entity)}' is null!");
                
                var insertsQueriesEntities = new Dictionary<object, List<QueryCommand?>?>();
                var entities = entity as IEnumerable ?? new[] { entity };

                var entityFirst = entities.Cast<object>().FirstOrDefault() ?? throw new InvalidOperationException($"$'{nameof(entity)}' is invalid!");

                // TODO: If >100, use bulk insert - test performance

                // Check FK table
                // Dictionary<object, object>? fkProperties = ToolsProp.GetFKProperties(entityFirst);
                Dictionary<string, object>? fkProperties = ToolsProp.GetForeignKeyEntities(entityFirst);
                if (fkProperties != null)
                {
                    foreach (var fkProp in fkProperties)
                    {
                        string tableNamefkProp = ToolsProp.GetTableName(fkProp.Value.GetType(), _enttityHelper.ReplacesTableName);
                        var pkFk = fkProp.Value.GetPrimaryKey();

                        string pkNameFk = pkFk.Name;
                        string pkValueFk = pkFk.GetValue(fkProp.Value, null).ToString();

                        if (!CheckIfExist(tableNamefkProp, 1, $"{pkNameFk} = {pkValueFk}"))
                            throw new InvalidOperationException($"Entity {fkProp.Value.GetType()} with {pkNameFk} '{pkValueFk}' or table '{tableNamefkProp}' does not exist!");
                    }
                }

                // Check MxN table
                if (!ignoreInversePropertyProperties)
                {
                    List<PropertyInfo>? inverseProperties = ToolsProp.GetInverseProperties(entityFirst);
                    if (inverseProperties != null)
                    {
                        foreach (var inverseProp in inverseProperties)
                        {
                            Type propInverseType = inverseProp.PropertyType.GetGenericArguments()[0];
                            string tableNameInverseProp = ToolsProp.GetTableName(propInverseType, _enttityHelper.ReplacesTableName);

                            if (!CheckIfExist(tableNameInverseProp, 0, null))
                            {
                                if (createTable) CreateTable<TEntity>(false, null, tableNameInverseProp);
                                else throw new InvalidOperationException($"Table '{tableNameInverseProp}' does not exist!");
                            }
                        }
                    }
                }

                var itemType = typeof(TEntity).IsGenericType && typeof(IEnumerable).IsAssignableFrom(typeof(TEntity))
                    ? typeof(TEntity).GetGenericArguments()[0]
                    : typeof(TEntity);

                tableName ??= ToolsProp.GetTableName(itemType, _enttityHelper.ReplacesTableName);

                if (!CheckIfExist(tableName, 0, null))
                {
                    if (createTable) CreateTable<TEntity>(false, null, tableName);
                    else throw new InvalidOperationException($"Table '{tableName}' does not exist!");
                }

                foreach (var entityItem in entities)
                {
                    if (!string.IsNullOrEmpty(namePropUnique))
                    {
                        var properties = ToolsProp.GetProperties(entityItem, true, false); // TODO: ToolsProp.GetProperty

                        // Check if ENTITY exists (duplicates)
                        if (CheckIfExist(tableName, 1, $"{namePropUnique} = '{properties[namePropUnique]}'"))
                        {
                            //Debug.WriteLine($"EH-101: Entity '{namePropUnique} {properties[namePropUnique]}' already exists in table!");
                            //return -101;
                            throw new Exception($"EH-101: Entity with {namePropUnique} '{properties[namePropUnique]}' already exists in table '{tableName}'!");
                        }
                    }

                    insertsQueriesEntities[entityItem] = _enttityHelper.GetQuery.Insert(entityItem, _enttityHelper.DbContext.Provider, _enttityHelper.ReplacesTableName, tableName, ignoreInversePropertyProperties).ToList();
                }

                long insertions = 0;

                foreach (var insertQueriesEntity in insertsQueriesEntities)
                {
                    if (insertQueriesEntity.Value == null) throw new Exception("EH-000: Insert query does not exist!");

                    var pk = ToolsProp.GetPrimaryKey(insertQueriesEntity.Key) ?? throw new Exception("EH-000: Entity does not have a primary key!");
                    var id = Execute.ExecuteScalar(_enttityHelper.DbContext,new List<QueryCommand?> { insertQueriesEntity.Value.First() }).First(); // Inserts the main entity
                    if (id == null || id == DBNull.Value) throw new Exception("EH-000: Insert query does not return an ID!");

                    var typePk = pk.PropertyType;
                    var targetType  = Nullable.GetUnderlyingType(typePk) ?? typePk;
                    // var convertedId = typePk.IsAssignableFrom(id.GetType()) ? id : Convert.ChangeType(id.ToString(), typePk);
                    // var convertedId = typePk.IsAssignableFrom(id.GetType()) ? id : Convert.ChangeType(id, targetType);

                    object convertedId;
                    if (targetType == typeof(Guid))
                    {
                        if (id is byte[] bytes && bytes.Length == 16) { convertedId = new Guid(bytes); }
                        else if (id is string s) { convertedId = Guid.Parse(s); }
                        else if (id is Guid guid) { convertedId = guid; }
                        else { throw new InvalidCastException($"Could not convert type '{id?.GetType()}' to Guid."); }
                    }
                    else
                    {
                        convertedId = typePk.IsAssignableFrom(id.GetType()) ? id : Convert.ChangeType(id, targetType);
                    }

                    //if (setPrimaryKeyAfterInsert)
                    //{
                    //    if (!pk.CanWrite || pk.SetMethod == null || !pk.SetMethod.IsPublic)
                    //        throw new Exception($"EH-000: Property '{pk.Name}' does not have a public setter.");

                    //    pk.SetValue(insertQueriesEntity.Key, convertedId);
                    //}

                    if (setPrimaryKeyAfterInsert)
                    {
                        if (pk.CanWrite || pk.SetMethod != null)
                        {
                            try
                            {
                                pk.SetValue(insertQueriesEntity.Key, convertedId);
                            }
                            catch (Exception ex)
                            {
                                throw new Exception($"EH-000: Failed to set primary key '{pk.Name}' via SetValue.", ex);
                            }
                        }
                        else
                        {
                            // Try to set via backing field
                            var backingField = entity.GetType().GetField($"<{pk.Name}>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
                            if (backingField != null)
                            {
                                backingField.SetValue(insertQueriesEntity.Key, convertedId);
                            }
                            else
                            {
                                throw new Exception($"EH-000: Property '{pk.Name}' is readonly and no backing field was found.");
                            }
                        }
                    }

                    insertions++;

                    // Useful for MxN
                    for (int i = 1; i < insertQueriesEntity.Value.Count; i++)
                    {
                        // insertQueriesEntity.Value[i] = insertQueriesEntity.Value[i].Replace("'&ID1'", $"'{id}'");
                        insertQueriesEntity.Value[i].Parameters[$"{_enttityHelper.DbContext.PrefixParameter}ID1"].Value = id;
                        
                        // insertions += ExecuteNonQuery(insertQueriesEntity.Value[i], 1);
                        insertions += Execute.ExecuteNonQuery(_enttityHelper.DbContext, new List<QueryCommand?>{ insertQueriesEntity.Value[i] }, 1).First();
                    }
                }

                return insertions;
            }
        }

        public long InsertLinkSelect(string selectQuery, EnttityHelper db2, string tableName, int timeOutSeconds)
        {
            long inserts;
            do
            {
                var dataReaderSelect = (IDataReader?)_enttityHelper.DbContext.ExecuteReader<IDataReader>(selectQuery, true, null, 0, null, null, true);
                if (dataReaderSelect is null) return 0;
                inserts = db2.Insert(dataReaderSelect, false, null, true, tableName, true, timeOutSeconds);
                dataReaderSelect.Close();
            } while (inserts == -942);

            _enttityHelper.DbContext.CloseConnection();
            return inserts;
        }


        public long LoadCSV(string csvFilePath, bool createTable, string? tableName, int batchSize, int timeOutSeconds, char delimiter, bool hasHeader, string? rowsToLoad, Encoding? encodingRead)
        {
            Validations.Validate.IsFileValid(csvFilePath);
            long totalInserts = 0;

            try
            {
                int rowCount = File.ReadLines(csvFilePath).Count();
                var rowsSelected = Definitions.DefineRows(rowsToLoad, rowCount);
                var hashRowsSelected = new HashSet<int>(rowsSelected);
                int rowIndex = 0;
                encodingRead ??= Encoding.UTF8;

                DataTable dataTable = new()
                {
                    TableName = Path.GetFileNameWithoutExtension(csvFilePath)
                };

                using StreamReader reader = new(csvFilePath, encodingRead);

                string[]? headers = null;
                int indexFirstRow = hashRowsSelected.Min();
                while (rowIndex < indexFirstRow)
                {
                    // Read the first row
                    // Header must contain at least the delimiters 
                    headers = Tools.ParseCsvLine(reader.ReadLine(), delimiter) ?? throw new InvalidOperationException("CSV/TXT file is empty or headers are missing.");
                    rowIndex++;
                }

                if (headers != null)
                {
                    if (hasHeader)
                    {
                        for (int i = 0; i < headers.Length; i++)
                        {
                            string columnName = string.IsNullOrWhiteSpace(headers[i]) ? $"ColumnEmpty_{i + 1}" : headers[i].Trim();
                            dataTable.Columns.Add(new DataColumn(columnName));
                        }
                    }
                    else
                    {
                        for (int i = 0; i < headers.Length; i++)
                        {
                            dataTable.Columns.Add(new DataColumn($"Column{i + 1}"));
                        }

                        DataRow firstRow = dataTable.NewRow();
                        for (int i = 0; i < headers.Length; i++)
                        {
                            firstRow[i] = headers[i];
                        }

                        if (hashRowsSelected.Contains(indexFirstRow)) dataTable.Rows.Add(firstRow);
                    }
                }

                tableName ??= Definitions.NameTableFromDataTable(dataTable.TableName, _enttityHelper.ReplacesTableName);

                if (!CheckIfExist(tableName, 0, null) && createTable)
                {
                    CreateTable(dataTable, tableName);
                }

                // Read and load rows
                while (!reader.EndOfStream)
                {
                    string[] rows = Tools.ParseCsvLine(reader.ReadLine(), delimiter) ?? throw new InvalidOperationException("Error reading a row from the CSV/TXT file.");
                    rowIndex++;

                    if (hashRowsSelected.Contains(rowIndex))
                    {
                        if (rows.Length != headers.Length)
                        {
                            Debug.WriteLine($"Mismatch between CSV/TXT header and row column count in row {rowIndex}");
                            throw new InvalidOperationException($"Mismatch between CSV/TXT header ({headers.Length} columns) and row column count in row {rowIndex} ({rows.Length} columns).");
                        }

                        DataRow row = dataTable.NewRow();
                        for (int i = 0; i < headers.Length; i++) { row[i] = rows[i]?.Trim(); }

                        //if (hashRowsSelected.Contains(rowIndex)) 
                        dataTable.Rows.Add(row);

                        if (dataTable.Rows.Count >= batchSize)
                        {
                            totalInserts += _enttityHelper.DbContext.PerformBulkCopyOperation(dataTable, tableName, timeOutSeconds);
                            dataTable.Clear();
                        }
                    }
                }

                if (dataTable.Rows.Count > 0) // Remaing selected rows
                {
                    totalInserts += _enttityHelper.DbContext.PerformBulkCopyOperation(dataTable, tableName, timeOutSeconds);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error during CSV/TXT loading process: {ex.Message}", ex);
            }
            return totalInserts;
        }


        public long Update<TEntity>(TEntity entity, string? nameId, string? tableName, bool ignoreInversePropertyProperties) where TEntity : class
        {
            // Entity or IEnumerable<Entity>
            var updatesQueriesEntities = new Dictionary<object, List<QueryCommand?>?>();
            var entities = entity as IEnumerable ?? new[] { entity };

            var itemType = typeof(TEntity).IsGenericType && typeof(IEnumerable).IsAssignableFrom(typeof(TEntity))
                ? typeof(TEntity).GetGenericArguments()[0]
                : typeof(TEntity);

            tableName ??= ToolsProp.GetTableName(itemType, _enttityHelper.ReplacesTableName);

            foreach (var entityItem in entities)
            {
                var queriesUpdate = _enttityHelper.GetQuery.Update(entityItem, _enttityHelper, nameId, tableName, ignoreInversePropertyProperties);
                updatesQueriesEntities[entityItem] = queriesUpdate.Reverse().ToList();
            }

            long updates = 0;

            foreach (var updateQueriesEntity in updatesQueriesEntities)
            {
                if (updateQueriesEntity.Value == null) throw new Exception("EH-000: Update query does not exist!");

                updates += updateQueriesEntity.Value.Sum(updateQuery => 
                        updateQuery is null 
                            ? throw new Exception($"EH-000: Error update query!")
                            : _enttityHelper.DbContext.ExecuteNonQuery(new[] { (QueryCommand?)updateQuery }, 1).First()
                            );
            }
            return updates;
        }
        
        // public IList Get(Type entityType, bool includeAll, string? filter, string? tableName, int? pageSize, int pageIndex, string? sortColumn, bool sortAscending)
        // {
        //     if (entityType == null) throw new ArgumentNullException(nameof(entityType));
        //     var genericGetDef = typeof(Features).GetMethod(
        //         nameof(Features.Get),
        //         new[] 
        //         { 
        //             typeof(bool), typeof(string), typeof(string), typeof(int?), typeof(int), typeof(string), typeof(bool) 
        //         }
        //     ) ?? throw new InvalidOperationException("Get<T> method not found in Features class.");
        //     
        //     MethodInfo getClosed = genericGetDef.MakeGenericMethod(entityType);
        //     
        //     object? result = getClosed.Invoke(
        //         this,
        //         new object?[]
        //         {
        //             includeAll,
        //             filter,
        //             tableName,
        //             pageSize,
        //             pageIndex,
        //             sortColumn,
        //             sortAscending
        //         }
        //     );
        //     
        //     return (IList)(result ?? Array.Empty<object>());
        // }
        
        // TODO: adjust filter parameter
        public List<TEntity>? Get<TEntity>(bool includeAll, string? filter, string? tableName, int? pageSize, int pageIndex, string? sortColumn, bool sortAscending) where TEntity : class
        {
            var querySelect = _enttityHelper.GetQuery.Get<TEntity>(filter, _enttityHelper.ReplacesTableName, tableName);
            var entities = ExecuteSelect<TEntity>(querySelect, pageSize, pageIndex, null, sortColumn, sortAscending);
            if (includeAll) { _ = IncludeAllRange(entities); }
            return entities;
        }

        public TEntity? Search<TEntity>(TEntity entity, bool includeAll, string? idPropName, string? tableName) where TEntity : class
        {
            var selectQuery = _enttityHelper.GetQuery.Search(entity, idPropName, _enttityHelper.ReplacesTableName, tableName);
            var entities = ExecuteSelect<TEntity>(selectQuery, null, 0, null, null, true);
            if (includeAll) { _ = IncludeAll(entities.FirstOrDefault()); }
            return entities.FirstOrDefault();
        }

        public bool CheckIfExist(string tableName, int minRecords, string? filter)
        {
            try
            {
                if (_enttityHelper.DbContext?.IDbConnection is null) throw new InvalidOperationException("Connection does not exist!");

                using IDbConnection dbConnection = _enttityHelper.DbContext.CreateOpenConnection();
                var countQuery = _enttityHelper.GetQuery.CheckIfExist(tableName, filter);
                using IDbCommand command = _enttityHelper.DbContext.CreateCommand(countQuery);
                object result = command.ExecuteScalar(); // >= 0

                if (result != null && result != DBNull.Value) { return Convert.ToInt32(result) >= minRecords; }
                return false;
            }
            catch (OracleException ex) when (ex.Number == 942)
            {
                return false; // ORA-00942: table or view does not exist
            }
            catch (SqlException ex) when (ex.Number == 208)
            {
                return false; // Invalid object name 'tableName'.
            }
            //catch (SQLiteException ex) when (ex.ErrorCode == SQLiteErrorCode.Table)
            //{
            //    return false;
            //}
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
            finally
            {
                if (_enttityHelper.DbContext?.IDbConnection is not null && _enttityHelper.DbContext.IDbConnection.State == ConnectionState.Open)
                    _enttityHelper.DbContext.IDbConnection.Close();
            }
        }

        public bool CheckIfExist<TEntity>(TEntity entity, string? tableName = null, string? nameId = null) where TEntity : class
        {
            try
            {
                var itemType = typeof(TEntity).IsGenericType && typeof(IEnumerable).IsAssignableFrom(typeof(TEntity))
                    ? typeof(TEntity).GetGenericArguments()[0]
                    : typeof(TEntity);

                tableName ??= ToolsProp.GetTableName(itemType, _enttityHelper.ReplacesTableName);
                nameId ??= ToolsProp.GetPrimaryKey(entity).Name;

                var query = _enttityHelper.GetQuery.CheckIfExist(entity, nameId);

                var result = _enttityHelper.DbContext.ExecuteScalar(new List<QueryCommand?> { query }).FirstOrDefault();
                return result != null && result != DBNull.Value;
            }
            catch (Exception ex)
            {
                // Log or rethrow
                return false;
            }
            finally
            {
                if (_enttityHelper.DbContext?.IDbConnection is not null && _enttityHelper.DbContext.IDbConnection.State == ConnectionState.Open)
                    _enttityHelper.DbContext.IDbConnection.Close();
            }
        }



        public long CountEntity<TEntity>(TEntity entity, string? tableName, string? nameId) where TEntity : class
        {
            try
            {
                // Entity or IEnumerable<Entity>
                var countQueriesEntities = new Dictionary<object, List<QueryCommand?>?>();
                var entities = entity as IEnumerable ?? new[] { entity };

                var itemType = typeof(TEntity).IsGenericType && typeof(IEnumerable).IsAssignableFrom(typeof(TEntity))
                    ? typeof(TEntity).GetGenericArguments()[0]
                    : typeof(TEntity);

                tableName ??= ToolsProp.GetTableName(itemType, _enttityHelper.ReplacesTableName);
                nameId ??= ToolsProp.GetPrimaryKey(entity).Name;

                foreach (var entityItem in entities)
                {
                    var queryCheck = _enttityHelper.GetQuery.CountEntity(entityItem, nameId, _enttityHelper.ReplacesTableName, tableName);
                    countQueriesEntities[entityItem] = new List<QueryCommand?>() { queryCheck };
                }

                //int count = 0;
                //foreach (var countQueriesEntity in countQueriesEntities)
                //{
                //    if (countQueriesEntity.Value == null) throw new Exception("EH-000: Count query does not exist!");

                //    var result = ExecuteScalar(countQueriesEntity.Value);
                //    if (result != null && result != DBNull.Value) count += Convert.ToInt32(result);
                //}
                var countQueries = countQueriesEntities.Values.SelectMany(x => x).ToList();
                var result = Execute.ExecuteScalar(_enttityHelper.DbContext, countQueries).FirstOrDefault();
                if (result != null && result != DBNull.Value) { return Convert.ToInt64(result); }
                return -1;
            }
            catch (OracleException ex) when (ex.Number == 942)
            {
                return -1; // ORA-00942: table or view does not exist
            }
            catch (SqlException ex) when (ex.Number == 208)
            {
                return -1; // Invalid object name 'tableName'.
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

        public long CountTable(string tableName, string? filter)
        {
            try
            {
                if (_enttityHelper.DbContext?.IDbConnection is null) throw new InvalidOperationException("Connection does not exist!");

                using IDbConnection dbConnection = _enttityHelper.DbContext.CreateOpenConnection();
                using IDbCommand command = _enttityHelper.DbContext.CreateCommand($"SELECT COUNT(*) FROM {tableName} WHERE {filter ?? "1 = 1"}");
                object result = command.ExecuteScalar();

                return (result != null && result != DBNull.Value) ? Convert.ToInt64(result) : 0;
            }
            catch (OracleException ex) when (ex.Number == 942) // ORA-00942: table or view does not exist
            {
                return -1;
            }
            catch (SqlException ex) when (ex.Number == 208) // Invalid object name 'tableName'
            {
                return -1;
            }
            //catch (SQLiteException ex) when (ex.ErrorCode == SQLiteErrorCode.Table)
            //{
            //    return -1;
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

        public async Task<long> GetTotalRecordCountAsync(string baseQuery, string? filter = null)
        {
            try
            {
                if (string.IsNullOrEmpty(baseQuery))
                    throw new ArgumentException("Base query cannot be null or empty.", nameof(baseQuery));

                var countQuery = new SqlQueryString(_enttityHelper).CountQuery(baseQuery, filter);

                using (var connection = _enttityHelper.DbContext.CreateOpenConnection())
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = countQuery.Sql;

                    var result = Task.Run(() => command.ExecuteScalar()).Result;
                    return Convert.ToInt64(result);
                }
            }
            catch (OracleException ex) when (ex.Number == 942) // ORA-00942: table or view does not exist
            {
                return -1;
            }
            catch (SqlException ex) when (ex.Number == 208) // Invalid object name 'tableName'
            {
                return -1;
            }
            //catch (SQLiteException ex) when (ex.ErrorCode == SQLiteErrorCode.Table)
            //{
            //    return -1;
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

        public bool CreateTable<TEntity>(bool createOnlyPrimaryTable, ICollection<string>? ignoreProps, string? tableName) where TEntity : class
        {
            if (_enttityHelper.DbContext?.IDbConnection is null) throw new InvalidOperationException("Connection does not exist!");
            var createsTableQuery = _enttityHelper.GetQuery.CreateTable<TEntity>(_enttityHelper.TypesDefault, createOnlyPrimaryTable, ignoreProps, _enttityHelper.ReplacesTableName, tableName);
            var queryCreates = createsTableQuery.Values.Reverse().ToList();
            var creates = Execute.ExecuteNonQuery(_enttityHelper.DbContext, queryCreates, -1); // The last table is the main table
            return createsTableQuery.Count == creates.Count;
        }

        public bool CreateTableIfNotExist<TEntity>(bool createOnlyPrimaryTable, ICollection<string>? ignoreProps, string? tableName) where TEntity : class
        {
            if (_enttityHelper.DbContext?.IDbConnection is null) throw new InvalidOperationException("Connection does not exist!");
            tableName ??= ToolsProp.GetTableName<TEntity>(_enttityHelper.ReplacesTableName);

            if (CheckIfExist(tableName, 0, null))
            {
                Debug.WriteLine($"Table '{tableName}' already exists!");
                if (createOnlyPrimaryTable) return true;
            }

            //return CreateTable<TEntity>(createOnlyPrimaryTable, ignoreProps, tableName);

            Dictionary<string, QueryCommand?>? createsTablesQueries = _enttityHelper.GetQuery.CreateTable<TEntity>(_enttityHelper.TypesDefault, createOnlyPrimaryTable, ignoreProps, _enttityHelper.ReplacesTableName, tableName);
            foreach (KeyValuePair<string, QueryCommand?> createTableQuery in createsTablesQueries)
            {
                if (CheckIfExist(createTableQuery.Key, 0, null))
                {
                    Debug.WriteLine($"Table '{createTableQuery.Key}' exists!");
                    createsTablesQueries.Remove(createTableQuery.Key);
                }
            }

            if (createsTablesQueries.Count == 0) return true;

            var queryCreates = createsTablesQueries.Values.Reverse().ToList();
            var creates = Execute.ExecuteNonQuery(_enttityHelper.DbContext, queryCreates, -1); // The last table is the main table
            return createsTablesQueries.Count == creates.Count;
        }

        public bool CreateTable(DataTable dataTable, string? tableName)
        {
            if (_enttityHelper.DbContext?.IDbConnection is null) throw new InvalidOperationException("Connection does not exist!");

            var createTableQuery = _enttityHelper.GetQuery.CreateTableFromDataTable(dataTable, _enttityHelper.TypesDefault, _enttityHelper.ReplacesTableName, tableName);

            if (Execute.ExecuteNonQuery(_enttityHelper.DbContext, new List<QueryCommand?>(){createTableQuery}, -1).First() != 0) // Return = -1
            {
                Debug.WriteLine("Table created!");
                return true;
            }
            else
            {
                throw new InvalidOperationException("Table not created!");
            }
        }

        public bool CreateTableIfNotExist(DataTable dataTable, string? tableName)
        {
            if (_enttityHelper.DbContext?.IDbConnection is null) throw new InvalidOperationException("Connection does not exist!");
            tableName ??= Definitions.NameTableFromDataTable(dataTable.TableName, _enttityHelper.ReplacesTableName);
            if (CheckIfExist(tableName, 0, null)) { Debug.WriteLine($"Table '{tableName}' already exists!"); return true; }
            return CreateTable(dataTable, tableName);
        }

        //public bool CreateTableInverseProperty<TEntity>(TEntity entity, string inversePropertyName, string? tableName = null)
        //{
        //    if (_enttityHelper.DbContext?.IDbConnection is null) throw new InvalidOperationException("Connection does not exist!");

        //    var createsTableQuery = _enttityHelper.GetQuery.CreateTable<TEntity>(_enttityHelper.TypesDefault, ignoreProps, null, _enttityHelper.ReplacesTableName, tableName);

        //    foreach (string? createTableQuery in createsTableQuery.Reverse()) // The last table is the main table
        //    {
        //        if (ExecuteNonQuery(createTableQuery) != 0) // Return = -1
        //        {
        //            Debug.WriteLine("Table created!");
        //        }
        //        else
        //        {
        //            throw new InvalidOperationException("Table not created!");
        //        }
        //        //if (createOnlyPrimaryTable) { break; }
        //    }
        //    return true;
        //}

        public long Delete<TEntity>(TEntity entity, string? nameId, string? tableName) where TEntity : class
        {
            // Entity or IEnumerable<Entity>
            var deletesQueriesEntities = new Dictionary<object, List<QueryCommand?>?>();
            var entities = entity as IEnumerable ?? new[] { entity };

            var itemType = typeof(TEntity).IsGenericType && typeof(IEnumerable).IsAssignableFrom(typeof(TEntity))
                ? typeof(TEntity).GetGenericArguments()[0]
                : typeof(TEntity);

            tableName ??= ToolsProp.GetTableName(itemType, _enttityHelper.ReplacesTableName);

            foreach (object entityItem in entities)
            {
                var queryDelete = _enttityHelper.GetQuery.Delete(entityItem, nameId, _enttityHelper.ReplacesTableName, tableName);
                deletesQueriesEntities[entityItem] = new List<QueryCommand?>() { queryDelete };
            }

            long deletions = 0;
            foreach (var deleteQueriesEntity in deletesQueriesEntities)
            {
                if (deleteQueriesEntity.Value == null) throw new Exception("EH-000: Delete query does not exist!");
                deletions += deleteQueriesEntity.Value.Sum(deleteQuery => deleteQuery is null ? throw new Exception($"EH-000: Error delete query!") : Execute.ExecuteNonQuery(_enttityHelper.DbContext, new List<QueryCommand?>() {deleteQuery}, 1).First());
            }

            return deletions;
        }

        public long ExecuteNonQuery(string? query, int expectedChanges)
        {
            return ExecuteNonQuery(new List<string?>() { query }, expectedChanges).FirstOrDefault();
        }

        public ICollection<long> ExecuteNonQuery(ICollection<string?> queries, int expectedChanges)
        {
            return Execute.ExecuteNonQuery(_enttityHelper.DbContext, queries, expectedChanges);
        }
        
        public long ExecuteNonQuery(QueryCommand? query, int expectedChanges)
        {
            return ExecuteNonQuery(new List<QueryCommand?>() { query }, expectedChanges).FirstOrDefault();
        }

        public ICollection<long> ExecuteNonQuery(ICollection<QueryCommand?> queries, int expectedChanges)
        {
            return Execute.ExecuteNonQuery(_enttityHelper.DbContext, queries, expectedChanges);
        }

        public List<TEntity>? ExecuteSelect<TEntity>(string? query, int? pageSize, int pageIndex, string? filterPage, string? sortColumnPage, bool sortAscendingPage) where TEntity : class
        {
            return (List<TEntity>?)Execute.ExecuteReader<TEntity>(_enttityHelper.DbContext, query, false, pageSize, pageIndex, filterPage, sortColumnPage, sortAscendingPage);
        }
        
        public List<TEntity>? ExecuteSelect<TEntity>(QueryCommand? query, int? pageSize, int pageIndex, string? filterPage, string? sortColumnPage, bool sortAscendingPage) where TEntity : class
        {
            return (List<TEntity>?)Execute.ExecuteReader<TEntity>(_enttityHelper.DbContext, query, false, pageSize, pageIndex, filterPage, sortColumnPage, sortAscendingPage);
        }

        public DataTable? ExecuteSelectDt(string? query, int? pageSize, int pageIndex, string? filterPage, string? sortColumnPage, bool sortAscendingPage)
        {
            try
            {
                if (Execute.ExecuteReader<IDataReader>(_enttityHelper.DbContext, query, true, pageSize, pageIndex, filterPage, sortColumnPage, sortAscendingPage) is not IDataReader resultSelect) return null;
                DataTable dtResult = resultSelect.ToDataTable();
                resultSelect.Close();
                _enttityHelper.DbContext.CloseConnection();
                return dtResult;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (_enttityHelper.DbContext?.IDbConnection is not null && _enttityHelper.DbContext.IDbConnection.State == ConnectionState.Open) _enttityHelper.DbContext.IDbConnection.Close();
            }
        }
        
        public DataTable? ExecuteSelectDt(QueryCommand? query, int? pageSize, int pageIndex, string? filterPage, string? sortColumnPage, bool sortAscendingPage)
        {
            try
            {
                if (Execute.ExecuteReader<IDataReader>(_enttityHelper.DbContext, query, true, pageSize, pageIndex, filterPage, sortColumnPage, sortAscendingPage) is not IDataReader resultSelect) return null;
                DataTable dtResult = resultSelect.ToDataTable();
                resultSelect.Close();
                _enttityHelper.DbContext.CloseConnection();
                return dtResult;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (_enttityHelper.DbContext?.IDbConnection is not null && _enttityHelper.DbContext.IDbConnection.State == ConnectionState.Open) _enttityHelper.DbContext.IDbConnection.Close();
            }
        }

        public object? ExecuteScalar(string? query)
        {
            try
            {
                return ExecuteScalar(new List<string?>() { query }).FirstOrDefault();
            }
            catch (Exception)
            {
                throw;
            }
        }
        
        public object? ExecuteScalar(QueryCommand? query)
        {
            try
            {
                return ExecuteScalar(new List<QueryCommand?>() { query }).FirstOrDefault();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public ICollection<object?> ExecuteScalar(ICollection<string?> queries)
        {
            return Execute.ExecuteScalar(_enttityHelper.DbContext, queries);
        }
        
        public ICollection<object?> ExecuteScalar(ICollection<QueryCommand?> queries)
        {
            return Execute.ExecuteScalar(_enttityHelper.DbContext, queries);
        }

        public bool IncludeAll<TEntity>(TEntity entity) where TEntity : class
        {
            // Check if the entity is an IEnumerable and not a string (to avoid treating strings as collections)
            if (entity is IEnumerable<object> entityList && entity is not string) { return IncludeAllRange(entityList); }
            return IncludeAllRange(new List<TEntity> { entity });
        }

        public bool IncludeAllRange<TEntity>(IEnumerable<TEntity>? entities) where TEntity : class
        {
            if (entities?.Any() != true) return false;
            Entities.Inclusions? inclusions = new(_enttityHelper);
            foreach (TEntity entity in entities)
            {
                inclusions.IncludeForeignKeyEntities(entity);
                inclusions.IncludeInverseProperties(entity, _enttityHelper.ReplacesTableName, _enttityHelper, null);
            }
            return true;
        }

        public bool IncludeEntityFK<TEntity>(TEntity entity, string fkName) where TEntity : class
        {
            if (entity == null) return false;
            new Entities.Inclusions(_enttityHelper).IncludeForeignKeyEntities(entity, fkName);
            return true;
        }

        public bool IncludeInverseEntity<TEntity>(TEntity entity, string inversePropertyName) where TEntity : class
        {
            if (entity == null) return false;
            new Entities.Inclusions(_enttityHelper).IncludeInverseProperties(entity, _enttityHelper.ReplacesTableName, _enttityHelper, inversePropertyName);
            return true;
        }

        public string GetTableName<TEntity>() where TEntity : class => ToolsProp.GetTableName<TEntity>(_enttityHelper.ReplacesTableName);

        public string? GetTableNameManyToMany(Type entity1, string namePropCollection)
        {
            PropertyInfo propCollection = entity1.GetProperty(namePropCollection);
            if (propCollection == null) throw new ArgumentNullException(nameof(propCollection), "Property not found!");
            return ToolsProp.GetTableNameManyToMany(entity1, propCollection, _enttityHelper.ReplacesTableName);
        }

        public string? GetPKName<TEntity>(TEntity entity) where TEntity : class => entity.GetPrimaryKey()?.Name;


        public string NormalizeText(string? text, char replaceSpace, bool toLower)
        {
            return Tools.Normalize(text, toLower, replaceSpace);
        }

        public string NormalizeColumnOrTableName(string? name, bool replaceInvalidChars)
        {
            return Tools.NormalizeColumnOrTableName(name, replaceInvalidChars);
        }

        public string GetDatabaseVersion(Database? database)
        {
            var queryDbVersion = new SqlQueryString(database).GetDatabaseVersion();
            return Execute.ExecuteScalar(database, new List<QueryCommand?>() { queryDbVersion }).First()?.ToString() ?? "Unknown Version";
        }


    }
}