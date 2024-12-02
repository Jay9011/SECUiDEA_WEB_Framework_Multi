using CoreDAL.Configuration;
using TestProject.Model;

namespace TestProject.DAL;

public class DatabaseSetupTests : IDisposable
{
    private readonly DatabaseTestSetup _testSetup;
    private readonly DatabaseSetupContainer _container;
    
    public DatabaseSetupTests()
    {
        _testSetup = new DatabaseTestSetup();
        _container = new DatabaseSetupContainer(_testSetup.SetupFiles);
    }

    [Fact]
    public void GetSetup_ShouldReturnCorrectSetup()
    {
        // Arrange
        const string sectionName = "TestDB1";
        
        // Act
        var setup = _container.GetSetup(sectionName);
        
        // Assert
        Assert.NotNull(setup);
        Assert.IsType<DatabaseSetup>(setup);
    }
    
    [Fact]
    public void UpdateSetup_ShouldUpdateConnectionInfo()
    {
        // Arrange
        const string sectionName = "TestDB1";
        var connectionInfo = TestMsSqlConnectionInfo.CreateValidConnectionInfo();
        
        // Act
        _container.UpdateSetup(sectionName, connectionInfo);
        var setup = _container.GetSetup(sectionName);
        string updatedConnectionString = setup.GetConnectionString();
        
        // Assert
        Assert.Contains(connectionInfo.Server, updatedConnectionString);
        Assert.Contains(connectionInfo.Database, updatedConnectionString);
    }

    [Fact]
    public void GetSetup_ShouldThrowException_WhenSectionNotFound()
    {
        // Arrange
        const string sectionName = "NonExistentDB";
        
        // Act & Assert
        Assert.Throws<KeyNotFoundException>(() => _container.GetSetup(sectionName));
    }
    
    public void Dispose()
    {
        _testSetup.Dispose();
    }
}