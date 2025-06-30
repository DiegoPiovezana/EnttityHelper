using EH.Command;
using EH.Properties;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace EH.Connection
{
    /// <summary>
    /// Allows you to get the main commands to be executed on the database.
    /// </summary>
    public class SqlQueryString
    {
        /// <summary>
        /// The <see cref="EnttityHelper"/> instance used to perform replacements on the table name and to perform additional queries to check whether a specific item is already in the table and then create the query.
        /// </summary>
        public EnttityHelper? EnttityHelper { get; set; } // TODO: Remove EnttityHelper and use Database directly.

        /// <summary>
        /// The <see cref="Database"/> instance used to execute the commands.
        /// </summary>
        public Database Database { get; set; }
        
       /// <summary>
       /// /// The <see cref="Features"/> instance used to define the features of the database.
       /// </summary>
        internal Features Features { get; set; }


        //Enums.DbType? DbType { get; set; }
        //public SqlQueryString() { }

        /// <summary>
        /// Creates a new instance of the <see cref="SqlQueryString"/> class.
        /// </summary>
        /// <param name="enttityHelper">The <see cref="EnttityHelper"/> instance.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public SqlQueryString(EnttityHelper enttityHelper)
        {
            EnttityHelper = enttityHelper ?? throw new ArgumentNullException(nameof(enttityHelper));
            Database = enttityHelper.DbContext ?? throw new ArgumentNullException(nameof(enttityHelper.DbContext));
            Features = new Features(enttityHelper);
        }

        /// <summary>
        /// Creates a new instance of the <see cref="SqlQueryString"/> class.
        /// </summary>
        /// <param name="database">The <see cref="Database"/> instance.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public SqlQueryString(Database database)
        {
            Database = database ?? throw new ArgumentNullException(nameof(database));
        }


        /// <summary>
        /// Gets the insert command.
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to be manipulated.</typeparam>
        /// <param name="entity">Entity to be inserted into the database.</param>  
        /// <param name="dbType">The type of database in which the information will be inserted. Example: Oracle.</param>  
        /// <param name="replacesTableName">(Optional) Terms that can be replaced in table names.</param>
        /// <param name="tableName1">(Optional) Name of the table to which the entity will be inserted. By default, the table informed in the "Table" attribute of the entity class will be considered.</param> 
        /// <param name="ignoreInverseAndCollectionProperties">(Optional) If true, properties that are part of an inverse property will be ignored.</param>
        /// <returns>String command.</returns>
        public ICollection<QueryCommand?> Insert<TEntity>(
            TEntity entity, 
            Enums.DbProvider? dbType = null, 
            Dictionary<string, string>? replacesTableName = null, 
            string? tableName1 = null, 
            bool ignoreInverseAndCollectionProperties = false
            ) where TEntity : class
        {
            //if (dbType == null) throw new ArgumentNullException("The type of database is invalid!");

            List<QueryCommand?> queries = new();

            Dictionary<string, Property>? properties = ToolsProp.GetProperties(entity, false, false);

            Dictionary<string, Property>? filteredProperties = properties
                .Where(p => p.Value.PropertyInfo.IsFkEntity() == false)
                .Where(p => p.Value.IsCollection == false)
                .Where(p => p.Value.Value is not null)
                .ToDictionary(p => p.Key, p => p.Value);
            
            if (filteredProperties is null || filteredProperties.Count == 0)
                throw new InvalidOperationException("No valid properties found for insert.");
            
            // string columns = string.Join(", ", filteredProperties.Keys);
            // string values = string.Join("', '", filteredProperties.Values);

            string columns = string.Join(", ", filteredProperties.Keys);
            string parametersSql = string.Join(", ", filteredProperties.Keys.Select(k => Database.PrefixParameter + k));

            // if (EnttityHelper is null) { throw new ArgumentNullException(nameof(EnttityHelper)); }

            dbType ??= Database.Provider;
            
            replacesTableName ??= EnttityHelper?.ReplacesTableName;
            tableName1 ??= ToolsProp.GetTableName<TEntity>(replacesTableName);
            string tableName1Escaped = EscapeIdentifier(tableName1);
            
            var pk = ToolsProp.GetPK(entity);

            var queryCommand = dbType switch
            {
                Enums.DbProvider.Oracle => new QueryCommand(
                    sql: $"INSERT INTO {tableName1Escaped} ({columns}) VALUES ({parametersSql}) RETURNING {pk.Name} INTO :Result",
                    entity: entity,
                    parameters: filteredProperties,
                    parametersOutput: new Dictionary<string, Property> { { "Result", new Property(pk, entity) } }
                ),
                Enums.DbProvider.SqlServer => new QueryCommand
                (
                    sql: $"INSERT INTO {tableName1Escaped} ({columns}) OUTPUT INSERTED.{pk.Name} VALUES ({parametersSql})",
                    entity: entity,
                    parameters: filteredProperties,
                    parametersOutput: new Dictionary<string, Property> { { "Result", new Property(pk, entity) } }
                ),
                Enums.DbProvider.SqLite => new QueryCommand
                (
                    sql: $"INSERT INTO {tableName1Escaped} ({columns}) VALUES ({parametersSql}) RETURNING {pk.Name}",
                    entity: entity,
                    parameters: filteredProperties,
                    parametersOutput: new Dictionary<string, Property> { { "Result", new Property(pk, entity) } }
                ),
                _ => throw new NotSupportedException($"Database type '{dbType}' not yet supported.")
            };
            
            queries.Add(queryCommand);

            if (!ignoreInverseAndCollectionProperties)
            {
                InsertInverseProperties(entity, replacesTableName, queries, properties);
                InsertCollectionProperties(entity, dbType, replacesTableName, queries, properties);
            }
            return queries;
        }

        private void InsertInverseProperties<TEntity>(
            TEntity entity, 
            Dictionary<string, string>? replacesTableName, 
            List<QueryCommand?> queries, 
            Dictionary<string, Property> properties
            )
        {
            Dictionary<string, Property>? inverseProperties = properties
                .Where(p => p.Value.InverseProperty != null)
                .ToDictionary(p => p.Key, p => p.Value);
            
            foreach (var invProp in inverseProperties)
            {
                if (invProp.Value.IsCollection != true) { throw new InvalidOperationException("The InverseProperty property must be a collection."); }
                
                Type collectionType = invProp.Value.PropertyInfo.PropertyType;
                Type entity2Type = collectionType.GetGenericArguments()[0];
                string tableNameInverseProperty = ToolsProp.GetTableNameManyToMany(entity.GetType(), invProp.Value.PropertyInfo, replacesTableName);
                string tableNameInversePropertyEscaped = EscapeIdentifier(tableNameInverseProperty);
                
                string tableName1 = ToolsProp.GetTableName(entity.GetType(), replacesTableName, false);
                string tableName2 = ToolsProp.GetTableName(entity2Type, replacesTableName, false);

                string idName1 = ToolsProp.GetPK((object)entity).Name; // Ex: User
                string idName2 = ToolsProp.GetPK((object)entity2Type).Name;  // Ex: Group

                var itemsCollection = (IEnumerable<object>)invProp.Value.Value;
                if (itemsCollection is null) { continue; } // If the collection is null, there is no need to insert anything.

                string idTb1 = tableName1.Substring(0, Math.Min(tableName1.Length, 27));
                string idTb2 = tableName2.Substring(0, Math.Min(tableName2.Length, 27));
                
                PropertyInfo prop1 = entity.GetType().GetProperty(idName1);
                //object idValue1 = prop1.GetValue(entity);
                // string idValue1 = "&ID1"; -> :ID1 (oracle) @ID1 (sqlserver)

                foreach (var item in itemsCollection)
                {
                    if (item != null)
                    {
                        PropertyInfo prop2 = item.GetType().GetProperty(idName2);

                        if (prop2 != null)
                        {
                            object idValue2 = prop2.GetValue(item);
                            // queries.Add($@"INSERT INTO {tableNameInverseProperty} (ID_{idTb1}, ID_{idTb2}) VALUES ('{idValue1}', '{idValue2}')");
                            string sql = $@"INSERT INTO {tableNameInversePropertyEscaped } (ID_{idTb1}, ID_{idTb2}) VALUES ({Database.PrefixParameter}ID1, {Database.PrefixParameter}ID_{idTb2})";
                            
                            Dictionary<string, Property> parameters = new()
                            {
                                { $"ID1", new Property(prop1, entity)},
                                { "ID_" + idTb2, new Property(prop2, item)}
                            };

                            queries.Add(new QueryCommand
                            (
                                sql: sql,
                                entity: entity,
                                parameters: parameters
                            ));
                        }
                    }
                }
            }
        }

        private void InsertCollectionProperties<TEntity>(
            TEntity entity, 
            Enums.DbProvider? dbType,
            Dictionary<string, string>? replacesTableName,
            List<QueryCommand?> queries, 
            Dictionary<string, Property> properties
            )
        {
            Dictionary<string, Property>? collectionProperties = properties
                .Where(p => p.Value.IsCollection == true)
                .Where(p => p.Value.InverseProperty == null)
                .ToDictionary(p => p.Key, p => p.Value);
            
            foreach (var collectionProp in collectionProperties)
            {
                Type collectionType = collectionProp.Value.PropertyInfo.PropertyType;
                Type entity2Type = collectionType.GetGenericArguments()[0]; // Ex: Item
                
                // Define the foreign key property name based on the collection property key.
                string? nameIdFk1 = ToolsProp.GetNameIdFk1(entity2Type, entity.GetType().Name);
                
                // Get item from collection
                if (collectionProp.Value.Value is not IEnumerable<object> itemsCollection)
                    continue;
                
                foreach (var item in itemsCollection)
                {
                    if (string.IsNullOrEmpty(nameIdFk1))
                    {
                        throw new InvalidOperationException("The collection property must have a foreign key property.");;
                    }
                    
                    var insertMethod = this.GetType()
                        .GetMethod("Insert", BindingFlags.Instance | BindingFlags.Public)
                        .MakeGenericMethod(entity2Type);

                    var result = insertMethod.Invoke(this, new object?[]
                    {
                        item,
                        dbType,
                        replacesTableName,
                        null,            // tableName1
                        true             // ignoreInverseAndCollectionProperties
                    }) as ICollection<QueryCommand?>;
                    
                    if (result != null)
                        foreach (var queryCommand in result)
                        {
                            //string nameIdFk1 = "OrderId";
                            Property valueId1 = queryCommand.Parameters[nameIdFk1];
                            queryCommand.Parameters.Remove(nameIdFk1);
                            queryCommand.Parameters["ID1"] = valueId1;

                            queryCommand.Sql = queryCommand.Sql.Replace(
                                Database.PrefixParameter + nameIdFk1,
                                Database.PrefixParameter + "ID1"
                            );
                            
                            queries.Add(queryCommand);
                        }
                }
            }
        }

        /// <summary>
        /// Gets the update command.
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to be updated.</typeparam>
        /// <param name="entity">Entity to be updated in the database.</param>
        /// <param name="enttityHelper">(Optional) Used to perform replacements on the table name and to perform additional queries to check whether a specific item is already in the table and then create the query.</param>
        /// <param name="nameId">(Optional) Name of the column in which the entity will be identified to be updated.</param>
        /// <param name="tableName">(Optional) Name of the table to which the entity will be inserted. By default, the table informed in the "Table" attribute of the entity class will be considered.</param> 
        /// <param name="ignoreInversePropertyProperties">(Optional) If true, properties that are part of an inverse property will be ignored.</param>
        /// <returns>String command.</returns>
        public ICollection<QueryCommand?> Update<TEntity>(TEntity entity, EnttityHelper? enttityHelper = null, string? nameId = null, string? tableName = null, bool ignoreInversePropertyProperties = false) where TEntity : class
        {
            if (enttityHelper is null) { throw new ArgumentNullException(nameof(enttityHelper)); }

            List<QueryCommand?> queries = new();

            tableName ??= ToolsProp.GetTableName<TEntity>(enttityHelper.ReplacesTableName);
            string tableNameEscaped = EscapeIdentifier(tableName);
            
            nameId ??= ToolsProp.GetPK(entity)?.Name ?? throw new InvalidOperationException("No primary key found!");

            Dictionary<string, Property> properties = ToolsProp.GetProperties(entity, true, false);
            
            StringBuilder queryBuilder = new();
            queryBuilder.Append($@"UPDATE {tableNameEscaped } SET ");
            
            //Dictionary<string, Property> parameters = new();
            
            foreach (var pair in properties)
            {
                //parameters.Add(pair);
                
                if (pair.Key == nameId) continue;
              
                // queryBuilder.Append($@"{pair.Key} = '{pair.Value.ValueText}', ");
                queryBuilder.Append($"{pair.Key} = {Database.PrefixParameter}{pair.Key}, ");
            }

            queryBuilder.Length -= 2; // Remove the last comma and space
            // queryBuilder.Append($@" WHERE {nameId} = '{properties[nameId]}'");
            queryBuilder.Append($@" WHERE {nameId} = {Database.PrefixParameter}{nameId}");
            //return queryBuilder.ToString();
            // queries.Add(queryBuilder.ToString());
         
            var cmd = new QueryCommand(queryBuilder.ToString(), entity, properties);
            queries.Add(cmd);

            if (!ignoreInversePropertyProperties) queries.AddRange(UpdateInverseProperty(entity, enttityHelper));
            return queries;
        }

        private ICollection<QueryCommand?> UpdateInverseProperty<TEntity>(TEntity entity, EnttityHelper enttityHelper) where TEntity : class
        {
            List<QueryCommand?> queries = new();

            var inverseProperties = ToolsProp.GetInverseProperties(entity);

            foreach (PropertyInfo invProp in inverseProperties)
            {
                Type collectionType = invProp.PropertyType;
                Type entity2Type = collectionType.GetGenericArguments()[0];
                string tableNameInverseProperty = ToolsProp.GetTableNameManyToMany(entity.GetType(), invProp, enttityHelper.ReplacesTableName);
                string tableNameInversePropertyEscaped = EscapeIdentifier(tableNameInverseProperty);

                var tableName1 = ToolsProp.GetTableName(entity.GetType(), enttityHelper.ReplacesTableName); // Ex.: TB_USER
                var tableName2 = ToolsProp.GetTableName(entity2Type, enttityHelper.ReplacesTableName);  // Ex.: TB_GROUP_USERS

                // MethodInfo checkIfExistMethod = typeof(EnttityHelper).GetMethod("CheckIfExist", new[] { typeof(string), typeof(int), typeof(string) });
                ValidateTableExist(tableName1);
                ValidateTableExist(tableName2);

                void ValidateTableExist(string tableName)
                {
                    // bool tableExists = (bool)checkIfExistMethod.Invoke(enttityHelper, new object[] { tableName, 0, null });
                    bool tableExists = Features.CheckIfExist(tableName, 0, null);
                    if (!tableExists) { throw new Exception($"Table '{tableName}' doesn't exist!"); }
                }

                string idTableName1 = tableName1.Split('.').Last();
                string idTableName2 = tableName2.Split('.').Last();

                var pk1 = ToolsProp.GetPK((object)entity);
                var pk2 = ToolsProp.GetPK((object)entity2Type);
                string idName1 = pk1.Name; // Ex: IdUser
                string idName2 = ToolsProp.GetPK((object)entity2Type).Name;  // Ex: IdGroup

                // MethodInfo selectMethod = typeof(EnttityHelper).GetMethod("ExecuteSelectDt");
                
                // var itemsBd = (DataTable)selectMethod.Invoke(enttityHelper,
                //     new object[]
                //     {
                //         $@"SELECT ID_{tableName2} FROM {tableNameInverseProperty} WHERE ID_{tableName1} = '{pk1.GetValue(entity)}'",
                //         null, 0, null, null, true
                //     });
                
                var parametersSelect = new Dictionary<string, Property>
                {
                    { $"ID_{idTableName1}", new Property(pk1, entity) }
                };

                QueryCommand queryCommandSelect = new QueryCommand(
                    // $@"SELECT ID_{tableName2} FROM {tableNameInversePropertyEscaped} WHERE ID_{tableName1} = '{pk1.GetValue(entity)}'",
                    $@"SELECT ID_{idTableName2} FROM {tableNameInversePropertyEscaped} WHERE ID_{idTableName1} = {enttityHelper.DbContext.PrefixParameter}ID_{idTableName1}",
                    parametersSelect,
                    null
                    );

                var itemsBd = Features.ExecuteSelectDt(queryCommandSelect, null, 0, null, null, true);

                var getMethod = typeof(EnttityHelper).GetMethod("Get").MakeGenericMethod(entity2Type);
                // var itemsCollectionBd = new List<dynamic>();
                List<object>? itemsCollectionBd = new(itemsBd.Rows.Count);

                foreach (DataRow row in itemsBd.Rows)
                {
                    object idItemEntity2 = row[0];
                    // var entity2InCollectionBd = Features.Get<object>(false, $"{pk2.Name} = '{idItemEntity2}'", null, null, 0, null, true);
                    var entity2InCollectionBd = (IEnumerable)getMethod.Invoke(enttityHelper, new object[] { false, $"{pk2.Name} = '{idItemEntity2}'", null, null, 0, null, true });
                    foreach (var entity2 in entity2InCollectionBd) { itemsCollectionBd.Add(entity2); }
                }

                //IEnumerable<object>? itemsCollectionNew = invProp.GetValue(entity) as IEnumerable<object>;
                //IEnumerable<object>? itemsCollectionOld = itemsCollectionBd;

                // List<string>? itemsCollectionNew = new();
                // List<string>? itemsCollectionOld = new();
                
                List<Property>? itemsCollectionNew = new();
                List<Property>? itemsCollectionOld = new();

                foreach (var itemNew in invProp.GetValue(entity) as IEnumerable<object>)
                {
                    if (itemNew is null) continue;
                    PropertyInfo prop2 = itemNew.GetType().GetProperty(idName2);
                    // if (prop2 != null) { itemsCollectionNew.Add(prop2.GetValue(itemNew).ToString()); }
                    if (prop2 != null) { itemsCollectionNew.Add(new Property(prop2, itemNew)); }
                }

                foreach (var itemOld in itemsCollectionBd)
                {
                    if (itemOld is null) continue;
                    PropertyInfo prop2 = itemOld.GetType().GetProperty(idName2);
                    // if (prop2 != null) { itemsCollectionOld.Add(prop2.GetValue(itemOld).ToString()); }
                    if (prop2 != null) { itemsCollectionOld.Add(new Property(prop2, itemOld)); }
                }

                PropertyInfo prop1 = entity.GetType().GetProperty(idName1);
                string idTb1 = idTableName1.Substring(0, Math.Min(idTableName1.Length, 27));
                string idTb2 = idTableName2.Substring(0, Math.Min(idTableName2.Length, 27));
                // object idValue1 = prop1.GetValue(entity);
                
                var itemProp1 = new Property(prop1, entity);

                if (itemsCollectionNew != null && itemsCollectionNew.Any())
                {
                    foreach (Property itemInsert in itemsCollectionNew)
                    {
                        bool itemContains = itemsCollectionNew.Any(p =>
                            p.Name == itemInsert.Name
                            && object.Equals(p.Value, itemInsert.Value)
                        );
                        
                        // if (!itemsCollectionOld.Contains(itemInsert))
                        if (!itemContains)
                        {
                            // queries.Add($@"INSERT INTO {tableNameInverseProperty} (ID_{idTb1}, ID_{idTb2}) VALUES ('{idValue1}', '{itemInsert}')");

                            string sql = $@"INSERT INTO {tableNameInversePropertyEscaped} (ID_{idTb1}, ID_{idTb2}) VALUES ({Database.PrefixParameter}ID_{idTb1}, {Database.PrefixParameter}ID_{idTb2})";

                            Dictionary<string, Property> parametersInsert = new()
                            {
                                {$"ID_" + idTb1, itemProp1},
                                {$"ID_" + idTb2, itemInsert}
                            };

                            QueryCommand queryCommandInsert = new(sql, null, parametersInsert);
                            queries.Add(queryCommandInsert);
                        }
                    }
                }

                if (itemsCollectionOld != null && itemsCollectionOld.Any())
                {
                    foreach (Property itemDelete in itemsCollectionOld)
                    {
                        bool itemContains = itemsCollectionNew.Any(p =>
                            p.Name == itemDelete.Name
                            && object.Equals(p.Value, itemDelete.Value)
                        );
                        
                        // if (!itemsCollectionNew.Contains(itemDelete))
                        if (!itemContains)
                        {
                            // queries.Add($@"DELETE FROM {tableNameInverseProperty} WHERE ID_{idTb1} = '{idValue1}' AND ID_{idTb2} = '{itemDelete}'");
                            string sql =$"DELETE FROM {tableNameInversePropertyEscaped} WHERE ID_{idTb1} = {Database.PrefixParameter}ID_{idTb1} AND ID_{idTb2} = {Database.PrefixParameter}ID_{idTb2}";
                            
                            Dictionary<string, Property> parametersDelete = new()
                            {
                                {$"{Database.PrefixParameter}ID_" + idTb1, itemProp1},
                                {$"{Database.PrefixParameter}ID_" + idTb2, itemDelete}
                            };
                            
                            QueryCommand queryCommandDelete = new(sql, null, parametersDelete);
                            queries.Add(queryCommandDelete);
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
        public QueryCommand? Get<TEntity>(string? filter = null, Dictionary<string, string>? replacesTableName = null, string? tableName = null)
        {
            filter = string.IsNullOrEmpty(filter?.Trim()) ? "1 = 1" : filter;
            tableName ??= ToolsProp.GetTableName<TEntity>(replacesTableName);
            string tableNameEscaped = EscapeIdentifier(tableName);
            // return $@"SELECT * FROM {tableName} WHERE ({filter})";
            return new QueryCommand($@"SELECT * FROM {tableNameEscaped} WHERE ({filter})", null, null);
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
        public QueryCommand? Search<TEntity>(TEntity entity, string? idPropName = null, Dictionary<string, string>? replacesTableName = null, string? tableName = null) where TEntity : class
        {
            idPropName ??= ToolsProp.GetPK(entity)?.Name;
            if (idPropName is null) { return null; }
            tableName ??= ToolsProp.GetTableName<TEntity>(replacesTableName);
            string tableNameEscaped = EscapeIdentifier(tableName);
            // return $@"SELECT * FROM {tableName} WHERE ({idPropName} = '{typeof(TEntity).GetProperty(idPropName).GetValue(entity, null)}')";
            
           Dictionary<string, Property> parameters = new()
            {
                {idPropName, new Property(typeof(TEntity).GetProperty(idPropName), entity)}
            };
            
            return new QueryCommand($@"SELECT * FROM {tableNameEscaped} WHERE ({idPropName} = {Database.PrefixParameter}{idPropName})", entity, parameters);
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
        public QueryCommand? Delete<TEntity>(TEntity entity, string? idPropName = null, Dictionary<string, string>? replacesTableName = null, string? tableName = null) where TEntity : class
        {
            idPropName ??= ToolsProp.GetPK(entity)?.Name;

            if (idPropName is null)
            {
                Debug.WriteLine("No primary key found!");
                return null;
            }

            tableName ??= ToolsProp.GetTableName<TEntity>(replacesTableName);
            string tableNameEscaped = EscapeIdentifier(tableName);

            // TODO: typeof(TEntity) vs entity.GetType()

            // return $@"DELETE FROM {tableName} WHERE ({idPropName} = '{entity.GetType().GetProperty(idPropName).GetValue(entity, null)}')";
            
           Dictionary<string, Property> parameters = new()
            {
                { idPropName, new Property(entity.GetType().GetProperty(idPropName), entity) }
            };
            
            
            return new QueryCommand($@"DELETE FROM {tableNameEscaped} WHERE ({idPropName} = {Database.PrefixParameter}{idPropName})", entity, parameters);
        }

        /// <summary>
        /// Generates a SQL query to check if an entity exists in the database based on its primary key property.
        /// </summary>
        /// <typeparam name="TEntity">The type of entity.</typeparam>
        /// <param name="entity">The entity object.</param>
        /// <param name="idPropName">(Optional) The name of the ID property.</param>
        /// <param name="replacesTableName">(Optional) Terms that can be replaced in table names.</param>
        /// <param name="tableName">(Optional) Name of the table to which the entity will be inserted. By default, the table informed in the "Table" attribute of the entity class will be considered.</param>
        /// <returns>A SQL query string.</returns>
        public QueryCommand? CheckIfExist<TEntity>(TEntity entity, string? idPropName = null, Dictionary<string, string>? replacesTableName = null, string? tableName = null) where TEntity : class
        {
            idPropName ??= ToolsProp.GetPK(entity)?.Name;

            if (idPropName is null)
            {
                Debug.WriteLine("No primary key found!");
                return null;
            }

            tableName ??= ToolsProp.GetTableName<TEntity>(replacesTableName);
            string tableNameScaped = EscapeIdentifier(tableName);
            // var idPropValue = entity.GetType().GetProperty(idPropName).GetValue(entity, null);
            PropertyInfo? idProp = entity.GetType().GetProperty(idPropName);

            // string whereClause = $"{idPropName} = '{idPropValue}'";
            string whereClause = $"{idPropName} = {Database.PrefixParameter}{idPropName}";

            string sql = Database.Provider switch
            {
                Enums.DbProvider.Oracle => $"SELECT 1 FROM {tableNameScaped} WHERE {whereClause} AND ROWNUM = 1",
                Enums.DbProvider.SqlServer => $"SELECT TOP 1 1 FROM {tableNameScaped} WHERE {whereClause}",
                _ => $"SELECT 1 FROM {tableNameScaped} WHERE {whereClause} LIMIT 1",
            };
            
            Dictionary<string, Property> parameters = new Dictionary<string, Property>()
            {
                [idPropName] = new Property(idProp, entity)
            };
            
           return new(sql, entity, parameters);
        }

        /// <summary>
        /// Generates an SQL query to check if a record exists in the specified table based on the given filter condition.
        /// </summary>
        /// <param name="tableName">The name of the database table to query.</param>
        /// <param name="filter">The SQL filter condition to apply when checking for the record. If null, a default condition "1 = 1" is applied.</param>
        /// <returns>A string representing the SQL query for checking the existence of a record in the specified table.</returns>
        public string CheckIfExist(string tableName, string? filter)
        {
            string tableNameScaped = EscapeIdentifier(tableName);
            return $"SELECT COUNT(*) FROM {tableNameScaped} WHERE {filter ?? "1 = 1"}";
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
        public QueryCommand? CountEntity<TEntity>(TEntity entity, string? idPropName = null, Dictionary<string, string>? replacesTableName = null, string? tableName = null) where TEntity : class
        {
            idPropName ??= ToolsProp.GetPK(entity)?.Name;

            if (idPropName is null)
            {
                Debug.WriteLine("No primary key found!");
                return null;
            }

            tableName ??= ToolsProp.GetTableName<TEntity>(replacesTableName);
            string tableNameEscaped = EscapeIdentifier(tableName);

            // var idPropValue = entity.GetType().GetProperty(idPropName).GetValue(entity, null);

            // TODO: typeof(TEntity) vs entity.GetType()

            // return $@"SELECT COUNT(*) FROM {tableName} WHERE ({idPropName} = '{idPropValue}')";
            
            Dictionary<string, Property> parameters = new()
            {
                {idPropName, new Property(entity.GetType().GetProperty(idPropName), entity)}
            };
            
            return new QueryCommand($@"SELECT COUNT(*) FROM {tableNameEscaped} WHERE ({idPropName} = {Database.PrefixParameter}{idPropName})", entity, parameters);
        }

        /// <summary>
        /// Allows you to get the table creation query for TEntity./>.
        /// </summary>
        /// <typeparam name="TEntity">The type of main entity.</typeparam>
        /// <param name="typesSql">Dictionary containing types related to C# code and database data.</param>
        /// <param name="onlyPrimaryTable">(Optional) If true, properties that do not belong to an auxiliary table will be ignored.</param>
        /// <param name="ignoreProps">(Optional) The query to create table will ignore the listed properties.</param>
        /// <param name="replacesTableName">(Optional) Terms that can be replaced in table names.</param>  
        /// <param name="tableName">(Optional) Name of the table to which the entity will be inserted. By default, the table informed in the "Table" attribute of the entity class will be considered.</param> 
        /// <returns>Table creation query. If it is necessary to create an auxiliary table, for an M:N relationship for example, more than one query will be returned.</returns>
        public Dictionary<string, QueryCommand?> CreateTable<TEntity>(Dictionary<string, string>? typesSql, bool onlyPrimaryTable, ICollection<string>? ignoreProps = null, Dictionary<string, string>? replacesTableName = null, string? tableName = null)
        {
            if (typesSql is null) { throw new ArgumentNullException(nameof(typesSql)); }
            ignoreProps ??= new List<string>();

            Dictionary<string, QueryCommand?> queryCreatesTable = new();
            StringBuilder queryBuilderPrincipal = new();
            tableName ??= ToolsProp.GetTableName<TEntity>(replacesTableName);
            string tableNameEscaped = EscapeIdentifier(tableName);

            Type entityType = typeof(TEntity);
            Type itemType = entityType;
            if (typeof(IEnumerable).IsAssignableFrom(entityType) && entityType.IsGenericType) { itemType = entityType.GetGenericArguments()[0]; }

            object entity = Activator.CreateInstance(itemType) ?? throw new ArgumentNullException(nameof(entity));
            var properties = ToolsProp.GetProperties(entity, false, false);
            var pk = ToolsProp.GetPK(entity);

            queryBuilderPrincipal.Append($@"CREATE TABLE {tableNameEscaped} (");

            foreach (KeyValuePair<string, Property> prop in properties)
            {
                if (prop.Value?.Type is null) { throw new InvalidOperationException($"Error mapping entity '{nameof(entity)}'!"); }

                if (ignoreProps.Contains(prop.Value.Name)) { continue; }

                if (prop.Value.IsCollection.HasValue && !prop.Value.IsCollection.Value) // Not IsCollection
                {
                    // if (prop.Value.IsFkEntity && prop.Value.IsFkEntity.Value) { continue; }
                    if (prop.Value.PropertyInfo.IsFkEntity()) { continue; }

                    typesSql.TryGetValue(prop.Value.Type.Name.Trim(), out string value);

                    if (value is null)
                    {
                        Debug.WriteLine($"Type default not found in Dictionary TypesDefault for '{prop.Value.Type.Name}'!");
                        throw new InvalidOperationException($"Type default not found in Dictionary TypesDefault for '{prop.Value.Type.Name}'! Please enter it into the dictionary or consider changing the type.");
                    }

                    // MaxLength?
                    if (prop.Value.MaxLength is not null)
                    {
                        if (prop.Value.MaxLength > 0 && prop.Value.MaxLength < int.MaxValue)
                        {
                            value = Regex.Replace(value, @"\([^()]*\)", "");
                            value += $"({prop.Value.MaxLength})";
                        }
                        else if (value.StartsWith("nvarchar", StringComparison.OrdinalIgnoreCase) || value.StartsWith("varchar", StringComparison.OrdinalIgnoreCase))
                        {
                            value = Regex.Replace(value, @"\([^()]*\)", "");
                            value += $"({GetMaxTextType(Database)})";
                        }
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
                    // string? pkEntity1 = pk.Name;
                    var propInfoPkEntity1 = pk ?? throw new InvalidOperationException("Primary key not found for the first entity!");
                    
                    // var queryCollection = CreateTableFromCollectionProp(entity1Type, propEntity2, propInfoPkEntity1, replacesTableName);
                    var (tableNameManyToMany, queryCollection) = RelationshipValidator.CreateManyToManyTable(entity1Type, propEntity2, Database, replacesTableName);
                    if (queryCollection is not null)
                    {
                        // Merged Dictionary (prioritizing the value of createsTable)
                        // queryCreatesTable =
                        //     queryCollection
                        //         .Concat(queryCreatesTable)
                        //         .GroupBy(pair => pair.Key)
                        //         .ToDictionary(group => group.Key, group => group.Last().Value);
                        
                        queryCreatesTable[tableNameManyToMany] = queryCollection;

                        // var entity2Collection = queryCreatesTable.FirstOrDefault(pair => pair.Value == "???");
                        // if (entity2Collection.Key is not null && entity2Collection.Key != tableName)
                        // {
                        //     //Type collection2Type = propEntity2.PropertyInfo.PropertyType;
                        //     //Type entity2Type = collection2Type.GetGenericArguments()[0];
                        //
                        //     //// Use reflection to call CreateTable<T> with the dynamic type entity2Type
                        //     //var methodCreateTable = typeof(EnttityHelper).GetMethod("CreateTable").MakeGenericMethod(entity2Type);
                        //
                        //     //// Call the method using reflection, passing the required parameters
                        //     //var queryCreateTableEntity2 = (Dictionary<string, string?>?)methodCreateTable.Invoke(this, new object[] { typesSql, true, ignoreProps, replacesTableName, entity2Collection.Key });
                        //     //queryCreatesTable[entity2Collection.Key] = queryCreateTableEntity2[entity2Collection.Key];
                        // }
                    }
                }
            }

            queryBuilderPrincipal.Length -= 2; // Remove the last comma and space
            queryBuilderPrincipal.Append(")");
            //createsTable.Add(queryBuilderPrincipal.ToString());
            
            QueryCommand queryCommand = new QueryCommand(queryBuilderPrincipal.ToString(), entity, null);
            
            // queryCreatesTable[tableName] = queryBuilderPrincipal.ToString();
            queryCreatesTable[tableName] = queryCommand;
            return queryCreatesTable;
        }
        
        private string GetMaxTextType(Database db)
        {
            return db.Provider switch
            {
                Enums.DbProvider.Oracle => "4000",
                Enums.DbProvider.SqlServer => $"max",
                _ => $"4000"
            };
        }

        // internal Dictionary<string, QueryCommand?> CreateTableFromCollectionProp(Type entity1Type, Property? propEntity2, PropertyInfo propInfoPkEntity1, Dictionary<string, string>? replacesTableName)
        // {
        //     Dictionary<string, QueryCommand?> createsTable = new();
        //
        //     string tableEntity1 = ToolsProp.GetTableName(entity1Type, replacesTableName);
        //     string pkEntity1Name = propInfoPkEntity1?.Name ?? throw new InvalidOperationException("Primary key not found for the first entity!");
        //         
        //     Type collection2Type = propEntity2.PropertyInfo.PropertyType;
        //     Type entity2Type = collection2Type.GetGenericArguments()[0];
        //
        //     var propsEntity2 = entity2Type.GetProperties();
        //     var propsPkEntity2 = propsEntity2.Where(prop => Attribute.IsDefined(prop, typeof(System.ComponentModel.DataAnnotations.KeyAttribute)));
        //     var propInfoPkEntity2 = propsPkEntity2?.FirstOrDefault() ?? propsEntity2.FirstOrDefault();
        //     var pkEntity2Name = propInfoPkEntity2?.Name;
        //    
        //     TableAttribute table2Attribute = entity2Type.GetCustomAttribute<TableAttribute>();
        //     string tableEntity2 = (table2Attribute?.Schema != null ? $"{table2Attribute.Schema}.{table2Attribute.Name}" : table2Attribute?.Name) ?? entity2Type.Name;
        //
        //     string tableNameManyToMany = ToolsProp.GetTableNameManyToMany(entity1Type, propEntity2.PropertyInfo, replacesTableName);
        //     var queryCommand = CreateCommandManyToMany(propInfoPkEntity1, tableNameManyToMany, tableEntity1, tableEntity2, propInfoPkEntity2, pkEntity1Name, pkEntity2Name);
        //
        //     // createsTable[tableNameManyToMany] = queryCollection;
        //     createsTable[tableNameManyToMany] = queryCommand;
        //     //createsTable[tableEntity2] = "???";
        //     return createsTable;
        // }
        //
        // private QueryCommand CreateCommandManyToMany(PropertyInfo propInfoPkEntity1, string tableNameManyToMany,
        //     string tableEntity1, string tableEntity2, PropertyInfo? propInfoPkEntity2, string pkEntity1Name,
        //     string? pkEntity2Name)
        // {
        //     string tableNameManyToManyEscaped = EscapeIdentifier(tableNameManyToMany);
        //
        //     string idTb1 = tableEntity1.Substring(0, Math.Min(tableEntity1.Length, 27));
        //     string idTb2 = tableEntity2.Substring(0, Math.Min(tableEntity2.Length, 27));
        //     string idTb1Escaped = EscapeIdentifier(idTb1);
        //     string idTb2Escaped = EscapeIdentifier(idTb2);
        //     idTb1 = idTb1.Contains('.') ? idTb1.Split('.').Last(): idTb1;  // // Ex.: "TEST.TB_USERS" -> "TB_USER"
        //     idTb2 = idTb2.Contains('.') ? idTb2.Split('.').Last(): idTb1;
        //     
        //     string typeSqlPk1 = ToolsProp.GetTypeSql(propInfoPkEntity1.PropertyType, Database);
        //     string typeSqlPk2 = ToolsProp.GetTypeSql(propInfoPkEntity2.PropertyType, Database);
        //
        //     string queryCollection =
        //         $@"CREATE TABLE {tableNameManyToManyEscaped} (" +
        //         $@"ID_{idTb1} {typeSqlPk1}, ID_{idTb2} {typeSqlPk2}, " +
        //         $@"PRIMARY KEY (ID_{idTb1}, ID_{idTb2}), " +
        //         $@"FOREIGN KEY (ID_{idTb1}) REFERENCES {idTb1Escaped}({pkEntity1Name}), " +
        //         $@"FOREIGN KEY (ID_{idTb2}) REFERENCES {idTb2Escaped}({pkEntity2Name}) " +
        //         $")";
        //
        //     QueryCommand queryCommand = new QueryCommand(queryCollection, null);
        //     return queryCommand;
        // }

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
        public QueryCommand? CreateTableFromDataTable(DataTable dataTable, Dictionary<string, string>? typesSql, Dictionary<string, string>? replacesTableName = null, string? tableName = null)
        {
            if (typesSql is null) { throw new ArgumentNullException(nameof(typesSql)); }

            StringBuilder queryBuilder = new();
            tableName ??= Definitions.NameTableFromDataTable(dataTable.TableName, replacesTableName);
            string tableNameEscaped = EscapeIdentifier(tableName);

            queryBuilder.Append($@"CREATE TABLE {tableNameEscaped} (");

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
            // return queryBuilder.ToString();
            
            return new QueryCommand(queryBuilder.ToString(), null, null);
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
        public QueryCommand AddForeignKeyConstraint(string tableName, string childKeyColumn, string parentTableName, string parentKeyColumn, string foreignKeyName)
        {
            string tableNameEscaped = EscapeIdentifier(tableName);
            
            string sql = $@"
                ALTER TABLE {tableNameEscaped}
                ADD CONSTRAINT {foreignKeyName}
                FOREIGN KEY ({childKeyColumn})
                REFERENCES {parentTableName}({parentKeyColumn});";
            
            return new QueryCommand(sql, null, null);
        }
        
        // private static readonly Regex OrderByRegex = new(@"ORDER\s+BY\s+[\w\W]+?$", RegexOptions.IgnoreCase | RegexOptions.Multiline);

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
            if (Database?.Provider is null)
                throw new ArgumentNullException("The database type is required to generate this query!");
            if (string.IsNullOrEmpty(baseQuery))
                throw new ArgumentNullException(nameof(baseQuery));
            if (pageSize <= 0)
                throw new ArgumentException("Page size must be greater than zero!", nameof(pageSize));
            if (pageIndex < 0)
                throw new ArgumentException("Page index cannot be negative!", nameof(pageIndex));

            var offset = pageSize * pageIndex;
            var filterClause = !string.IsNullOrEmpty(filter) ? $"WHERE {filter}" : string.Empty;
            
            // var orderClause = !string.IsNullOrEmpty(sortColumn) ? $"ORDER BY {sortColumn} {(sortAscending ? "ASC" : "DESC")}" : "ORDER BY 1";
            //var orderClause = !string.IsNullOrEmpty(sortColumn) ? $"ORDER BY {sortColumn} {(sortAscending ? "ASC" : "DESC")}" : string.Empty;
            var orderClause = !string.IsNullOrEmpty(sortColumn) ? $"ORDER BY {sortColumn} {(sortAscending ? "ASC" : "DESC")}" : null;
            // var orderClause = $"ORDER BY {(string.IsNullOrWhiteSpace(sortColumn) ? "1" : sortColumn)} {(sortAscending ? "ASC" : "DESC")}";
            
            // var cleanBaseQuery = OrderByRegex.Replace(baseQuery.Trim(), "");
            
            // var lastOrderByIndex = baseQuery.ToUpper().LastIndexOf("ORDER BY");
            // if (lastOrderByIndex > 0)
            // {
            //     baseQuery = baseQuery.Substring(0, lastOrderByIndex).TrimEnd().TrimEnd(';');
            // }

            string sql = Database.Provider switch
            {
                Enums.DbProvider.Oracle => Database.Version.Major switch
                {
                    >= 12 => $@"{baseQuery} {filterClause} {orderClause ?? ""} OFFSET {offset} ROWS FETCH NEXT {pageSize} ROWS ONLY",
                    <= 11 => $@"SELECT /*+ FIRST_ROWS({pageSize}) */ * FROM (SELECT a.*, ROW_NUMBER() OVER ({orderClause ?? "ORDER BY 1"}) AS rnum FROM ({baseQuery} {filterClause}) a) WHERE rnum > {offset} AND rnum <= {offset + pageSize}"
                    //<= 11 => $@"SELECT /*+ FIRST_ROWS({pageSize}) */ * FROM ( SELECT inner_query.*, ROWNUM AS rnum FROM ( {baseQuery} {filterClause} {orderClause} ) inner_query WHERE ROWNUM <= {offset + pageSize} ) WHERE rnum > {offset}",
                    
                    // >= 12 => $@"SELECT * FROM ({baseQuery}) {filterClause} {orderClause} OFFSET {offset} ROWS FETCH NEXT {pageSize} ROWS ONLY",
                    // _   => $@"SELECT * FROM (SELECT a.*, ROWNUM AS rnum FROM ({baseQuery}) a {filterClause} {orderClause}) WHERE rnum > {offset} AND rnum <= {offset + pageSize}"
                },
                Enums.DbProvider.SqlServer or Enums.DbProvider.PostgreSql =>
                    $@"{baseQuery} {filterClause} {orderClause ?? ""} OFFSET {offset} ROWS FETCH NEXT {pageSize} ROWS ONLY",
                    // $@"SELECT * FROM ({baseQuery}) AS paged {filterClause} {orderClause} OFFSET {offset} ROWS FETCH NEXT {pageSize} ROWS ONLY",
                Enums.DbProvider.MySql =>
                    $@"{baseQuery} {filterClause} {orderClause ?? ""} OFFSET {offset} ROWS FETCH NEXT {pageSize} ROWS ONLY",
                    // $@"SELECT * FROM ({baseQuery}) AS paged {filterClause} {orderClause} LIMIT {pageSize} OFFSET {offset}",
                _ => $@"{baseQuery} {filterClause} {orderClause ?? ""} OFFSET {offset} ROWS FETCH NEXT {pageSize} ROWS ONLY"
                // _ => $@"SELECT * FROM ({baseQuery}) AS paged {filterClause} {orderClause} OFFSET {offset} ROWS FETCH NEXT {pageSize} ROWS ONLY",
            };

            return sql;
        }
        
        public QueryCommand PaginatedQuery(QueryCommand baseQuery, int pageSize, int pageIndex, string? filter, string? sortColumn, bool sortAscending)
        {
            if (Database?.Provider is null)
                throw new ArgumentNullException("The database type is required to generate this query!");
            if (string.IsNullOrWhiteSpace(baseQuery.Sql))
                throw new ArgumentNullException(nameof(baseQuery.Sql));
            if (pageSize <= 0)
                throw new ArgumentException("Page size must be greater than zero!", nameof(pageSize));
            if (pageIndex < 0)
                throw new ArgumentException("Page index cannot be negative!", nameof(pageIndex));

            var offset = pageSize * pageIndex;
            var filterClause = !string.IsNullOrWhiteSpace(filter) ? $"WHERE {filter}" : string.Empty;
            var orderClause = !string.IsNullOrWhiteSpace(sortColumn) ? $"ORDER BY {sortColumn} {(sortAscending ? "ASC" : "DESC")}" : null;

            string sql = Database.Provider switch
            {
                Enums.DbProvider.Oracle => Database.Version.Major switch
                {
                    >= 12 => $@"{baseQuery.Sql} {filterClause} {orderClause ?? ""} OFFSET {offset} ROWS FETCH NEXT {pageSize} ROWS ONLY",
                    <= 11 => $@"SELECT /*+ FIRST_ROWS({pageSize}) */ * FROM (
                                    SELECT a.*, ROW_NUMBER() OVER ({orderClause ?? "ORDER BY 1"}) AS rnum
                                    FROM ({baseQuery.Sql} {filterClause}) a
                                )
                                WHERE rnum > {offset} AND rnum <= {offset + pageSize}"
                },
                Enums.DbProvider.SqlServer or Enums.DbProvider.PostgreSql =>
                    $@"{baseQuery.Sql} {filterClause} {orderClause} OFFSET {offset} ROWS FETCH NEXT {pageSize} ROWS ONLY",
                Enums.DbProvider.MySql =>
                    $@"{baseQuery.Sql} {filterClause} {orderClause} LIMIT {pageSize} OFFSET {offset}",
                _ =>
                    $@"{baseQuery.Sql} {filterClause} {orderClause} OFFSET {offset} ROWS FETCH NEXT {pageSize} ROWS ONLY",
            };

            return new QueryCommand(sql, baseQuery.Entity, baseQuery.Parameters);
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
        public QueryCommand CountQuery(string baseQuery, string? filter = null)
        {
            var orderByIndex = baseQuery.ToUpperInvariant().LastIndexOf("ORDER BY");

            var mainQuery = orderByIndex >= 0
                ? baseQuery.Substring(0, orderByIndex).Trim()
                : baseQuery.Trim();

            var filterClause = !string.IsNullOrEmpty(filter)
                ? $"WHERE {filter}"
                : string.Empty;

            // return $"SELECT COUNT(1) FROM ({mainQuery}) CountQuery {filterClause}";
            return new QueryCommand($"SELECT COUNT(1) FROM ({mainQuery}) CountQuery {filterClause}", null, null);
        }

        /// <summary>
        /// Generates a database-specific query to retrieve the version of the connected database.
        /// </summary>
        /// <param name="database">
        /// An optional <see cref="Database"/> object containing the database connection. 
        /// If <paramref name="database"/> is null, the default connection <see cref="Database.IDbConnection"/> is used.
        /// </param>
        /// <returns>
        /// A string containing the SQL query to retrieve the database version.
        /// This query should be executed manually to collect the version information.
        /// </returns>
        /// <exception cref="NotSupportedException">
        /// Thrown when the database type is not recognized or supported.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// Thrown when no valid database connection is provided.
        /// </exception>
        /// <remarks>
        /// This method generates a version query string based on the type of the database connection. 
        /// Supported databases include Oracle, SQL Server, MySQL, PostgreSQL, and SQLite. 
        /// Ensure the <paramref name="database"/> or its connection is properly configured before using this method.
        /// </remarks>
        /// <example>
        /// Example usage for a SQL Server connection:
        /// <code>
        /// var database = new Database(new SqlConnection("Data Source=localhost;Initial Catalog=master;User ID=sa;Password=your_password"));
        /// string query = GetDatabaseVersion(database);
        /// Console.WriteLine($"Generated Query: {query}");
        /// // Execute the query using the database connection.
        /// </code>
        /// </example>
        public QueryCommand GetDatabaseVersion(Database? database = null)
        {
            try
            {
                var connection = (database?.IDbConnection ?? Database.IDbConnection) ?? throw new ArgumentNullException(nameof(database), "Database or its connection cannot be null.");
                string databaseType = connection.GetType().Name.ToLowerInvariant();

                string queryVersion = databaseType switch
                {
                    "oracleconnection" => "SELECT BANNER FROM V$VERSION WHERE ROWNUM = 1",
                    "sqlconnection" => "SELECT @@VERSION",
                    "mysqlconnection" => "SELECT VERSION()",
                    "npgsqlconnection" => "SHOW server_version",
                    "sqliteconnection" => "SELECT sqlite_version()",
                    _ => throw new NotSupportedException($"Unsupported database type: {databaseType}")
                };

                var db = database ?? Database;

                // return queryVersion;
                return new QueryCommand(queryVersion, null, null);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error retrieving the database version.", ex);
            }
        }
       
        /// <summary>
        /// Escapes the provided identifier based on the database provider,
        /// supporting dot-separated schema/table names, but only if necessary.
        /// </summary>
        /// <param name="name">The identifier to be escaped (e.g., "dbo.User").</param>
        /// <returns>The escaped identifier specific to the configured database provider.</returns>
        public string EscapeIdentifier(string name)
        {
            bool NeedsQuoting(string part)
            {
                if (!part.All(c => char.IsLetterOrDigit(c) || c == '_'))
                    return true;
                if (Database.ReservedWords.Contains(part))
                    return true;
                return false;
            }

            string Quote(string part) => Database.Provider switch
            {
                Enums.DbProvider.Oracle     => $"\"{part.Replace("\"", "\"\"")}\"",
                Enums.DbProvider.SqlServer  => $"[{part}]",
                Enums.DbProvider.SqLite     => $"`{part}`",
                Enums.DbProvider.PostgreSql => $"\"{part.Replace("\"", "\"\"")}\"",
                Enums.DbProvider.MySql      => $"`{part}`",
                _                           => part
            };

            var parts = name.Split('.');
            // Determine if table part needs quoting
            var needsQuoteFlags = parts.Select(NeedsQuoting).ToArray();
            bool shouldQuoteAll = parts.Length > 1 && needsQuoteFlags[needsQuoteFlags.Length - 1];

            var escaped = parts.Select((part, index) =>
            {
                bool quote = shouldQuoteAll || needsQuoteFlags[index];
                if (quote)
                {
                    return Quote(part);
                }
                // for Oracle, unquoted identifiers are uppercased
                return (Database.Provider == Enums.DbProvider.Oracle)
                    ? part.ToUpperInvariant()
                    : part;
            });

            return string.Join(".", escaped);
        }



    }
}
