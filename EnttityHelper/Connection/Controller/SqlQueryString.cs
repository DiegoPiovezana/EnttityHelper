using EH.Command;
using EH.Properties;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        /// <summary>
        /// Gets the insert command.
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to be manipulated.</typeparam>
        /// <param name="entity">Entity to be inserted into the database.</param>  
        /// <param name="replacesTableName">(Optional) Terms that can be replaced in table names.</param>
        /// <param name="tableName1">(Optional) Name of the table to which the entity will be inserted. By default, the table informed in the "Table" attribute of the entity class will be considered.</param> 
        /// <param name="ignoreInversePropertyProperties">(Optional) If true, properties that are part of an inverse property will be ignored.</param>
        /// <returns>String command.</returns>
        public ICollection<string?> Insert<TEntity>(TEntity entity, Dictionary<string, string>? replacesTableName = null, string? tableName1 = null, bool ignoreInversePropertyProperties = false)
        {
            List<string?> queries = new();

            Dictionary<string, Property>? properties = ToolsProp.GetProperties(entity, false, false);

            Dictionary<string, Property>? filteredProperties = properties.Where(p => p.Value.IsVirtual == false).ToDictionary(p => p.Key, p => p.Value);
            string columns = string.Join(", ", filteredProperties.Keys);
            string values = string.Join("', '", filteredProperties.Values);
            tableName1 ??= ToolsProp.GetTableName<TEntity>(replacesTableName);
            queries.Add($"INSERT INTO {tableName1} ({columns}) VALUES ('{values}')");

            if (!ignoreInversePropertyProperties) InsertInverseProperty(entity, replacesTableName, tableName1, queries, properties);
            return queries;
        }

        private static void InsertInverseProperty<TEntity>(TEntity entity, Dictionary<string, string>? replacesTableName, string? tableName1, List<string?> queries, Dictionary<string, Property> properties)
        {
            Dictionary<string, Property>? inverseProperties = properties.Where(p => p.Value.InverseProperty != null).ToDictionary(p => p.Key, p => p.Value);
            foreach (var invProp in inverseProperties)
            {
                Type collectionType = invProp.Value.PropertyInfo.PropertyType;
                Type entity2Type = collectionType.GetGenericArguments()[0];
                string tableNameInverseProperty = ToolsProp.GetTableNameManyToMany(tableName1, entity2Type, replacesTableName);

                if (invProp.Value.IsCollection != true) { throw new InvalidOperationException("The InverseProperty property must be a collection."); }

                var tableName2 = ToolsProp.GetTableName(entity2Type, replacesTableName);

                string idName1 = ToolsProp.GetPK((object)entity).Name; // Ex: User
                string idName2 = ToolsProp.GetPK((object)entity2Type).Name;  // Ex: Group

                var itemsCollection = (IEnumerable<object>)invProp.Value.Value;
                if (itemsCollection is null) { continue; } // If the collection is null, there is no need to insert anything.

                foreach (var item in itemsCollection)
                {
                    PropertyInfo prop1 = entity.GetType().GetProperty(idName1);
                    PropertyInfo prop2 = item.GetType().GetProperty(idName2);

                    if (prop2 != null)
                    {
                        string idTb1 = tableName1.Substring(0, Math.Min(tableName1.Length, 27));
                        string idTb2 = tableName2.Substring(0, Math.Min(tableName2.Length, 27));

                        object idValue1 = prop1.GetValue(entity);
                        object idValue2 = prop2.GetValue(item);

                        //queries.Add($"INSERT INTO {tableNameInverseProperty} (ID_{idName1}1, ID_{idName2}2) VALUES ('{idValue1}', '{idValue2}')"); // ID_{pkEntity1}1 INT, ID_{pkEntity2}2 INT
                        queries.Add($"INSERT INTO {tableNameInverseProperty} (ID_{idTb1}, ID_{idTb2}) VALUES ('{idValue1}', '{idValue2}')");
                    }
                }
            }
        }

        /// <summary>
        /// Gets the update command.
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to be manipulated.</typeparam>
        /// <param name="entity">Entity to be updated in the database.</param>
        /// <param name="nameId">(Optional) Entity Id column name.</param>
        /// <param name="replacesTableName">(Optional) Terms that can be replaced in table names.</param>
        /// <param name="tableName">(Optional) Name of the table to which the entity will be inserted. By default, the table informed in the "Table" attribute of the entity class will be considered.</param> 
        /// <param name="ignoreInversePropertyProperties">(Optional) If true, properties that are part of an inverse property will be ignored.</param>
        /// <returns>String command.</returns>
        public ICollection<string?> Update<TEntity>(TEntity entity, string? nameId = null, Dictionary<string, string>? replacesTableName = null, string? tableName = null, bool ignoreInversePropertyProperties = false) where TEntity : class
        {
            List<string?> queries = new();

            StringBuilder queryBuilder = new();
            tableName ??= ToolsProp.GetTableName<TEntity>(replacesTableName);

            queryBuilder.Append($"UPDATE {tableName} SET ");

            nameId ??= ToolsProp.GetPK(entity)?.Name;

            if (nameId is null)
            {
                Debug.WriteLine("No primary key (or equivalent) found!");
                return null;
            }

            var properties = ToolsProp.GetProperties(entity);

            foreach (KeyValuePair<string, Property> pair in properties)
            {
                if (pair.Key != nameId)
                {
                    queryBuilder.Append($"{pair.Key} = '{pair.Value.ValueSql}', ");
                }
            }

            queryBuilder.Length -= 2; // Remove the last comma and space
            queryBuilder.Append($" WHERE {nameId} = '{properties[nameId]}'");
            //return queryBuilder.ToString();
            queries.Add(queryBuilder.ToString());

            if (!ignoreInversePropertyProperties) UpdateInverseProperty(entity, replacesTableName, tableName, queries, properties);
            return queries;
        }

        private static void UpdateInverseProperty<TEntity>(TEntity entity, Dictionary<string, string>? replacesTableName, string? tableName, List<string?> queries, Dictionary<string, Property> properties) where TEntity : class
        {
            Dictionary<string, Property>? inverseProperties = properties.Where(p => p.Value.InverseProperty != null)
                            .ToDictionary(p => p.Key, p => p.Value);

            foreach (var invProp in inverseProperties)
            {
                Type collectionType = invProp.Value.PropertyInfo.PropertyType;
                Type entity2Type = collectionType.GetGenericArguments()[0];
                string tableNameInverseProperty = ToolsProp.GetTableNameManyToMany(tableName, entity2Type, replacesTableName);

                if (invProp.Value.IsCollection != true) { throw new InvalidOperationException("The InverseProperty property must be a collection."); }

                var tableName2 = ToolsProp.GetTableName(entity2Type, replacesTableName);

                string idName1 = ToolsProp.GetPK((object)entity).Name; // Ex: User
                string idName2 = ToolsProp.GetPK((object)entity2Type).Name;  // Ex: Group

                var itemsCollection = (IEnumerable<object>)invProp.Value.Value;
                if (itemsCollection is null) { continue; } // If the collection is null, there is no need to insert anything.

                foreach (var item in itemsCollection)
                {
                    PropertyInfo prop1 = entity.GetType().GetProperty(idName1);
                    PropertyInfo prop2 = item.GetType().GetProperty(idName2);

                    if (prop2 != null)
                    {
                        string idTb1 = tableName.Substring(0, Math.Min(tableName.Length, 27));
                        string idTb2 = tableName2.Substring(0, Math.Min(tableName2.Length, 27));

                        object idValue1 = prop1.GetValue(entity);
                        object idValue2 = prop2.GetValue(item);

                        queries.Add($"UPDATE {tableNameInverseProperty} SET ID_{idTb1} = '{idValue1}', ID_{idTb2} = '{idValue2}'");
                    }
                }
            }
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
            return $"SELECT * FROM {tableName} WHERE ({filter})";
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
            return $"SELECT * FROM {tableName} WHERE ({idPropName} = '{typeof(TEntity).GetProperty(idPropName).GetValue(entity, null)}')";
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

            return $"DELETE FROM {tableName} WHERE ({idPropName} = '{typeof(TEntity).GetProperty(idPropName).GetValue(entity, null)}')";
        }

        /// <summary>
        /// Allows you to obtain the table creation query for TEntity./>.
        /// </summary>
        /// <typeparam name="TEntity">The type of main entity.</typeparam>
        /// <param name="typesSql">Dictionary containing types related to C# code and database data.</param>
        /// <param name="ignoreProps">(Optional) The query to create table will ignore the listed properties.</param>
        /// <param name="onlyPrimaryTable">(Optional) If true, properties that do not belong to an auxiliary table will be ignored.</param>
        /// <param name="replacesTableName">(Optional) Terms that can be replaced in table names.</param>  
        /// <param name="tableName">(Optional) Name of the table to which the entity will be inserted. By default, the table informed in the "Table" attribute of the entity class will be considered.</param> 
        /// <returns>Table creation query. If it is necessary to create an auxiliary table, for an M:N relationship for example, more than one query will be returned.</returns>
        public ICollection<string?> CreateTable<TEntity>(Dictionary<string, string>? typesSql, ICollection<string>? ignoreProps = null, bool onlyPrimaryTable = false, Dictionary<string, string>? replacesTableName = null, string? tableName = null)
        {
            if (typesSql is null) { throw new ArgumentNullException(nameof(typesSql)); }
            ignoreProps ??= new List<string>();

            ICollection<string?> createsTable = new List<string?>();
            StringBuilder queryBuilderPrincipal = new();
            tableName ??= ToolsProp.GetTableName<TEntity>(replacesTableName);

            queryBuilderPrincipal.Append($"CREATE TABLE {tableName} (");

            TEntity entity = Activator.CreateInstance<TEntity>() ?? throw new ArgumentNullException(nameof(entity));
            var properties = ToolsProp.GetProperties(entity, false, false);
            var pk = ToolsProp.GetPK((object)entity);

            foreach (KeyValuePair<string, Property> prop in properties)
            {
                if (prop.Value?.Type is null) { throw new InvalidOperationException($"Error mapping entity '{nameof(entity)}' property types!"); }

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
                        queryBuilderPrincipal.Append($"{prop.Key} {value} PRIMARY KEY, ");
                    }
                    else
                    {
                        queryBuilderPrincipal.Append($"{prop.Key} {value}, ");
                    }

                    // MinimumLength?
                    if (prop.Value.MinLength > 0)
                    {
                        queryBuilderPrincipal.Append($"CHECK(LENGTH({prop.Key}) >= {prop.Value.MinLength}), ");
                    }
                }
                else // IsCollection
                {
                    if (onlyPrimaryTable) { continue; }
                    string queryCollection = CreateTableFromCollectionProp(replacesTableName, tableName, properties, pk, prop);
                    createsTable.Add(queryCollection);
                }
            }

            queryBuilderPrincipal.Length -= 2; // Remove the last comma and space
            queryBuilderPrincipal.Append(")");
            createsTable.Add(queryBuilderPrincipal.ToString());
            return createsTable;
        }

        private static string CreateTableFromCollectionProp(Dictionary<string, string>? replacesTableName, string? tableName, Dictionary<string, Property> properties, PropertyInfo? pk, KeyValuePair<string, Property> prop)
        {
            var pkEntity1 = pk.Name;
            string tableEntity1 = tableName;

            var propEntity2 = properties[prop.Value.Name];
            Type collection2Type = propEntity2.PropertyInfo.PropertyType;
            Type entity2Type = collection2Type.GetGenericArguments()[0];

            var propsEntity2 = entity2Type.GetProperties();
            var propPkEntity2 = propsEntity2.Where(prop => Attribute.IsDefined(prop, typeof(System.ComponentModel.DataAnnotations.KeyAttribute)));
            var pkEntity2 = propPkEntity2?.FirstOrDefault()?.Name ?? propsEntity2.FirstOrDefault().Name;

            TableAttribute table2Attribute = entity2Type.GetCustomAttribute<TableAttribute>();
            string tableEntity2 = table2Attribute.Name ?? entity2Type.Name;

            string tableNameManyToMany = ToolsProp.GetTableNameManyToMany(tableEntity1, entity2Type, replacesTableName);

            string idTb1 = tableEntity1.Substring(0, Math.Min(tableEntity1.Length, 27));
            string idTb2 = tableEntity2.Substring(0, Math.Min(tableEntity2.Length, 27));

            string queryCollection =
                $"CREATE TABLE {tableNameManyToMany} (" +
                $"ID_{tableEntity1} INT, ID_{tableEntity2} INT, " +
                $"PRIMARY KEY (ID_{idTb1}, ID_{idTb2}), " +
                $"FOREIGN KEY (ID_{idTb1}) REFERENCES {idTb1}({pkEntity1}), " +
                $"FOREIGN KEY (ID_{idTb2}) REFERENCES {idTb2}({pkEntity2}) " +
                $")";

            return queryCollection;
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
            tableName ??= Define.NameTableFromDataTable(dataTable.TableName, replacesTableName);

            queryBuilder.Append($"CREATE TABLE {tableName} (");

            var columns = dataTable.Columns;
            foreach (DataColumn column in columns)
            {
                string nameColumn = column.ColumnName;
                nameColumn = nameColumn.Length > 30 ? nameColumn.Substring(0, 30) : nameColumn;
                nameColumn = Tools.Normalize(nameColumn, '_', false);

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
            string sqlQuery = $@"
                ALTER TABLE {tableName}
                ADD CONSTRAINT {foreignKeyName}
                FOREIGN KEY ({childKeyColumn})
                REFERENCES {parentTableName}({parentKeyColumn});";

            return sqlQuery;
        }


    }
}
