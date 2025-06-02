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
        /// Gets the output parameters associated with the SQL query.
        /// </summary>
        /// <remarks>
        /// This property contains a collection of output parameters that are returned
        /// by the execution of the SQL query. Each entry in the collection is a key-value
        /// pair where the key is the parameter name and the value is the associated <see cref="Property"/> object.
        /// Output parameters are typically used to return values from stored procedures or queries.
        /// </remarks>
        public IDictionary<string, Property> ParametersOutput { get; private set; } = new Dictionary<string, Property>();

        // public string SqlRollback { get; set; } = string.Empty;
        // public DbTransaction? Transaction { get; set; }
        // public DateTime ExecutionDate { get; set; }
        // public bool Commited { get; set; } = false;
        
        

        public QueryCommand(string sql, IDictionary<string, Property>? parameters, IDictionary<string, Property>? parametersOutput = null)
        {
            if (string.IsNullOrWhiteSpace(sql))
                throw new ArgumentException("SQL query cannot be null or empty.", nameof(sql));
            
            Sql = sql;
            Parameters = parameters ?? new Dictionary<string, Property>();
            ParametersOutput = parametersOutput ?? new Dictionary<string, Property>();
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
        public string ToQuery(Database db)
        {
            var query = Sql;
            
            foreach (var parameter in Parameters
                         .OrderByDescending(p => p.Key.Length))
            {
                var placeholder = $"{db.PrefixParameter}{parameter.Key}";
                var value = parameter.Value?.ValueSql?.ToString() ?? string.Empty;

                // if (value is string)
                //     query = query.Replace(placeholder, $"'{value}'");
                // else
                //     query = query.Replace(placeholder, value.ToString());

                value = value.Replace("'", "''");
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