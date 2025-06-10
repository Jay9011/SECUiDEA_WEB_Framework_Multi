namespace TestProject.DAL;
using CoreDAL.Configuration;
using CoreDAL.Configuration.Models;
using CoreDAL.ORM;
using CoreDAL.ORM.Extensions;
using FileIOHelper.Helpers;

public class MsSQLAutoSQLParamTest
{
    private readonly DatabaseSetup _setup;
    private readonly MsSqlConnectionInfo _connectionInfo;
    private const string TEST_SECTION = "TEST_MSSQL_AutoParam";
    
    public MsSQLAutoSQLParamTest()
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

    public class TestInsertParam : SQLParam
    {
        [DbParameter]
        public string Name { get; set; }
        
        [DbParameter]
        public int NewId { get; set; }
        [DbParameter]
        public string NonDbParam { get; set; }
        
        [DbParameter]
        public string ResultMessage { get; set; }
    }

    [Fact]
    public void TestInsert()
    {
        // Arrange
        var param = new TestInsertParam
        {
            Name = "Tester",
            NewId = 0,
            ResultMessage = string.Empty,
            NonDbParam = "Non-DB Param"
        };
        
        // Act
        var result = _setup.DAL.ExecuteProcedure(_setup, "sp_TestInsert", param);
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(param.NewId > 0);
        Assert.True(param.ResultMessage.Contains(param.NewId.ToString()));
    }

    public class TestUpdateParam : SQLParam
    {
        [DbParameter]
        public int Id { get; set; }
        
        [DbParameter]
        public string Name { get; set; }
        
        [DbParameter]
        public int RowsAffected { get; set; }
        
        [DbParameter]
        public string ResultMessage { get; set; }
        
        [DbParameter]
        public DateTime ResultDateTime { get; set; }
    }
    
    [Fact]
    public void TestInsertAndUpdate()
    {
        // Arrange
        var insertParam = new TestInsertParam
        {
            Name = "Tester",
            NewId = 0,
            ResultMessage = string.Empty
        };
        
        // Act
        var result = _setup.DAL.ExecuteProcedure(_setup, "sp_TestInsert", insertParam);
        
        // Arrange
        var updateParam = new TestUpdateParam
        {
            Id = insertParam.NewId,
            Name = "Updated Tester",
            RowsAffected = 0,
            ResultMessage = string.Empty,
            ResultDateTime = new DateTime(1900, 1, 1)
        };
        
        // Act
        result = _setup.DAL.ExecuteProcedure(_setup, "sp_TestUpdate", updateParam);
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(updateParam.RowsAffected > 0);
        Assert.True(updateParam.ResultMessage.Contains(updateParam.RowsAffected.ToString()));
        
        var nearNow = DateTime.Now.AddMinutes(-5);
        Assert.True(updateParam.ResultDateTime > nearNow);
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