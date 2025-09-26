#define REFERENCE_EXISTS

using System.Security.Cryptography;
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

    [Fact]
    public void SafetyAES256Test()
    {
        // Arrage
        string txt1 = "test";
        string txt2 = "longlongtexttestlonglongtexttestlonglongtexttestlonglongtexttestlonglongtexttestlonglongtexttestlonglongtexttest";
        string salt = Guid.NewGuid().ToString();

        // Act
        ICryptoManager cryptoManager = new SafetyAES256();
        var encAesS1 = cryptoManager.Encrypt(txt1); // Salt 없이 테스트
        var encAesS2 = cryptoManager.Encrypt(txt2, salt); // Salt 넣고 테스트

        // Assert
        Assert.Equal(txt1, cryptoManager.Decrypt(encAesS1));
        Assert.Equal(txt2, cryptoManager.Decrypt(encAesS2, salt));
    }

    [Fact]
    public void SafetyAES256_ECB_ISO_Test()
    {
        // Arrange
        string txt1 = "test";
        string txt2 = "longlongtexttestlonglongtexttestlonglongtexttestlonglongtexttestlonglongtexttestlonglongtexttestlonglongtexttest";
        string salt = Guid.NewGuid().ToString();

        // Act
        ICryptoManager cryptoManager = new SafetyAES256(CipherMode.ECB, PaddingMode.ISO10126);
        var encAesS1 = cryptoManager.Encrypt(txt1); // Salt 없이 테스트
        var encAesS2 = cryptoManager.Encrypt(txt2, salt); // Salt 넣고 테스트

        // Assert
        Assert.Equal(txt1, cryptoManager.Decrypt(encAesS1));
        Assert.Equal(txt2, cryptoManager.Decrypt(encAesS2, salt));
    }

    [Fact]
    public void SafetySHA512Test()
    {
        // Arrange
        string txt1 = "test";
        string txt2 = "longlongtexttestlonglongtexttestlonglongtexttestlonglongtexttestlonglongtexttestlonglongtexttestlonglongtexttest";
        string salt = Guid.NewGuid().ToString();

        // Act
        ICryptoManager cryptoManager = new SafetySHA512();
        string encSha512S1 = cryptoManager.Encrypt(txt1); // Salt 없이 테스트
        string encSha512S2 = cryptoManager.Encrypt(txt2, salt); // Salt 넣고 테스트

        // Assert
        Assert.Equal(encSha512S1, cryptoManager.Encrypt(txt1));
        Assert.Equal(encSha512S2, cryptoManager.Encrypt(txt2, salt));
    }

#endif
}