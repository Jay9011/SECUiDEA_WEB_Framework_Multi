using FileIOHelper.Helpers;
using FileIOHelper.Interface;
using TestProject.Model;

namespace TestProject.Helper;

public class IniFileHelperTests : IDisposable
{
    private readonly string _tempFilePath;
    private readonly IIniFileHelper _iniFileHelper;
    
    public IniFileHelperTests()
    {
        _tempFilePath = TestHelper.CreateTempFile();
        _iniFileHelper = new IniFileHelper(_tempFilePath);
    }

    [Fact]
    public void WriteAndReadValue_ShouldReturnSameValue()
    {
        // Arrange
        const string section = "TestSection";
        const string key = "TestKey";
        const string value = "TestValue";
        
        // Act
        _iniFileHelper.WriteValue(section, key, value);
        string result = _iniFileHelper.ReadValue(section, key);
        
        // Assert
        Assert.Equal(value, result);
    }

    [Fact]
    public void ReadSection_ShouldReturnAllValues()
    {
        // Arrange
        const string section = "TestSection";
        var expectedValues = new Dictionary<string, string>()
        {
            ["Key1"] = "Value1",
            ["Key2"] = "Value2",
            ["Key3"] = "Value3",
        };
        
        // Act
        _iniFileHelper.WriteSection(section, expectedValues);
        Thread.Sleep(100);
        var result = _iniFileHelper.ReadSection(section);
        
        // Assert
        Assert.Equal(expectedValues, result);
        foreach (var x in expectedValues)
        {
            Assert.Equal(x.Value, result[x.Key]);
        }
    }

    public void Dispose()
    {
        TestHelper.DeleteTempFile(_tempFilePath);
    }
}