using System.Data;
using static EH.Connection.Enums;

namespace EH.Connection
{
    /// <summary>
    /// Represents a factory for creating database connections.
    /// </summary>
    public abstract class DatabaseFactory
    {
        /// <summary>
        /// Gets or sets the IDbConnection object.
        /// </summary>
        public IDbConnection? IDbConnection { get; internal set; }

        /// <summary>
        /// Gets or sets the IP address.
        /// </summary>
        public string? Ip { get; set; }

        /// <summary>
        /// Gets or sets the port number.
        /// </summary>
        public int? Port { get; set; }

        /// <summary>
        /// Gets or sets the service name.
        /// </summary>
        public string? Service { get; set; }

        /// <summary>
        /// Gets or sets the username for authentication.
        /// </summary>
        public string? User { get; set; }

        /// <summary>
        /// Gets or sets the password for authentication.
        /// </summary>
        public string? Pass { get; set; }

        /// <summary>
        /// Gets or sets the type of database (e.g., Oracle or SqlServer).
        /// </summary>
        public Enums.DbType? Type { get; set; }

        /// <summary>
        /// Gets or sets the database name.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the database owner.
        /// </summary>
        public string? Owner { get; set; }

        /// <summary>
        /// Gets or sets the character set.
        /// </summary>
        public string? Chcp { get; set; }

        /// <summary>
        /// Gets or sets the NLS language.
        /// </summary>
        public string? NlsLang { get; set; }

        /// <summary>
        /// Gets or sets the path for the client.
        /// </summary>
        public string? PathClient { get; set; }



        #region Abstract Functions

        /// <summary>
        /// Creates a new IDbConnection instance.
        /// </summary>
        public abstract IDbConnection CreateConnection();

        /// <summary>
        /// Creates a new IDbCommand instance.
        /// </summary>
        public abstract IDbCommand CreateCommand();

        /// <summary>
        /// Creates a new IDbTransaction instance.
        /// </summary>
        /// <returns></returns>
        public abstract IDbTransaction CreateTransaction();

        /// <summary>
        /// Creates and opens a new IDbConnection instance.
        /// </summary>
        public abstract IDbConnection CreateOpenConnection();

        /// <summary>
        /// Opens the database connection.
        /// </summary>
        public abstract bool OpenConnection();

        /// <summary>
        /// Closes the database connection.
        /// </summary>
        public abstract bool CloseConnection();

        /// <summary>
        /// Creates an IDbCommand instance with the specified command text.
        /// </summary>
        public abstract IDbCommand CreateCommand(string commandText);

        /// <summary>
        /// Creates an IDbCommand instance for a stored procedure with the specified name and connection.
        /// </summary>
        public abstract IDbCommand CreateStoredProcCommand(string procName, IDbConnection connection);

        /// <summary>
        /// Creates an IDataParameter instance with the specified name and value.
        /// </summary>
        public abstract IDataParameter CreateParameter(string parameterName, object parameterValue);

        /// <summary>
        /// Creates a new instance of the DatabaseFactory class.
        /// </summary>
        public abstract object Clone();

        #endregion
    }
}