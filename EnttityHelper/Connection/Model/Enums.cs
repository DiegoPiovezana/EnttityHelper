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
        public enum DbProvider
        {
            /// <summary>
            /// Oracle
            /// </summary>
            Oracle,

            /// <summary>
            /// SQL Server
            /// </summary>
            SqlServer,

            /// <summary>
            /// SQLite
            /// </summary>
            SqLite,

            /// <summary>
            /// PostgreSQL
            /// </summary>
            PostgreSql,

            /// <summary>
            /// MySQL
            /// </summary>
            MySql
        }
    }
}
