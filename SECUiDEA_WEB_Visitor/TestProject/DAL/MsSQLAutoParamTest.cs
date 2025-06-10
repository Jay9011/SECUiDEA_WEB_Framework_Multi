using CoreDAL.Configuration;
using CoreDAL.Configuration.Models;
using FileIOHelper.Helpers;

namespace TestProject.DAL;

public class MsSQLAutoParamTest
{
    private readonly DatabaseSetup _setup;
    private readonly MsSqlConnectionInfo _connectionInfo;
    private const string TEST_SECTION = "TEST_MSSQL_AutoParam";
    
    public MsSQLAutoParamTest ()
    {
        _connectionInfo = new MsSqlConnectionInfo
        {
            Server = "localhost",
            Database = "TEST_S1ACCESS",
            UserId = "sa",
            Password = "s1access!"
        };
        
        _setup = new DatabaseSetup(DatabaseType.MSSQL,
            new RegistryHelper("Software\\SECUIDEA"),
            TEST_SECTION);
        
        _setup.UpdateConnectionInfo(_connectionInfo);
    }

    [Fact]
    public void TestInsert()
    {
        // Arrange
        var param = new Dictionary<string, object>
        {
            ["Name"] = "Tester",
            ["NewId"] = 0,
            ["ResultMessage"] = string.Empty,
            ["NonDbParam"] = "Non-DB Param"
        };
        
        // Act
        var result = _setup.DAL.ExecuteProcedure(_setup, "sp_TestInsert", param);
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.True((int)param["NewId"] > 0);
        Assert.True(param["ResultMessage"].ToString().Contains(param["NewId"].ToString()));
    }
    
    [Fact]
    public void TestInsertAndUpdate()
    {
        // Arrange
        var insertParam = new Dictionary<string, object>
        {
            ["Name"] = "Tester",
            ["NewId"] = 0,
            ["ResultMessage"] = string.Empty
        };
        
        // Act
        var result = _setup.DAL.ExecuteProcedure(_setup, "sp_TestInsert", insertParam);
        
        // Arrange
        var updateParam = new Dictionary<string, object>
        {
            ["Id"] = insertParam["NewId"],
            ["Name"] = "Updated Tester",
            ["RowsAffected"] = 0,
            ["ResultMessage"] = string.Empty,
            ["ResultDateTime"] = "1900-01-01"
        };
        
        // Act
        result = _setup.DAL.ExecuteProcedure(_setup, "sp_TestUpdate", updateParam);
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.True((int)updateParam["RowsAffected"] > 0);
        Assert.True(updateParam["ResultMessage"].ToString().Contains(updateParam["RowsAffected"].ToString()));
        
        var resultDateTime = (DateTime)updateParam["ResultDateTime"];
        var nearNow = DateTime.Now.AddMinutes(-5);
        Assert.True(resultDateTime > nearNow);
    }

    [Fact]
    public void TestSelect()
    {
        // Act
        var result = _setup.DAL.ExecuteProcedure(_setup, "sp_TestSelect");
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.DataSet.Tables.Count, 5);
        Assert.Equal(result.ReturnValue, 10);
    }
}