using System.Data;

namespace EH.Connection
{
    public abstract class DatabaseFactory
    {
        public IDbConnection IDbConnection { get; internal set; } 
          
        public string Ip { get; set; }
        public string Port { get; set; }
        public string Service { get; set; }
        public string User { get; set; }
        public string Pass { get; set; }

        public string Type { get; set; } // Oracle or SqlServer
        public string Name { get; set; }
        public string Owner { get; set; }
        public string Chcp { get; set; }
        public string NlsLang { get; set; }
        public string PathClient { get; set; }


        #region Abstract Functions
        public abstract IDbConnection CreateConnection();
        public abstract IDbCommand CreateCommand();
        public abstract IDbConnection CreateOpenConnection();
        public abstract bool OpenConnection();
        public abstract bool CloseConnection();
        public abstract IDbCommand CreateCommand(string commandText);
        public abstract IDbCommand CreateStoredProcCommand(string procName, IDbConnection connection);
        public abstract IDataParameter CreateParameter(string parameterName, object parameterValue);
        public abstract object Clone();        

        #endregion

    }
}
