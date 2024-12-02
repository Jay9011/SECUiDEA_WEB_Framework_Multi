#define REFERENCE_EXISTS

using CryptoManagerNamespace;

namespace TestProject.BLL;

public class CryptoManagerTest
{
#if REFERENCE_EXISTS
    [Fact]
    public void EncAesS1Test()
    {
        // Arrange
        var strTxt = "test";
        
        // Act
        var encAesS1 = CryptoManager.S1.EncAesS1(strTxt);
        
        // Assert
        Assert.NotNull(encAesS1);
        Assert.Equal(strTxt, CryptoManager.S1.DecAesS1(encAesS1));
    }
#endif
}