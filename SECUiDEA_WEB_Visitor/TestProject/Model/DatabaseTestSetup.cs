using CoreDAL.Configuration;

namespace TestProject.Model;

public class DatabaseTestSetup : IDisposable
{
    public string IniFilePath { get; private set; }
    public Dictionary<string, (string filePath, DatabaseType dbType)> SetupFiles { get; private set; }

    public DatabaseTestSetup()
    {
        IniFilePath = TestHelper.CreateTempFile();
        SetupFiles = new Dictionary<string, (string filePath, DatabaseType dbType)>
        {
            ["TestDB1"] = (IniFilePath, DatabaseType.MSSQL),
            ["TestDB2"] = (IniFilePath, DatabaseType.ORACLE)
        };
    }
    
    public void Dispose()
    {
        TestHelper.DeleteTempFile(IniFilePath);
    }
}