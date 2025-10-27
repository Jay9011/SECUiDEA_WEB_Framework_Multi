#define REFERENCE_EXISTS

using CryptoManager;
using CryptoManager.Services;
using System.Security.Cryptography;

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

    [Fact]
    public void SafetyRSA_BasicEncryptDecryptTest()
    {
        // Arrange
        string txt1 = "test";
        string txt2 = "Hello RSA Encryption!";

        // Act
        using (SafetyRSA cryptoManager = new SafetyRSA())
        {
            string encrypted1 = cryptoManager.Encrypt(txt1);
            string encrypted2 = cryptoManager.Encrypt(txt2);

            // Assert
            Assert.NotNull(encrypted1);
            Assert.NotNull(encrypted2);
            Assert.NotEqual(txt1, encrypted1); // 암호화되어야 함
            Assert.NotEqual(txt2, encrypted2);
            Assert.Equal(txt1, cryptoManager.Decrypt(encrypted1));
            Assert.Equal(txt2, cryptoManager.Decrypt(encrypted2));
        }
    }

    [Fact]
    public void SafetyRSA_WithSaltTest()
    {
        // Arrange
        string txt = "test with salt";
        string salt = Guid.NewGuid().ToString();

        // Act
        using (SafetyRSA cryptoManager = new SafetyRSA())
        {
            string encrypted = cryptoManager.Encrypt(txt, salt);

            // Assert
            Assert.NotNull(encrypted);
            Assert.Equal(txt, cryptoManager.Decrypt(encrypted, salt));
            Assert.Equal(txt, cryptoManager.Decrypt(encrypted)); // RSA는 salt를 무시함
        }
    }

    [Fact]
    public void SafetyRSA_LongTextTest()
    {
        // Arrange
        string longText = "This is a longer text for RSA encryption testing. RSA has size limits.";

        // Act & Assert
        using (SafetyRSA cryptoManager = new SafetyRSA())
        {
            // MaxDataSize 내의 데이터는 암호화 가능
            if (longText.Length <= cryptoManager.MaxDataSize)
            {
                string encrypted = cryptoManager.Encrypt(longText);
                Assert.Equal(longText, cryptoManager.Decrypt(encrypted));
            }
        }
    }

    [Fact]
    public void SafetyRSA_DataTooLargeTest()
    {
        // Arrange
        using (SafetyRSA cryptoManager = new SafetyRSA(2048))
        {
            // RSA 2048-bit OAEP SHA256: 최대 190바이트
            string tooLargeText = new string('A', cryptoManager.MaxDataSize + 1);

            // Act & Assert
            Assert.Throws<ArgumentException>(() => cryptoManager.Encrypt(tooLargeText));
        }
    }

    [Fact]
    public void SafetyRSA_KeyExportAndImportTest()
    {
        // Arrange
        string txt = "test key export";
        string encrypted;
        string publicKey;
        string privateKey;

        // Act
        using (SafetyRSA cryptoManager1 = new SafetyRSA())
        {
            encrypted = cryptoManager1.Encrypt(txt);
            publicKey = cryptoManager1.ExportPublicKey();
            privateKey = cryptoManager1.ExportPrivateKey();
        }

        // Assert - 개인키로 새 인스턴스 생성하여 복호화
        using (SafetyRSA cryptoManager2 = new SafetyRSA(privateKey))
        {
            string decrypted = cryptoManager2.Decrypt(encrypted);
            Assert.Equal(txt, decrypted);
        }

        // Assert - 공개키로는 복호화 불가
        using (SafetyRSA cryptoManager3 = new SafetyRSA(publicKey))
        {
            Assert.Throws<InvalidOperationException>(() => cryptoManager3.Decrypt(encrypted));
        }
    }

    [Fact]
    public void SafetyRSA_RandomnessTest()
    {
        // Arrange
        string txt = "test randomness";

        // Act
        using (SafetyRSA cryptoManager = new SafetyRSA())
        {
            string encrypted1 = cryptoManager.Encrypt(txt);
            string encrypted2 = cryptoManager.Encrypt(txt);

            // Assert - RSA OAEP는 매번 다른 암호문 생성 (랜덤 패딩)
            Assert.NotEqual(encrypted1, encrypted2);
            Assert.Equal(txt, cryptoManager.Decrypt(encrypted1));
            Assert.Equal(txt, cryptoManager.Decrypt(encrypted2));
        }
    }

    [Fact]
    public void SafetyHybridRSA_BasicEncryptDecryptTest()
    {
        // Arrange
        string txt1 = "test";
        string txt2 = "Hello Hybrid RSA Encryption!";

        // Act
        using (SafetyHybridRSA cryptoManager = new SafetyHybridRSA())
        {
            string encrypted1 = cryptoManager.Encrypt(txt1);
            string encrypted2 = cryptoManager.Encrypt(txt2);

            // Assert
            Assert.NotNull(encrypted1);
            Assert.NotNull(encrypted2);
            Assert.NotEqual(txt1, encrypted1);
            Assert.NotEqual(txt2, encrypted2);
            Assert.Equal(txt1, cryptoManager.Decrypt(encrypted1));
            Assert.Equal(txt2, cryptoManager.Decrypt(encrypted2));
        }
    }

    [Fact]
    public void SafetyHybridRSA_WithSaltTest()
    {
        // Arrange
        string txt = "test with salt";
        string salt = Guid.NewGuid().ToString();

        // Act
        using (SafetyHybridRSA cryptoManager = new SafetyHybridRSA())
        {
            string encrypted = cryptoManager.Encrypt(txt, salt);

            // Assert
            Assert.NotNull(encrypted);
            Assert.Equal(txt, cryptoManager.Decrypt(encrypted, salt));
            Assert.Equal(txt, cryptoManager.Decrypt(encrypted)); // Hybrid는 salt를 무시함
        }
    }

    [Fact]
    public void SafetyHybridRSA_LargeDataTest()
    {
        // Arrange - SafetyRSA보다 훨씬 긴 텍스트 (RSA 제한을 초과)
        string largeText = new string('A', 5000); // 5KB

        // Act
        using (SafetyHybridRSA cryptoManager = new SafetyHybridRSA())
        {
            string encrypted = cryptoManager.Encrypt(largeText);
            string decrypted = cryptoManager.Decrypt(encrypted);

            // Assert
            Assert.NotNull(encrypted);
            Assert.Equal(largeText, decrypted);
        }
    }

    [Fact]
    public void SafetyHybridRSA_VeryLongTextTest()
    {
        // Arrange
        string veryLongText = "longlongtext".PadRight(10000, 'X'); // 10KB

        // Act
        using (SafetyHybridRSA cryptoManager = new SafetyHybridRSA())
        {
            string encrypted = cryptoManager.Encrypt(veryLongText);
            string decrypted = cryptoManager.Decrypt(encrypted);

            // Assert
            Assert.Equal(veryLongText.Length, decrypted.Length);
            Assert.Equal(veryLongText, decrypted);
        }
    }

    [Fact]
    public void SafetyHybridRSA_KeyExportAndImportTest()
    {
        // Arrange
        string txt = "test key export";
        string encrypted;
        string publicKey;
        string privateKey;

        // Act
        using (SafetyHybridRSA cryptoManager1 = new SafetyHybridRSA())
        {
            encrypted = cryptoManager1.Encrypt(txt);
            publicKey = cryptoManager1.ExportPublicKey();
            privateKey = cryptoManager1.ExportPrivateKey();
        }

        // Assert - 개인키로 새 인스턴스 생성하여 복호화
        using (SafetyHybridRSA cryptoManager2 = new SafetyHybridRSA(privateKey))
        {
            string decrypted = cryptoManager2.Decrypt(encrypted);
            Assert.Equal(txt, decrypted);
        }

        // Assert - 공개키로는 복호화 불가
        using (SafetyHybridRSA cryptoManager3 = new SafetyHybridRSA(publicKey))
        {
            Assert.Throws<InvalidOperationException>(() => cryptoManager3.Decrypt(encrypted));
        }
    }

    [Fact]
    public void SafetyHybridRSA_PublicKeyEncryptionTest()
    {
        // Arrange
        string txt = "test public key encryption";
        string publicKey;
        string privateKey;

        // Act - 키 쌍 생성
        using (SafetyHybridRSA cryptoManager1 = new SafetyHybridRSA())
        {
            publicKey = cryptoManager1.ExportPublicKey();
            privateKey = cryptoManager1.ExportPrivateKey();
        }

        // Act - 공개키로 암호화
        string encrypted;
        using (SafetyHybridRSA cryptoManager2 = new SafetyHybridRSA(publicKey))
        {
            encrypted = cryptoManager2.Encrypt(txt);
        }

        // Act - 개인키로 복호화
        using (SafetyHybridRSA cryptoManager3 = new SafetyHybridRSA(privateKey))
        {
            string decrypted = cryptoManager3.Decrypt(encrypted);

            // Assert
            Assert.Equal(txt, decrypted);
        }
    }

    [Fact]
    public void SafetyHybridRSA_RandomnessTest()
    {
        // Arrange
        string txt = "test randomness";

        // Act
        using (SafetyHybridRSA cryptoManager = new SafetyHybridRSA())
        {
            string encrypted1 = cryptoManager.Encrypt(txt);
            string encrypted2 = cryptoManager.Encrypt(txt);
            string encrypted3 = cryptoManager.Encrypt(txt);

            // Assert - 매번 다른 AES 키 사용으로 매번 다른 암호문 생성
            Assert.NotEqual(encrypted1, encrypted2);
            Assert.NotEqual(encrypted2, encrypted3);
            Assert.NotEqual(encrypted1, encrypted3);

            // 모두 정상적으로 복호화
            Assert.Equal(txt, cryptoManager.Decrypt(encrypted1));
            Assert.Equal(txt, cryptoManager.Decrypt(encrypted2));
            Assert.Equal(txt, cryptoManager.Decrypt(encrypted3));
        }
    }

    [Fact]
    public void SafetyHybridRSA_KeySizeTest()
    {
        // Arrange & Act
        using (SafetyHybridRSA cryptoManager2048 = new SafetyHybridRSA(2048))
        using (SafetyHybridRSA cryptoManager4096 = new SafetyHybridRSA(4096))
        {
            string txt = "test key sizes";

            string encrypted2048 = cryptoManager2048.Encrypt(txt);
            string encrypted4096 = cryptoManager4096.Encrypt(txt);

            // Assert
            Assert.Equal(2048, cryptoManager2048.KeySize);
            Assert.Equal(4096, cryptoManager4096.KeySize);
            Assert.Equal(txt, cryptoManager2048.Decrypt(encrypted2048));
            Assert.Equal(txt, cryptoManager4096.Decrypt(encrypted4096));
        }
    }

    [Fact]
    public void SafetyRSA_And_SafetyHybridRSA_ComparisonTest()
    {
        // Arrange
        string shortText = "short";
        string mediumText = new string('M', 150); // RSA 제한 내
        string longText = new string('L', 1000); // RSA 제한 초과

        // Act & Assert - SafetyRSA
        using (SafetyRSA rsaManager = new SafetyRSA())
        {
            // 짧은 텍스트 OK
            string encrypted1 = rsaManager.Encrypt(shortText);
            Assert.Equal(shortText, rsaManager.Decrypt(encrypted1));

            // 중간 텍스트 OK (MaxDataSize 내)
            string encrypted2 = rsaManager.Encrypt(mediumText);
            Assert.Equal(mediumText, rsaManager.Decrypt(encrypted2));

            // 긴 텍스트는 예외 발생
            Assert.Throws<ArgumentException>(() => rsaManager.Encrypt(longText));
        }

        // Act & Assert - SafetyHybridRSA
        using (SafetyHybridRSA hybridManager = new SafetyHybridRSA())
        {
            // 모든 크기 OK
            string encrypted1 = hybridManager.Encrypt(shortText);
            string encrypted2 = hybridManager.Encrypt(mediumText);
            string encrypted3 = hybridManager.Encrypt(longText);

            Assert.Equal(shortText, hybridManager.Decrypt(encrypted1));
            Assert.Equal(mediumText, hybridManager.Decrypt(encrypted2));
            Assert.Equal(longText, hybridManager.Decrypt(encrypted3));
        }
    }

#endif
}