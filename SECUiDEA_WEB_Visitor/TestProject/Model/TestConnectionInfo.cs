using CoreDAL.Configuration.Models;

namespace TestProject.Model;

public class TestMsSqlConnectionInfo
{
    public static MsSqlConnectionInfo CreateValidConnectionInfo()
    {
        return new MsSqlConnectionInfo()
        {
            Server = "localhost",
            Database = "TestDB",
            UserId = "sa",
            Password = "password",
            Port = 1433
        };
    }
}

public class TestOracleConnectionInfo
{
    public static OracleConnectionInfo CreateValidConnectionInfo()
    {
        return new OracleConnectionInfo()
        {
            Host = "localhost",
            Port = 1521,
            ServiceName = "TestDB",
            UserId = "sa",
            Password = "password"
        };
    }
}