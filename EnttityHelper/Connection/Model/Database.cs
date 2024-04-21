﻿using Oracle.ManagedDataAccess.Client;
using System;
using System.Data;
using System.Data.SqlClient;

namespace EH.Connection
{
    /// <summary>
    /// Main methods of creating, opening and closing a connection.
    /// </summary>
    public class Database : DatabaseFactory
    {
        /// <summary>
        /// Creates an object for the database informing the connection string.
        /// </summary>
        /// <param name="stringConnection">Database connection string</param> 
        /// <remarks>
        /// <para>
        /// This method supports the following database types:
        /// </para>
        /// <list type="bullet">
        /// <item><description>Oracle</description><para>Example: "Data Source={Ip}:{Port}/{Service};User Id={User};Password={Pass}"</para>Sample: "Data Source=127.0.0.1:1521/xe;User Id=myUser;Password=myPassword"</item>
        /// <item><description>SqlServer</description><para>Example: "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog={Service};User ID={User};Password={Pass}"</para>Alternative: "Data Source={Ip};Initial Catalog={Service};User ID={User};Password={Pass}"</item>
        /// <item><description>Sqlite</description><para>Example: "Data Source=c:\mydb.db;Version=3;Password=myPassword;"</para>Alternative: "Data Source=c:\mydb.db;Version=3;"</item>
        /// </list>        
        /// </remarks>
        public Database(string stringConnection)
        {
            try
            {
                ToolsDbConnect.MapDatabase(stringConnection, this);
                CreateConnection();
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Creates an object for the database informing the connection string.
        /// </summary>
        /// <returns>Connection</returns>
        /// <exception cref="Exception"></exception>
        public override IDbConnection CreateConnection()
        {
            IDbConnection = Type switch
            {
                Enums.DbType.Oracle => new OracleConnection($"Data Source={Ip}:{Port}/{Service};User Id={User};Password={Pass}"),
                Enums.DbType.SQLServer => new SqlConnection($"Data Source={Ip};Initial Catalog={Service};User ID={User};Password={Pass}"),
                _ => throw new Exception("Invalid database type!"),
            };
            return IDbConnection;
        }

        /// <summary>
        /// Creates a command object.
        /// </summary>
        /// <returns>Command</returns>
        public override IDbCommand? CreateCommand()
        {
            return IDbConnection?.CreateCommand();
        }

        /// <summary>
        /// Creates a command object.
        /// </summary>
        /// <returns>Transactio</returns>
        public override IDbTransaction? CreateTransaction()
        {
            return IDbConnection?.BeginTransaction();
        }

        /// <summary>
        /// Create and open a connection.
        /// </summary>
        /// <returns>Connection</returns>
        public override IDbConnection CreateOpenConnection()
        {
            IDbConnection = CreateConnection();
            IDbConnection.Open();
            return IDbConnection;
        }

        /// <summary>
        /// Creates a command object.
        /// </summary>
        /// <param name="commandText"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public override IDbCommand CreateCommand(string commandText)
        {
            if (IDbConnection is null) throw new Exception("Connection is null!");

            return Type switch
            {
                Enums.DbType.SQLServer => new SqlCommand(commandText, (SqlConnection)IDbConnection),
                Enums.DbType.Oracle => new OracleCommand(commandText, (OracleConnection)IDbConnection),
                _ => throw new Exception("Invalid database type!"),
            };
        }

        /// <summary>
        /// Create a stored procedure command object.
        /// </summary>
        /// <param name="procName"></param>
        /// <param name="connection"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public override IDbCommand CreateStoredProcCommand(string procName, IDbConnection? connection)
        {
            if (connection is null) throw new Exception("Connection is null!");

            return Type switch
            {
                Enums.DbType.SQLServer => new SqlCommand
                {
                    CommandType = CommandType.StoredProcedure,
                    CommandText = procName,
                    Connection = (SqlConnection)connection
                },
                Enums.DbType.Oracle => new OracleCommand
                {
                    CommandType = CommandType.StoredProcedure,
                    CommandText = procName,
                    Connection = (OracleConnection)connection
                },
                _ => throw new Exception("Invalid database type!"),
            };
        }

        /// <summary>
        /// Create a parameter object.
        /// </summary>
        /// <param name="parameterName"></param>
        /// <param name="parameterValue"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public override IDataParameter CreateParameter(string parameterName, object parameterValue)
        {
            return Type switch
            {
                Enums.DbType.SQLServer => new SqlParameter(parameterName, parameterValue),
                Enums.DbType.Oracle => new OracleParameter(parameterName, parameterValue),
                _ => throw new Exception("Invalid database type!"),
            };
        }

        /// <summary>
        /// Creates a bulk copy object specific to the database type.
        /// </summary>    
        /// <returns>
        /// <para>A bulk copy object specific to the database type.</para>
        /// Returns an IDbBulkCopy implementation for SQL Server (SqlBulkCopy) or Oracle (OracleBulkCopy), depending on the configured database type.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown when the database type is not supported.</exception>
        public override object CreateBulkCopy()
        {
            return Type switch
            {
                Enums.DbType.SQLServer => new SqlBulkCopy((SqlConnection)IDbConnection),
                Enums.DbType.Oracle => new OracleBulkCopy((OracleConnection)IDbConnection),
                _ => throw new ArgumentException("Invalid database type!", nameof(Type)),
            };
        }

        /// <summary>
        /// Validates that the connection can be made.
        /// </summary>
        public bool ValidateConnection()
        {
            try
            {
                if (OpenConnection())
                {
                    CloseConnection();
                    return true;
                }
                else
                {
                    throw new Exception("Failed to open database connection!");
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Opens the database connection.
        /// </summary>   
        public override bool OpenConnection()
        {
            try
            {
                if (IDbConnection is null) throw new Exception("Connection is null!");
                if (IDbConnection.State != ConnectionState.Open) { IDbConnection.Open(); }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Terminates the connection.
        /// </summary>     
        public override bool CloseConnection()
        {
            try
            {
                if (IDbConnection is null) throw new Exception("Connection is null!");
                IDbConnection.Close();
                GC.SuppressFinalize(this);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Clone the object.
        /// </summary>
        /// <returns></returns>
        public override object Clone()
        {
            return MemberwiseClone();
        }

    }
}
