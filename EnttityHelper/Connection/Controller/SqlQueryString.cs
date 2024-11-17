using EH.Command;
using EH.Properties;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace EH.Connection
{
    /// <summary>
    /// Allows you to obtain the main commands to be executed on the database.
    /// </summary>
    public class SqlQueryString
    {
        EnttityHelper? EnttityHelper { get; set; }
        Enums.DbType? DbType { get; set; }


        //public SqlQueryString() { }

        public SqlQueryString(EnttityHelper? enttityHelper)
        {
            EnttityHelper = enttityHelper;
            DbType = enttityHelper?.DbContext.Type;
        }

        public SqlQueryString(Enums.DbType? dbType)
        {
            DbType = dbType;
        }


        /// <summary>
        /// Gets the insert command.
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to be manipulated.</typeparam>
        /// <param name="entity">Entity to be inserted into the database.</param>  
        /// <param name="dbType">The type of database in which the information will be inserted. Example: Oracle.</param>  
        /// <param name="replacesTableName">(Optional) Terms that can be replaced in table names.</param>
        /// <param name="tableName1">(Optional) Name of the table to which the entity will be inserted. By default, the table informed in the "Table" attribute of the entity class will be considered.</param> 
        /// <param name="ignoreInversePropertyProperties">(Optional) If true, properties that are part of an inverse property will be ignored.</param>
        /// <returns>String command.</returns>
        public ICollection<string?> Insert<TEntity>(TEntity entity, Enums.DbType? dbType = null, Dictionary<string, string>? replacesTableName = null, string? tableName1 = null, bool ignoreInversePropertyProperties = false) where TEntity : class
        {
            //if (dbType == null) throw new ArgumentNullException("The type of database is invalid!");

            List<string?> queries = new();

            Dictionary<string, Property>? properties = ToolsProp.GetProperties(entity, false, false);

            Dictionary<string, Property>? filteredProperties = properties.Where(p => p.Value.IsVirtual == false).ToDictionary(p => p.Key, p => p.Value);
            string columns = string.Join(", ", filteredProperties.Keys);
            string values = string.Join("', '", filteredProperties.Values);

            dbType ??= EnttityHelper.DbContext.Type;
            replacesTableName ??= EnttityHelper.ReplacesTableName;
            tableName1 ??= ToolsProp.GetTableName<TEntity>(replacesTableName);
            var pk = ToolsProp.GetPK(entity);

            switch (dbType)
            {
                case Enums.DbType.Oracle:
                    queries.Add($@"INSERT INTO {tableName1} ({columns}) VALUES ('{values}') RETURNING {pk.Name} INTO :Result");
                    break;
                case Enums.DbType.SQLServer:
                    queries.Add($@"INSERT INTO {tableName1} OUTPUT INSERTED.{pk.Name} ({columns}) VALUES ('{values}')");
                    break;
                case Enums.DbType.SQLite:
                    queries.Add($@"INSERT INTO {tableName1} ({columns}) VALUES ('{values}') RETURNING {pk.Name}");
                    break;
                default:
                    throw new NotSupportedException("Database type is not supported!");
            }

            if (!ignoreInversePropertyProperties) InsertInverseProperty(entity, replacesTableName, queries, properties);
            return queries;
        }

        private static void InsertInverseProperty<TEntity>(TEntity entity, Dictionary<string, string>? replacesTableName, List<string?> queries, Dictionary<string, Property> properties)
        {
            Dictionary<string, Property>? inverseProperties = properties.Where(p => p.Value.InverseProperty != null).ToDictionary(p => p.Key, p => p.Value);
            foreach (var invProp in inverseProperties)
            {
                Type collectionType = invProp.Value.PropertyInfo.PropertyType;
                Type entity2Type = collectionType.GetGenericArguments()[0];
                string tableNameInverseProperty = ToolsProp.GetTableNameManyToMany(entity.GetType(), invProp.Value.PropertyInfo, replacesTableName);

                if (invProp.Value.IsCollection != true) { throw new InvalidOperationException("The InverseProperty property must be a collection."); }

                string tableName1 = ToolsProp.GetTableName(entity.GetType(), replacesTableName);
                string tableName2 = ToolsProp.GetTableName(entity2Type, replacesTableName);

                string idName1 = ToolsProp.GetPK((object)entity).Name; // Ex: User
                string idName2 = ToolsProp.GetPK((object)entity2Type).Name;  // Ex: Group

                var itemsCollection = (IEnumerable<object>)invProp.Value.Value;
                if (itemsCollection is null) { continue; } // If the collection is null, there is no need to insert anything.

                string idTb1 = tableName1.Substring(0, Math.Min(tableName1.Length, 27));
                string idTb2 = tableName2.Substring(0, Math.Min(tableName2.Length, 27));
                //PropertyInfo prop1 = entity.GetType().GetProperty(idName1);
                //object idValue1 = prop1.GetValue(entity);
                string idValue1 = "&ID1";

                foreach (var item in itemsCollection)
                {
                    if (item != null)
                    {
                        PropertyInfo prop2 = item.GetType().GetProperty(idName2);

                        if (prop2 != null)
                        {
                            object idValue2 = prop2.GetValue(item);
                            queries.Add($@"INSERT INTO {tableNameInverseProperty} (ID_{idTb1}, ID_{idTb2}) VALUES ('{idValue1}', '{idValue2}')");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets the update command.
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to be manipulated.</typeparam>
        /// <param name="entity">Entity to be updated in the database.</param>
        /// <param name="enttityHelper">(Optional) Used to perform replacements on the table name and to perform additional queries to check whether or not a specific item is already in the table and then create the query.</param>
        /// <param name="nameId">(Optional) Name of the column in which the entity will be identified to be updated.</param>
        /// <param name="tableName">(Optional) Name of the table to which the entity will be inserted. By default, the table informed in the "Table" attribute of the entity class will be considered.</param> 
        /// <param name="ignoreInversePropertyProperties">(Optional) If true, properties that are part of an inverse property will be ignored.</param>
        /// <returns>String command.</returns>
        public ICollection<string?> Update<TEntity>(TEntity entity, EnttityHelper? enttityHelper = null, string? nameId = null, string? tableName = null, bool ignoreInversePropertyProperties = false) where TEntity : class
        {
            if (enttityHelper is null) { throw new ArgumentNullException(nameof(enttityHelper)); }

            List<string?> queries = new();

            StringBuilder queryBuilder = new();
            tableName ??= ToolsProp.GetTableName<TEntity>(enttityHelper.ReplacesTableName);

            queryBuilder.Append($@"UPDATE {tableName} SET ");

            nameId ??= ToolsProp.GetPK(entity)?.Name;

            if (nameId is null)
            {
                throw new InvalidOperationException("No primary key found!");
            }

            var properties = ToolsProp.GetProperties(entity, true, false);

            foreach (KeyValuePair<string, Property> pair in properties)
            {
                if (pair.Key != nameId)
                {
                    queryBuilder.Append($@"{pair.Key} = '{pair.Value.ValueSql}', ");
                }
            }

            queryBuilder.Length -= 2; // Remove the last comma and space
            queryBuilder.Append($@" WHERE {nameId} = '{properties[nameId]}'");
            //return queryBuilder.ToString();
            queries.Add(queryBuilder.ToString());

            if (!ignoreInversePropertyProperties) queries.AddRange(UpdateInverseProperty(entity, enttityHelper));
            return queries;
        }

        private static ICollection<string?> UpdateInverseProperty<TEntity>(TEntity entity, EnttityHelper enttityHelper) where TEntity : class
        {
            List<string?> queries = new();

            var inverseProperties = ToolsProp.GetInverseProperties(entity);

            foreach (PropertyInfo invProp in inverseProperties)
            {
                Type collectionType = invProp.PropertyType;
                Type entity2Type = collectionType.GetGenericArguments()[0];
                string tableNameInverseProperty = ToolsProp.GetTableNameManyToMany(entity.GetType(), invProp, enttityHelper.ReplacesTableName);

                var tableName1 = ToolsProp.GetTableName(entity.GetType(), enttityHelper.ReplacesTableName); // Ex.: TB_USER
                var tableName2 = ToolsProp.GetTableName(entity2Type, enttityHelper.ReplacesTableName);  // Ex.: TB_GROUP_USERS

                MethodInfo checkIfExistMethod = typeof(EnttityHelper).GetMethod("CheckIfExist");
                ValidateTableExist(tableName1);
                ValidateTableExist(tableName2);

                void ValidateTableExist(string tableName)
                {
                    bool tableExists = (bool)checkIfExistMethod.Invoke(enttityHelper, new object[] { tableName, 0, null });
                    if (!tableExists) { throw new Exception($"Table '{tableName}' doesn't exist!"); }
                }

                var pk1 = ToolsProp.GetPK((object)entity);
                var pk2 = ToolsProp.GetPK((object)entity2Type);
                string idName1 = pk1.Name; // Ex: IdUser
                string idName2 = ToolsProp.GetPK((object)entity2Type).Name;  // Ex: IdGroup

                MethodInfo selectMethod = typeof(EnttityHelper).GetMethod("ExecuteSelectDt");
                var itemsBd = (DataTable)selectMethod.Invoke(enttityHelper, new object[] { $@"SELECT ID_{tableName2} FROM {tableNameInverseProperty} WHERE ID_{tableName1}='{pk1.GetValue(entity)}'" });

                var getMethod = typeof(EnttityHelper).GetMethod("Get").MakeGenericMethod(entity2Type);

                List<object>? itemsCollectionBd = new(itemsBd.Rows.Count);
                foreach (DataRow row in itemsBd.Rows)
                {
                    object idItemEntity2 = row[0];
                    var entity2InCollectionBd = (IEnumerable)getMethod.Invoke(enttityHelper, new object[] { false, $"{pk2.Name} = '{idItemEntity2}'", null }); // Error here
                    foreach (var entity2 in entity2InCollectionBd) { itemsCollectionBd.Add(entity2); }
                }

                //IEnumerable<object>? itemsCollectionNew = invProp.GetValue(entity) as IEnumerable<object>;
                //IEnumerable<object>? itemsCollectionOld = itemsCollectionBd;

                List<string>? itemsCollectionNew = new();
                List<string>? itemsCollectionOld = new();

                foreach (var itemNew in invProp.GetValue(entity) as IEnumerable<object>)
                {
                    if (itemNew is null) continue;
                    PropertyInfo prop2 = itemNew.GetType().GetProperty(idName2);
                    if (prop2 != null) { itemsCollectionNew.Add(prop2.GetValue(itemNew).ToString()); }
                }

                foreach (var itemOld in itemsCollectionBd)
                {
                    if (itemOld is null) continue;
                    PropertyInfo prop2 = itemOld.GetType().GetProperty(idName2);
                    if (prop2 != null) { itemsCollectionOld.Add(prop2.GetValue(itemOld).ToString()); }
                }

                PropertyInfo prop1 = entity.GetType().GetProperty(idName1);
                string idTb1 = tableName1.Substring(0, Math.Min(tableName1.Length, 27));
                string idTb2 = tableName2.Substring(0, Math.Min(tableName2.Length, 27));
                object idValue1 = prop1.GetValue(entity);

                if (itemsCollectionNew != null && itemsCollectionNew.Any())
                {
                    foreach (object itemInsert in itemsCollectionNew)
                    {
                        if (!itemsCollectionOld.Contains(itemInsert))
                        {
                            queries.Add($@"INSERT INTO {tableNameInverseProperty} (ID_{idTb1}, ID_{idTb2}) VALUES ('{idValue1}', '{itemInsert}')");
                        }

                    }
                }

                if (itemsCollectionOld != null && itemsCollectionOld.Any())
                {
                    foreach (object itemDelete in itemsCollectionOld)
                    {
                        if (!itemsCollectionNew.Contains(itemDelete))
                        {
                            queries.Add($@"DELETE FROM {tableNameInverseProperty} WHERE ID_{idTb1} = '{idValue1}' AND ID_{idTb2} = '{itemDelete}'");
                        }
                    }
                }
            }

            return queries;
        }

        /// <summary>
        /// Generates a SQL SELECT query for a specified entity with optional filters.
        /// </summary>
        /// <typeparam name="TEntity">The type of entity.</typeparam>     
        /// <param name="filter">(Optional) The filter criteria.</param>
        /// <param name="replacesTableName">(Optional) Terms that can be replaced in table names.</param>
        /// <param name="tableName">(Optional) Name of the table to which the entity will be inserted. By default, the table informed in the "Table" attribute of the entity class will be considered.</param> 
        /// <returns>A SELECT SQL query string.</returns>
        public string? Get<TEntity>(string? filter = null, Dictionary<string, string>? replacesTableName = null, string? tableName = null)
        {
            filter = string.IsNullOrEmpty(filter?.Trim()) ? "1 = 1" : filter;
            tableName ??= ToolsProp.GetTableName<TEntity>(replacesTableName);
            return $@"SELECT * FROM {tableName} WHERE ({filter})";
        }

        /// <summary>
        /// Generates a SQL SELECT query for a specified entity based on the ID property.
        /// </summary>
        /// <typeparam name="TEntity">The type of entity.</typeparam>
        /// <param name="entity">The entity object.</param>    
        /// <param name="idPropName">(Optional) The name of the ID property.</param>
        /// <param name="replacesTableName">(Optional) Terms that can be replaced in table names.</param> 
        /// <param name="tableName">(Optional) Name of the table to which the entity will be inserted. By default, the table informed in the "Table" attribute of the entity class will be considered.</param> 
        /// <returns>A select SQL query string.</returns>
        public string? Search<TEntity>(TEntity entity, string? idPropName = null, Dictionary<string, string>? replacesTableName = null, string? tableName = null) where TEntity : class
        {
            idPropName ??= ToolsProp.GetPK(entity)?.Name;
            if (idPropName is null) { return null; }
            tableName ??= ToolsProp.GetTableName<TEntity>(replacesTableName);
            return $@"SELECT * FROM {tableName} WHERE ({idPropName} = '{typeof(TEntity).GetProperty(idPropName).GetValue(entity, null)}')";
        }

        /// <summary>
        /// Generates a SQL DELETE query for a specified entity based on the ID property.
        /// </summary>
        /// <typeparam name="TEntity">The type of entity.</typeparam>
        /// <param name="entity">The entity object.</param>
        /// <param name="idPropName">(Optional) The name of the ID property.</param>
        /// <param name="replacesTableName">(Optional) Terms that can be replaced in table names.</param>
        /// <param name="tableName">(Optional) Name of the table to which the entity will be inserted. By default, the table informed in the "Table" attribute of the entity class will be considered.</param> 
        /// <returns>A delete SQL query string.</returns>
        public string? Delete<TEntity>(TEntity entity, string? idPropName = null, Dictionary<string, string>? replacesTableName = null, string? tableName = null) where TEntity : class
        {
            idPropName ??= ToolsProp.GetPK(entity)?.Name;

            if (idPropName is null)
            {
                Debug.WriteLine("No primary key found!");
                return null;
            }

            tableName ??= ToolsProp.GetTableName<TEntity>(replacesTableName);

            // TODO: typeof(TEntity) vs entity.GetType()

            return $@"DELETE FROM {tableName} WHERE ({idPropName} = '{entity.GetType().GetProperty(idPropName).GetValue(entity, null)}')";
        }

        /// <summary>
        /// Generates an SQL query to count the occurrences of a specified entity in the database,
        /// using its primary key property as a filter.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity to count.</typeparam>
        /// <param name="entity">The entity instance containing the primary key value to filter by.</param>
        /// <param name="idPropName">
        /// (Optional) The name of the primary key property for filtering. If null, the method will attempt to identify the primary key automatically.
        /// </param>
        /// <param name="replacesTableName">
        /// (Optional) A dictionary mapping class names to table names, allowing custom table name replacements.
        /// </param>
        /// <param name="tableName">
        /// (Optional) The name of the table to query. If null, the method will attempt to identify the table name based on the entity type.
        /// </param>
        /// <returns>
        /// An SQL string for counting occurrences of the specified entity in the table. Returns null if the primary key is not found.
        /// </returns>
        public string? Count<TEntity>(TEntity entity, string? idPropName = null, Dictionary<string, string>? replacesTableName = null, string? tableName = null) where TEntity : class
        {
            idPropName ??= ToolsProp.GetPK(entity)?.Name;

            if (idPropName is null)
            {
                Debug.WriteLine("No primary key found!");
                return null;
            }

            tableName ??= ToolsProp.GetTableName<TEntity>(replacesTableName);

            // TODO: typeof(TEntity) vs entity.GetType()

            return $@"SELECT COUNT(*) FROM {tableName} WHERE ({idPropName} = '{entity.GetType().GetProperty(idPropName).GetValue(entity, null)}')";
        }

        /// <summary>
        /// Allows you to obtain the table creation query for TEntity./>.
        /// </summary>
        /// <typeparam name="TEntity">The type of main entity.</typeparam>
        /// <param name="typesSql">Dictionary containing types related to C# code and database data.</param>
        /// <param name="onlyPrimaryTable">(Optional) If true, properties that do not belong to an auxiliary table will be ignored.</param>
        /// <param name="ignoreProps">(Optional) The query to create table will ignore the listed properties.</param>
        /// <param name="replacesTableName">(Optional) Terms that can be replaced in table names.</param>  
        /// <param name="tableName">(Optional) Name of the table to which the entity will be inserted. By default, the table informed in the "Table" attribute of the entity class will be considered.</param> 
        /// <returns>Table creation query. If it is necessary to create an auxiliary table, for an M:N relationship for example, more than one query will be returned.</returns>
        public Dictionary<string, string?> CreateTable<TEntity>(Dictionary<string, string>? typesSql, bool onlyPrimaryTable, ICollection<string>? ignoreProps = null, Dictionary<string, string>? replacesTableName = null, string? tableName = null)
        {
            if (typesSql is null) { throw new ArgumentNullException(nameof(typesSql)); }
            ignoreProps ??= new List<string>();

            Dictionary<string, string?> queryCreatesTable = new();
            StringBuilder queryBuilderPrincipal = new();
            tableName ??= ToolsProp.GetTableName<TEntity>(replacesTableName);

            Type entityType = typeof(TEntity);
            Type itemType = entityType;
            if (typeof(IEnumerable).IsAssignableFrom(entityType) && entityType.IsGenericType) { itemType = entityType.GetGenericArguments()[0]; }

            object entity = Activator.CreateInstance(itemType) ?? throw new ArgumentNullException(nameof(entity));
            var properties = ToolsProp.GetProperties(entity, false, false);
            var pk = ToolsProp.GetPK(entity);

            queryBuilderPrincipal.Append($@"CREATE TABLE {tableName} (");

            foreach (KeyValuePair<string, Property> prop in properties)
            {
                if (prop.Value?.Type is null) { throw new InvalidOperationException($"Error mapping entity '{nameof(entity)}'!"); }

                if (ignoreProps.Contains(prop.Value.Name)) { continue; }

                if (prop.Value.IsCollection.HasValue && !prop.Value.IsCollection.Value) // Not IsCollection
                {
                    if (prop.Value.IsVirtual.HasValue && prop.Value.IsVirtual.Value) { continue; }

                    typesSql.TryGetValue(prop.Value.Type.Name.Trim(), out string value);

                    if (value is null)
                    {
                        Debug.WriteLine($"Type default not found in Dictionary TypesDefault for '{prop.Value.Type.Name}'!");
                        throw new InvalidOperationException($"Type default not found in Dictionary TypesDefault for '{prop.Value.Type.Name}'! Please enter it into the dictionary or consider changing the type.");
                    }

                    // MaxLength?
                    if (prop.Value.MaxLength > 0)
                    {
                        value = Regex.Replace(value, @"\([^()]*\)", "");
                        value += $"({prop.Value.MaxLength})";
                    }

                    // PK?                    
                    if (prop.Key == pk?.Name || prop.Key == pk?.GetCustomAttribute<ColumnAttribute>()?.Name)
                    {
                        queryBuilderPrincipal.Append($@"{prop.Key} {value} PRIMARY KEY, ");
                    }
                    else
                    {
                        queryBuilderPrincipal.Append($@"{prop.Key} {value}, ");
                    }

                    // MinimumLength?
                    if (prop.Value.MinLength > 0)
                    {
                        queryBuilderPrincipal.Append($@"CHECK(LENGTH({prop.Key}) >= {prop.Value.MinLength}), ");
                    }
                }
                else // IsCollection
                {
                    if (onlyPrimaryTable) { continue; }

                    Type entity1Type = entity.GetType(); // User
                    Property? propEntity2 = properties[prop.Value.Name]; // Group
                    string? pkEntity1 = pk.Name;

                    var queryCollection = CreateTableFromCollectionProp(entity1Type, propEntity2, pkEntity1, replacesTableName);

                    // Merged Dictionary (prioritizing the value of createsTable)
                    queryCreatesTable =
                        queryCollection
                        .Concat(queryCreatesTable)
                        .GroupBy(pair => pair.Key)
                        .ToDictionary(group => group.Key, group => group.Last().Value);

                    var entity2Collection = queryCreatesTable.FirstOrDefault(pair => pair.Value == "???");
                    if (entity2Collection.Key is not null && entity2Collection.Key != tableName)
                    {
                        //Type collection2Type = propEntity2.PropertyInfo.PropertyType;
                        //Type entity2Type = collection2Type.GetGenericArguments()[0];

                        //// Use reflection to call CreateTable<T> with the dynamic type entity2Type
                        //var methodCreateTable = typeof(EnttityHelper).GetMethod("CreateTable").MakeGenericMethod(entity2Type);

                        //// Call the method using reflection, passing the required parameters
                        //var queryCreateTableEntity2 = (Dictionary<string, string?>?)methodCreateTable.Invoke(this, new object[] { typesSql, true, ignoreProps, replacesTableName, entity2Collection.Key });
                        //queryCreatesTable[entity2Collection.Key] = queryCreateTableEntity2[entity2Collection.Key];
                    }
                }
            }

            queryBuilderPrincipal.Length -= 2; // Remove the last comma and space
            queryBuilderPrincipal.Append(")");
            //createsTable.Add(queryBuilderPrincipal.ToString());
            queryCreatesTable[tableName] = queryBuilderPrincipal.ToString();
            return queryCreatesTable;
        }

        internal static Dictionary<string, string?> CreateTableFromCollectionProp(Type entity1Type, Property? propEntity2, string? pkEntity1, Dictionary<string, string>? replacesTableName)
        {
            Dictionary<string, string?> createsTable = new();

            string tableEntity1 = ToolsProp.GetTableName(entity1Type, replacesTableName);

            Type collection2Type = propEntity2.PropertyInfo.PropertyType;
            Type entity2Type = collection2Type.GetGenericArguments()[0];

            var propsEntity2 = entity2Type.GetProperties();
            var propPkEntity2 = propsEntity2.Where(prop => Attribute.IsDefined(prop, typeof(System.ComponentModel.DataAnnotations.KeyAttribute)));
            var pkEntity2 = propPkEntity2?.FirstOrDefault()?.Name ?? propsEntity2.FirstOrDefault().Name;

            TableAttribute table2Attribute = entity2Type.GetCustomAttribute<TableAttribute>();
            string tableEntity2 = table2Attribute.Name ?? entity2Type.Name;

            string tableNameManyToMany = ToolsProp.GetTableNameManyToMany(entity1Type, propEntity2.PropertyInfo, replacesTableName);

            string idTb1 = tableEntity1.Substring(0, Math.Min(tableEntity1.Length, 27));
            string idTb2 = tableEntity2.Substring(0, Math.Min(tableEntity2.Length, 27));

            string queryCollection =
                $@"CREATE TABLE {tableNameManyToMany} (" +
                $@"ID_{tableEntity1} INT, ID_{tableEntity2} INT, " +
                $@"PRIMARY KEY (ID_{idTb1}, ID_{idTb2}), " +
                $@"FOREIGN KEY (ID_{idTb1}) REFERENCES {idTb1}({pkEntity1}), " +
                $@"FOREIGN KEY (ID_{idTb2}) REFERENCES {idTb2}({pkEntity2}) " +
                $")";

            createsTable[tableNameManyToMany] = queryCollection;
            //createsTable[tableEntity2] = "???";
            return createsTable;
        }

        /// <summary>
        /// Retrieves the SQL query for creating a table based on the structure of a DataTable.
        /// </summary>
        /// <param name="dataTable">The DataTable object representing the structure of the table.</param>
        /// <param name="typesSql">A dictionary mapping column names to SQL data types.</param>
        /// <param name="replacesTableName">(Optional) A dictionary specifying replacements for table names in the SQL query.</param>
        /// <param name="tableName">(Optional) The name of the table. If not specified, the name from the DataTable object will be used.</param>
        /// <returns>The SQL query string for creating the table.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the dataTable parameter is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown if an error occurs while generating the SQL query.</exception>
        public string? CreateTableFromDataTable(DataTable dataTable, Dictionary<string, string>? typesSql, Dictionary<string, string>? replacesTableName = null, string? tableName = null)
        {
            if (typesSql is null) { throw new ArgumentNullException(nameof(typesSql)); }

            StringBuilder queryBuilder = new();
            tableName ??= Definitions.NameTableFromDataTable(dataTable.TableName, replacesTableName);

            queryBuilder.Append($@"CREATE TABLE {tableName} (");

            var columns = dataTable.Columns;
            foreach (DataColumn column in columns)
            {
                string nameColumn = column.ColumnName;
                //nameColumn = nameColumn.Length > 30 ? nameColumn.Substring(0, 30) : nameColumn;
                nameColumn = nameColumn.NormalizeColumnOrTableName();
                // TODO: Column empty?

                typesSql.TryGetValue(column.DataType.Name.Trim(), out string typeColumn);

                if (typeColumn is null)
                {
                    Debug.WriteLine($"Type default not found in Dictionary TypesDefault for '{column.ColumnName}'!");
                    throw new InvalidOperationException($"Type default not found in Dictionary TypesDefault for '{column.ColumnName}'! Please enter it into the dictionary or consider changing the type.");
                }

                queryBuilder.Append($"{nameColumn} {typeColumn}, ");
            }

            queryBuilder.Length -= 2; // Remove the last comma and space
            queryBuilder.Append(")");
            return queryBuilder.ToString();
        }

        /// <summary>
        /// Creates a SQL query to add a foreign key constraint to a table.
        /// </summary>
        /// <param name="tableName">The name of the child table where the foreign key will be added.</param>
        /// <param name="childKeyColumn">The name of the column in the child table that will be the foreign key.</param>
        /// <param name="parentTableName">The name of the parent table containing the referenced primary key column.</param>
        /// <param name="parentKeyColumn">The name of the column in the parent table that is the primary key being referenced.</param>
        /// <param name="foreignKeyName">The name for the foreign key constraint.</param>
        /// <returns>A SQL query to add the foreign key constraint.</returns>
        public string AddForeignKeyConstraint(string tableName, string childKeyColumn, string parentTableName, string parentKeyColumn, string foreignKeyName)
        {
            return $@"
                ALTER TABLE {tableName}
                ADD CONSTRAINT {foreignKeyName}
                FOREIGN KEY ({childKeyColumn})
                REFERENCES {parentTableName}({parentKeyColumn});";
        }

        /// <summary>
        /// Builds a paginated SQL query by applying optional filtering, sorting, and pagination parameters to a base query.
        /// </summary>
        /// <param name="baseQuery">The base SQL query to which pagination, filtering, and sorting will be applied.</param>
        /// <param name="pageSize">The number of records to retrieve per page. Must be a positive integer.</param>
        /// <param name="pageIndex">The zero-based index of the page to retrieve. Must be a non-negative integer.</param>
        /// <param name="filter">(Optional) A SQL-compatible filter condition to be appended to the query (e.g., "ColumnName = 'Value'").</param>
        /// <param name="sortColumn">(Optional) The name of the column to be used for sorting the query results.</param>
        /// <param name="sortAscending">Determines the sorting direction. Set to true for ascending order; false for descending order. Default is true.</param>
        /// <returns>
        /// A string representing the paginated SQL query with the specified filtering, sorting, and pagination applied.
        /// </returns>
        /// <remarks>
        /// - If <paramref name="filter"/> is null or empty, no filtering clause is added.
        /// - If <paramref name="sortColumn"/> is null or empty, no sorting clause is added.
        /// - Uses the OFFSET-FETCH syntax for pagination, compatible with SQL Server, Oracle (12c+), and other SQL dialects.
        /// - Ensure the <paramref name="baseQuery"/> does not already include conflicting filters or sorting logic unless intended.
        /// </remarks>

        public string PaginatedQuery(string baseQuery, int pageSize, int pageIndex, string? filter, string? sortColumn, bool sortAscending)
        {
            if (DbType is null)
                throw new ArgumentNullException("The database type is required to generate this query!");
            if (string.IsNullOrEmpty(baseQuery))
                throw new ArgumentNullException(nameof(baseQuery));
            if (pageSize <= 0)
                throw new ArgumentException("Page size must be greater than zero!", nameof(pageSize));
            if (pageIndex < 0)
                throw new ArgumentException("Page index cannot be negative!", nameof(pageIndex));

            var offset = pageSize * pageIndex;
            var filterClause = !string.IsNullOrEmpty(filter) ? $"WHERE {filter}" : string.Empty;
            var orderClause = !string.IsNullOrEmpty(sortColumn) ? $"ORDER BY {sortColumn} {(sortAscending ? "ASC" : "DESC")}" : "ORDER BY 1"; // Oracle needs a defined order for ROW_NUMBER

            //return $"{baseQuery} {filterClause} {orderClause} OFFSET {offset} ROWS FETCH NEXT {pageSize} ROWS ONLY";

            return DbType switch
            {
                //Enums.DbType.Oracle => $@"SELECT /*+ FIRST_ROWS({pageSize}) */ * FROM (SELECT a.*, ROW_NUMBER() OVER ({orderClause}) AS rnum FROM ({baseQuery} {filterClause}) a) WHERE rnum > {offset} AND rnum <= {offset + pageSize}",
                Enums.DbType.Oracle => $@"SELECT /*+ FIRST_ROWS({pageSize}) */ * FROM ( SELECT inner_query.*, ROWNUM AS rnum FROM ( {baseQuery} {filterClause} {orderClause} ) inner_query WHERE ROWNUM <= {offset + pageSize} ) WHERE rnum > {offset}",
                Enums.DbType.Oracle12c => $@"{baseQuery} {filterClause} {orderClause} OFFSET {offset} ROWS FETCH NEXT {pageSize} ROWS ONLY",
                Enums.DbType.SQLServer or Enums.DbType.PostgreSQL => $"{baseQuery} {filterClause} {orderClause} OFFSET {offset} ROWS FETCH NEXT {pageSize} ROWS ONLY",
                Enums.DbType.MySQL => $"{baseQuery} {filterClause} {orderClause} LIMIT {pageSize} OFFSET {offset}",
                _ => $"{baseQuery} {filterClause} {orderClause} OFFSET {offset} ROWS FETCH NEXT {pageSize} ROWS ONLY",
            };
        }

        /// <summary>
        /// Builds a SQL query to count the total number of records from a base query, optionally applying a filter condition.
        /// </summary>
        /// <param name="baseQuery">The base SQL query to count records from. Should not include SELECT or ORDER BY clauses.</param>
        /// <param name="filter">(Optional) A SQL-compatible filter condition to limit the records being counted (e.g., "ColumnName = 'Value'").</param>
        /// <returns>
        /// A string representing the SQL query to count the records.
        /// </returns>
        /// <remarks>
        /// - If <paramref name="filter"/> is null or empty, the count query is generated without a WHERE clause.
        /// - Ensure the <paramref name="baseQuery"/> does not conflict with the added filter logic.
        /// - The method assumes the <paramref name="baseQuery"/> is well-formed and valid for counting.
        /// </remarks>
        public string CountQuery(string baseQuery, string? filter)
        {
            var mainQuery = baseQuery.Split(new[] { "ORDER BY" }, StringSplitOptions.RemoveEmptyEntries)[0];
            var filterClause = !string.IsNullOrEmpty(filter) ? $"WHERE {filter}" : string.Empty;
            return $"SELECT COUNT(*) FROM ({mainQuery} {filterClause}) AS TotalCount";
        }


    }
}
