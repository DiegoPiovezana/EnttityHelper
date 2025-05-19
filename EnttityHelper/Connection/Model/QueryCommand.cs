using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using EH.Properties;

namespace EH.Connection
{
    /// <summary>
    /// Represents a command containing a SQL query and its associated parameters.
    /// </summary>
    public class QueryCommand
    {
        /// <summary>
        /// Gets or sets the SQL query string associated with the command.
        /// </summary>
        /// <remarks>
        /// This property holds the SQL query to be executed by the command. The value
        /// can be either a raw SQL statement or a predefined query string. It is the
        /// core of the <see cref="QueryCommand"/> and is necessary for executing database operations.
        /// </remarks>
        public string Sql { get; set; } = string.Empty;

        /// <summary>
        /// Gets the collection of parameters associated with the SQL query.
        /// </summary>
        /// <remarks>
        /// This property contains the parameters used in the SQL query. Each parameter is represented
        /// as a key-value pair, where the key is the parameter name and the value is an instance
        /// of <see cref="EH.Properties.Property"/>. The parameters provide values to the placeholders
        /// in the query, ensuring proper execution and security against SQL injection.
        /// </remarks>
        public IDictionary<string, Property> Parameters { get; } = new Dictionary<string, Property>();

        /// <summary>
        /// Gets or sets the database provider to be used for the query execution.
        /// </summary>
        /// <remarks>
        /// The database provider determines the type of database for which the query is intended.
        /// Examples of supported providers include Oracle, SQL Server, SQLite, PostgreSQL, and MySQL.
        /// This property plays a critical role in ensuring that the query and parameters are compatible
        /// with the corresponding database engine.
        /// </remarks>
        public Enums.DbProvider? DbProvider { get; set; }


        public QueryCommand(string sql, IDictionary<string, Property>? parameters, Enums.DbProvider? dbProvider)
        {
            if (string.IsNullOrWhiteSpace(sql))
                throw new ArgumentException("SQL query cannot be null or empty.", nameof(sql));
            
            Sql = sql;
            Parameters = parameters ?? new Dictionary<string, Property>();
            DbProvider = dbProvider;
        }
        
        
        public void AddParameter(string name, Property property)
        {
            if (!Parameters.ContainsKey(name))
                Parameters.Add(name, property);
            else
                throw new ArgumentException($"Parameter '{name}' already exists.");
        }



        public override string ToString()
        {
            var parametersStr = string.Join(", ", Parameters.Select(p => $"{p.Key}={p.Value?.Value}"));
            return $"{Sql} [{parametersStr}]";
        }

    }
}