namespace EH.Connection
{
    /// <summary>
    /// Contains the types of databases supported by the library.
    /// </summary>
    public static class Enums
    {
        /// <summary>
        /// Types of databases supported by the library.
        /// </summary>
        public enum DbType
        {
            /// <summary>
            /// Oracle (Older versions prior to 12c)
            /// </summary>
            Oracle,

            /// <summary>
            /// Oracle 12c or newer versions
            /// </summary>
            Oracle_Newer,

            /// <summary>
            /// SQL Server
            /// </summary>
            SQLServer,

            /// <summary>
            /// SQLite
            /// </summary>
            SQLite,

            /// <summary>
            /// PostgreSQL
            /// </summary>
            PostgreSQL,

            /// <summary>
            /// MySQL
            /// </summary>
            MySQL
        }
    }
}
