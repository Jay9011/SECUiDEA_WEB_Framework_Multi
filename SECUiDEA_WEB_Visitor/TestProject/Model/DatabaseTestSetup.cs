using CoreDAL.Configuration;
using FileIOHelper;
using FileIOHelper.Helpers;

namespace TestProject.Model;

public class DatabaseTestSetup : IDisposable
{
    public IIOHelper _testIoHelper;
    public string IniFilePath { get; private set; }
    public Dictionary<string, (DatabaseType dbType, IIOHelper ioHelper)> SetupFiles { get; private set; }

    public DatabaseTestSetup(IIOHelper testIoHelper)
    {
        _testIoHelper = testIoHelper;
        
        SetupFiles = new Dictionary<string, (DatabaseType dbType, IIOHelper ioHelper)>
        {
            ["TestDB1"] = (DatabaseType.MSSQL, testIoHelper),
            ["TestDB2"] = (DatabaseType.ORACLE, testIoHelper)
        };
    }
    
    public void Dispose()
    {
        TestHelper.DeleteTempFile(IniFilePath);
    }
}