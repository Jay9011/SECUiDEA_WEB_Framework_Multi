using TestProject.Model;

namespace TestProject.DAL;

public class ConnectionInfoTests
{
    [Fact]
    public void MsSqlConnectionInfo_Validate_ShouldPass_WithValidInfo()
    {
        // Arrange
        var connectionInfo = TestMsSqlConnectionInfo.CreateValidConnectionInfo();

        // Act
        bool isValid = connectionInfo.Validate(out string errorMessage);

        // Assert
        Assert.True(isValid);
        Assert.Empty(errorMessage);
    }
    
    [Fact]
    public void MsSqlConnectionInfo_ToConnectionString_ShouldGenerateValidString()
    {
        // Arrange
        var connectionInfo = TestMsSqlConnectionInfo.CreateValidConnectionInfo();
        
        // Act
        string connectionString = connectionInfo.ToConnectionString();
        
        // Assert
        Assert.Equal("Server=localhost,1433;Database=TestDB;User Id=sa;Password=password;", connectionString);
    }
    
    [Fact]
    public void OracleConnectionInfo_Validate_ShouldPass_WithValidInfo()
    {
        // Arrange
        var connectionInfo = TestOracleConnectionInfo.CreateValidConnectionInfo();

        // Act
        bool isValid = connectionInfo.Validate(out string errorMessage);

        // Assert
        Assert.True(isValid);
        Assert.Empty(errorMessage);
    }
    
    [Fact]
    public void OracleConnectionInfo_ToConnectionString_ShouldGenerateValidString()
    {
        // Arrange
        var connectionInfo = TestOracleConnectionInfo.CreateValidConnectionInfo();
        
        // Act
        string connectionString = connectionInfo.ToConnectionString();
        
        // Assert
        Assert.Equal("Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=localhost)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=TestDB)));User Id=sa;Password=password;", connectionString);
    }
}