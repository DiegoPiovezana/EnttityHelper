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
        public IDictionary<string, Property> Parameters { get; private set; } = new Dictionary<string, Property>();

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
        
        /// <summary>
        /// Gets or sets the prefix parameter used in database operations. E.g., @ or :
        /// </summary>
        public string PrefixParameter { get; set; }
        

        public QueryCommand(string sql, IDictionary<string, Property>? parameters, Enums.DbProvider? dbProvider, string prefixParameter)
        {
            if (string.IsNullOrWhiteSpace(prefixParameter))
                throw new ArgumentException("Prefix parameter cannot be null or empty.", nameof(prefixParameter));
            
            if (dbProvider == null)
                throw new ArgumentNullException(nameof(dbProvider), "Database provider cannot be null.");
        
            if (string.IsNullOrWhiteSpace(sql))
                throw new ArgumentException("SQL query cannot be null or empty.", nameof(sql));
            
            Sql = sql;
            Parameters = parameters ?? new Dictionary<string, Property>();
            DbProvider = dbProvider;
            PrefixParameter = prefixParameter;
        }

        /// <summary>
        /// Adds a parameter to the command's parameter collection.
        /// </summary>
        /// <param name="name">The name of the parameter to add.</param>
        /// <param name="property">The property representing the parameter value to add.</param>
        /// <exception cref="ArgumentException">
        /// Thrown if a parameter with the specified name already exists in the collection.
        /// </exception>
        public void AddParameter(string name, Property property)
        {
            if (!Parameters.ContainsKey(name))
                Parameters.Add(name, property);
            else
                throw new ArgumentException($"Parameter '{name}' already exists.");
        }

        /// <summary>
        /// Removes a parameter from the command's parameter collection by its name.
        /// </summary>
        /// <param name="name">The name of the parameter to remove.</param>
        /// <exception cref="ArgumentException">
        /// Thrown if the parameter with the specified name does not exist in the collection.
        /// </exception>
        public void RemoveParameter(string name)
        {
            if (Parameters.ContainsKey(name))
                Parameters.Remove(name);
            else
                throw new ArgumentException($"Parameter '{name}' does not exist.");
        }


        /// <summary>
        /// Converts the SQL query and its associated parameters into a complete query string with parameter values substituted in place of placeholders.
        /// </summary>
        /// <returns>
        /// A string representing the SQL query with parameter placeholders replaced by their respective values.
        /// </returns>
        public string ToQuery()
        {
            var query = Sql;
            foreach (var parameter in Parameters)
            {
                var placeholder = $"{PrefixParameter}{parameter.Key}";
                var value = parameter.Value?.Value;

                // if (value is string)
                //     query = query.Replace(placeholder, $"'{value}'");
                // else
                //     query = query.Replace(placeholder, value.ToString());
                
                query = query.Replace(placeholder, $"'{value}'");
            }

            return query;
            
        }

        /// <summary>
        /// Returns a string representation of the current query command, including the SQL statement and its associated parameters.
        /// </summary>
        /// <returns>
        /// A string containing the SQL query and its parameters in the format "SQL [parameter1=value1, parameter2=value2]".
        /// </returns>
        public override string ToString()
        {
            var parametersStr = string.Join(", ", Parameters.Select(p => $"{p.Key}={p.Value?.Value}"));
            return $"{Sql} [{parametersStr}]";
        }

    }
}