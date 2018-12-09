namespace GoldRush.Data.DbContexts
{
    public static class ConnectionStringTemplates
    {
        public const string MSSQL = @"Server=HOST_NAME;Database=DATABASE_NAME;UID=USER_NAME;Pwd=PASSWORD;";
        public const string MYSQL = @"Host=localhost;Port=3306;Database=mysql-db;UID=sa;PWD=masterkey;";
        public const string POSTGRESQL = @"Host=HOST_NAME;Database=DATABASE_NAME;Username=USER_NAME;Password=PASSWORD;Port=5432";
        public const string SQLITE = @"Data Source=DATABASE_FILENAME";

    }
}