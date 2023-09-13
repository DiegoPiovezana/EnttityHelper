using Oracle.ManagedDataAccess.Client;
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
        public Database(string stringConnection)
        {
            try
            {
                ToolsDb.MapDatabase(stringConnection, this);
                CreateConnection();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public override IDbConnection CreateConnection()
        {
            IDbConnection = Type switch
            {
                "sqlserver" => new SqlConnection($"Data Source={Ip};Initial Catalog={Service};User ID={User};Password={Pass}"),
                "oracle" => new OracleConnection($"Data Source={Ip}:{Port}/{Service};User Id={User};Password={Pass}"),
                _ => throw new Exception("Invalid database type!"),
            };
            return IDbConnection;
        }

        public override IDbCommand CreateCommand()
        {
            return IDbConnection.CreateCommand();
        }

        public override IDbConnection CreateOpenConnection()
        {
            IDbConnection = CreateConnection();
            IDbConnection.Open();
            return IDbConnection;
        }

        public override IDbCommand CreateCommand(string commandText)
        {
            return Type switch
            {
                "sqlserver" => new SqlCommand(commandText, (SqlConnection)IDbConnection),
                "oracle" => new OracleCommand(commandText, (OracleConnection)IDbConnection),
                _ => throw new Exception("Invalid database type!"),
            };
        }

        public override IDbCommand CreateStoredProcCommand(string procName, IDbConnection connection)
        {
            return Type switch
            {
                "sqlserver" => new SqlCommand
                {
                    CommandType = CommandType.StoredProcedure,
                    CommandText = procName,
                    Connection = (SqlConnection)connection
                },
                "oracle" => new OracleCommand
                {
                    CommandType = CommandType.StoredProcedure,
                    CommandText = procName,
                    Connection = (OracleConnection)connection
                },
                _ => throw new Exception("Invalid database type!"),
            };
        }

        public override IDataParameter CreateParameter(string parameterName, object parameterValue)
        {
            return Type switch
            {
                "sqlserver" => new SqlParameter(parameterName, parameterValue),
                "oracle" => new OracleParameter(parameterName, parameterValue),
                _ => throw new Exception("Invalid database type!"),
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
                IDbConnection.Close();
                GC.SuppressFinalize(this);             
                return true;
            }
            catch (Exception)
            {                          
                return false;
            }
        }

        protected virtual void Dispose(bool disposing = true)
        {
            if (disposing) IDbConnection.Close();
            GC.SuppressFinalize(this);
        }

        public override object Clone()
        {
            return MemberwiseClone();
        }

    }
}
