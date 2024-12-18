using FileIOHelper;
using Microsoft.Win32;

namespace TestProject.Helper;

public class RegistryHelperTests : IDisposable
{
    private const string RegistryPath = "Software\\SECUIDEA";
    private readonly IIOHelper _registryHelper;
    
    public RegistryHelperTests()
    {
        _registryHelper = IOHelperFactory.Create(IOType.Registry, RegistryPath, RegistryHive.CurrentUser);
    }

    [Fact]
    public void CheckPermission()
    {
        // Act
        bool result = _registryHelper.CheckPermission(RegistryPath, FileAccess.ReadWrite);
        
        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsExists()
    {
        // Arrange
        const string section = "TestSection";
        const string key = "TestKey";
        const string value = "TestValue";
        
        // Act
        bool before = _registryHelper.IsExists();
        _registryHelper.WriteValue(section, key, value);
        bool after = _registryHelper.IsExists();
        
        // Assert
        Assert.False(before);
        Assert.True(after);
    }
    
    [Fact]
    public void WriteAndReadValue()
    {
        // Arrange
        const string section = "WriteAndReadValueSection";
        const string key = "TestKey";
        const string value = "TestValue";
        
        // Act
        _registryHelper.WriteValue(section, key, value);
        string actual = _registryHelper.ReadValue(section, key);
        
        // Assert
        Assert.Equal(value, actual);
    }

    [Fact]
    public void WriteAndReadSection()
    {
        // Arrange
        const string section = "WriteAndReadSection";
        var values = new Dictionary<string, string>()
        {
            { "Key1", "Value1" },
            { "Key2", "Value2" },
            { "Key3", "Value3" },
        };
        
        // Act
        _registryHelper.WriteSection(section, values);
        Dictionary<string, string> actual = _registryHelper.ReadSection(section);
        
        // Assert
        Assert.Equal(values, actual);
    }

    [Fact]
    public void CacheWorksWhenWriteValueWithHelper()
    {
        // Arrange
        const string section = "CacheWorksWhenWriteValueWithHelper";
        const string key = "TestKey";
        const string value = "TestValue";
        const string updatedValue = "UpdatedValue";
        
        // Act
        _registryHelper.WriteValue(section, key, value);
        string firstRead = _registryHelper.ReadValue(section, key);
        
        _registryHelper.WriteValue(section, key, updatedValue);
        string secondRead = _registryHelper.ReadValue(section, key);
        
        // Assert
        Assert.NotEqual(firstRead, secondRead);
        Assert.Equal(value, firstRead);
        Assert.Equal(updatedValue, secondRead);
    }
    
    public void Dispose()
    {
        Registry.CurrentUser.DeleteSubKeyTree(RegistryPath, false);
    }
}