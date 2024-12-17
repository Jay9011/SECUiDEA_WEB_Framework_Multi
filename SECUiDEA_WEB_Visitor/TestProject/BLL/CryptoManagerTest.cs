#define REFERENCE_EXISTS

using CryptoManager;
using CryptoManager.Services;

namespace TestProject.BLL;

public class CryptoManagerTest
{
    public CryptoManagerTest()
    {
    }
    
#if REFERENCE_EXISTS
    [Fact]
    public void EncAesS1Test()
    {
        // Arrange
        var strTxt = "test";
        
        // Act
        ICryptoManager cryptoManager = new S1AES("tLscn/FdlqXhd4Wp");
        var encAesS1 = cryptoManager.Encrypt(strTxt);
        
        // Assert
        Assert.NotNull(encAesS1);
        Assert.Equal(strTxt, cryptoManager.Decrypt(encAesS1));
    }
#endif
}