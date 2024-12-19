using System.Data;
using CoreDAL.Configuration;
using CoreDAL.Configuration.Interface;
using CoreDAL.Configuration.Models;
using CoreDAL.ORM;
using CoreDAL.ORM.Extensions;

namespace TestProject.DAL;

public class SECUTest
{
    private readonly IDatabaseSetup _setup;
    private readonly MsSqlConnectionInfo _connectionInfo;
    private const string TEST_SECTION = "TEST_SECU_MSSQL";

    public SECUTest()
    {
        _connectionInfo = new MsSqlConnectionInfo
        {
            Server = "192.168.0.63",
            Database = "S1ACCESS",
            UserId = "sa",
            Password = "s1access!"
        };
        
        _setup = new DatabaseSetup(DatabaseType.MSSQL, 
            new FileIOHelper.Helpers.RegistryHelper("Software\\SECUIDEA"), 
            TEST_SECTION);
        _setup.UpdateConnectionInfo(_connectionInfo);
    }

    public class TokenValue
    {
        public string ConfValue { get; set; }
        public string LocalKey { get; set; }
    }
    
    [Fact]
    public async Task APIKeyTest()
    {
        // Arrange & Act
        SQLResult result = await _setup.DAL.TestConnectionAsync(_setup);
        
        var parameters = new Dictionary<string, object>
        {
            { "UserID", "apiUser" },
            { "UserEmail", "api@api.com" },
            { "TokenConfKey", "Token Valid Time" }
        };

        result = _setup.DAL.ExecuteProcedure(_setup, "WEB_API_GETTOKEN_SEL", parameters);

        DataTable dt = result.DataSet.Tables[0];
        DataRow dr = result.DataSet.Tables[1].Rows[0];

        var tokens = dt.ToObject<TokenValue>();
        var token = dr.ToObject<TokenValue>();
        
        // Assert
        Assert.True(result.IsSuccess);
    }
}