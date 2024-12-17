using CoreDAL.Configuration;
using CoreDAL.Configuration.Interface;
using CryptoManager;
using CryptoManager.Services;
using FileIOHelper;
using FileIOHelper.Helpers;
using TestProject.Model;

namespace TestProject.DAL;

public class DatabaseRegistryTests : IDisposable
{
    private readonly IIOHelper _ioHelper = new RegistryHelper("Software\\SECUIDEA");
    private readonly DatabaseTestSetup _testSetup;
    private readonly DatabaseSetupContainer _container;
    private readonly DatabaseSetupContainer _containerWithCryption;

    public DatabaseRegistryTests()
    {
        _testSetup = new DatabaseTestSetup(_ioHelper);
        _container = new DatabaseSetupContainer(_testSetup.SetupFiles);
        ICryptoManager cryptoManager = new S1AES("tLscn/FdlqXhd4Wp");
        _containerWithCryption = new DatabaseSetupContainer(_testSetup.SetupFiles, cryptoManager);
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
    public void TestContainerWithCryption()
    {
        // Arrange
        const string sectionName = "TestDB2";
        var connectionInfo = TestOracleConnectionInfo.CreateValidConnectionInfo();
        
        // Act
        _containerWithCryption.UpdateSetup(sectionName, connectionInfo);
        var setup = _containerWithCryption.GetSetup(sectionName);
        string updatedConnectionString = setup.GetConnectionString();
        
        string encryptedHostName = _ioHelper.ReadValue("TestDB2", "Host");
        
        // Assert
        Assert.Contains(connectionInfo.Host, updatedConnectionString);
        Assert.Contains(connectionInfo.Protocol, updatedConnectionString);
        Assert.NotEqual(connectionInfo.Host, encryptedHostName);
    }

    public void Dispose()
    {
    }
}