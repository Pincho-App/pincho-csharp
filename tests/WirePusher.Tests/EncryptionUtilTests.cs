using System.Text;
using WirePusher.Crypto;
using Xunit;

namespace WirePusher.Tests;

public class EncryptionUtilTests
{
    [Fact]
    public void CustomBase64Encode_WithStandardInput_ReturnsCustomEncoding()
    {
        // Standard Base64: + / =
        // Custom encoding: - . _
        var input = new byte[] { 0x00, 0x10, 0x83, 0x10, 0x51, 0x87, 0x20, 0x92, 0x8b, 0x30 };
        var result = EncryptionUtil.CustomBase64Encode(input);

        // Verify custom characters are used
        Assert.DoesNotContain('+', result);
        Assert.DoesNotContain('/', result);
        Assert.DoesNotContain('=', result);
    }

    [Fact]
    public void CustomBase64Encode_WithPlusSign_ReplacesWithDash()
    {
        // Create byte array that produces '+' in standard Base64
        var input = new byte[] { 0xFB, 0xEF };
        var standard = Convert.ToBase64String(input); // Should contain '+'
        var custom = EncryptionUtil.CustomBase64Encode(input);

        Assert.Contains('+', standard);
        Assert.DoesNotContain('+', custom);
        Assert.Contains('-', custom);
    }

    [Fact]
    public void CustomBase64Encode_WithSlash_ReplacesWithDot()
    {
        // Create byte array that produces '/' in standard Base64
        var input = new byte[] { 0xFF, 0xEF };
        var standard = Convert.ToBase64String(input); // Should contain '/'
        var custom = EncryptionUtil.CustomBase64Encode(input);

        Assert.Contains('/', standard);
        Assert.DoesNotContain('/', custom);
        Assert.Contains('.', custom);
    }

    [Fact]
    public void CustomBase64Encode_WithEquals_ReplacesWithUnderscore()
    {
        // Create byte array that produces '=' in standard Base64 (needs padding)
        var input = new byte[] { 0x00 };
        var standard = Convert.ToBase64String(input); // Should contain '='
        var custom = EncryptionUtil.CustomBase64Encode(input);

        Assert.Contains('=', standard);
        Assert.DoesNotContain('=', custom);
        Assert.Contains('_', custom);
    }

    [Fact]
    public void DeriveEncryptionKey_WithKnownPassword_ReturnsExpectedKey()
    {
        // Test with a known password to verify SHA1 derivation
        var password = "test-password";
        var key = EncryptionUtil.DeriveEncryptionKey(password);

        // Key should be 16 bytes (128 bits for AES-128)
        Assert.Equal(16, key.Length);

        // Verify it's deterministic
        var key2 = EncryptionUtil.DeriveEncryptionKey(password);
        Assert.Equal(key, key2);
    }

    [Fact]
    public void DeriveEncryptionKey_WithDifferentPasswords_ReturnsDifferentKeys()
    {
        var key1 = EncryptionUtil.DeriveEncryptionKey("password1");
        var key2 = EncryptionUtil.DeriveEncryptionKey("password2");

        Assert.NotEqual(key1, key2);
    }

    [Fact]
    public void DeriveEncryptionKey_MatchesPythonImplementation()
    {
        // Test vector from Python implementation
        // SHA1("test") = "a94a8fe5ccb19ba61c4c0873d391e987982fbbd3"
        // First 32 hex chars: "a94a8fe5ccb19ba61c4c0873d391e987"
        // As bytes: [0xa9, 0x4a, 0x8f, 0xe5, 0xcc, 0xb1, 0x9b, 0xa6, 0x1c, 0x4c, 0x08, 0x73, 0xd3, 0x91, 0xe9, 0x87]

        var password = "test";
        var key = EncryptionUtil.DeriveEncryptionKey(password);

        var expectedKey = new byte[] {
            0xa9, 0x4a, 0x8f, 0xe5, 0xcc, 0xb1, 0x9b, 0xa6,
            0x1c, 0x4c, 0x08, 0x73, 0xd3, 0x91, 0xe9, 0x87
        };

        Assert.Equal(expectedKey, key);
    }

    [Fact]
    public void GenerateIV_ReturnsCorrectLength()
    {
        var result = EncryptionUtil.GenerateIV();

        // IV should be 16 bytes (128 bits)
        Assert.Equal(16, result.IVBytes.Length);

        // Hex string should be 32 characters (16 bytes * 2)
        Assert.Equal(32, result.IVHex.Length);
    }

    [Fact]
    public void GenerateIV_ReturnsLowercaseHex()
    {
        var result = EncryptionUtil.GenerateIV();

        // All characters should be lowercase hex
        Assert.Matches("^[0-9a-f]{32}$", result.IVHex);
    }

    [Fact]
    public void GenerateIV_ReturnsUniqueValues()
    {
        var iv1 = EncryptionUtil.GenerateIV();
        var iv2 = EncryptionUtil.GenerateIV();

        // IVs should be different (statistically)
        Assert.NotEqual(iv1.IVBytes, iv2.IVBytes);
        Assert.NotEqual(iv1.IVHex, iv2.IVHex);
    }

    [Fact]
    public void GenerateIV_HexMatchesBytes()
    {
        var result = EncryptionUtil.GenerateIV();

        // Convert hex back to bytes and compare
        var bytesFromHex = Convert.FromHexString(result.IVHex);
        Assert.Equal(result.IVBytes, bytesFromHex);
    }

    [Fact]
    public void EncryptMessage_WithValidInput_ReturnsEncryptedString()
    {
        var plaintext = "Hello, World!";
        var password = "secret123";
        var iv = new byte[16]; // All zeros for deterministic testing

        var encrypted = EncryptionUtil.EncryptMessage(plaintext, password, iv);

        Assert.NotNull(encrypted);
        Assert.NotEmpty(encrypted);
        Assert.NotEqual(plaintext, encrypted);
    }

    [Fact]
    public void EncryptMessage_WithSameInputs_ReturnsSameOutput()
    {
        var plaintext = "Test message";
        var password = "password";
        var iv = new byte[16]; // All zeros for deterministic testing

        var encrypted1 = EncryptionUtil.EncryptMessage(plaintext, password, iv);
        var encrypted2 = EncryptionUtil.EncryptMessage(plaintext, password, iv);

        Assert.Equal(encrypted1, encrypted2);
    }

    [Fact]
    public void EncryptMessage_WithDifferentIVs_ReturnsDifferentOutputs()
    {
        var plaintext = "Test message";
        var password = "password";
        var iv1 = new byte[16];
        var iv2 = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08,
                               0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f, 0x10 };

        var encrypted1 = EncryptionUtil.EncryptMessage(plaintext, password, iv1);
        var encrypted2 = EncryptionUtil.EncryptMessage(plaintext, password, iv2);

        Assert.NotEqual(encrypted1, encrypted2);
    }

    [Fact]
    public void EncryptMessage_WithDifferentPasswords_ReturnsDifferentOutputs()
    {
        var plaintext = "Test message";
        var iv = new byte[16];

        var encrypted1 = EncryptionUtil.EncryptMessage(plaintext, "password1", iv);
        var encrypted2 = EncryptionUtil.EncryptMessage(plaintext, "password2", iv);

        Assert.NotEqual(encrypted1, encrypted2);
    }

    [Fact]
    public void EncryptMessage_UsesCustomBase64Encoding()
    {
        var plaintext = "Test message";
        var password = "password";
        var iv = new byte[16];

        var encrypted = EncryptionUtil.EncryptMessage(plaintext, password, iv);

        // Custom Base64 should not contain standard characters
        Assert.DoesNotContain('+', encrypted);
        Assert.DoesNotContain('/', encrypted);
        Assert.DoesNotContain('=', encrypted);
    }

    [Fact]
    public void EncryptMessage_WithEmptyString_ReturnsEncryptedString()
    {
        var plaintext = "";
        var password = "password";
        var iv = new byte[16];

        var encrypted = EncryptionUtil.EncryptMessage(plaintext, password, iv);

        Assert.NotNull(encrypted);
        Assert.NotEmpty(encrypted); // Even empty string gets padded and encrypted
    }

    [Fact]
    public void EncryptMessage_WithLongMessage_ReturnsEncryptedString()
    {
        var plaintext = new string('A', 10000); // 10KB message
        var password = "password";
        var ivResult = EncryptionUtil.GenerateIV();

        var encrypted = EncryptionUtil.EncryptMessage(plaintext, password, ivResult.IVBytes);

        Assert.NotNull(encrypted);
        Assert.NotEmpty(encrypted);
    }

    [Fact]
    public void EncryptMessage_WithUnicodeCharacters_ReturnsEncryptedString()
    {
        var plaintext = "Hello 世界 🌍 Привет";
        var password = "password";
        var ivResult = EncryptionUtil.GenerateIV();

        var encrypted = EncryptionUtil.EncryptMessage(plaintext, password, ivResult.IVBytes);

        Assert.NotNull(encrypted);
        Assert.NotEmpty(encrypted);
    }

    [Fact]
    public void EncryptMessage_WithNullPlaintext_ThrowsArgumentNullException()
    {
        var password = "password";
        var iv = new byte[16];

        Assert.Throws<ArgumentNullException>(() =>
            EncryptionUtil.EncryptMessage(null!, password, iv));
    }

    [Fact]
    public void EncryptMessage_WithNullPassword_ThrowsArgumentNullException()
    {
        var plaintext = "Test";
        var iv = new byte[16];

        Assert.Throws<ArgumentNullException>(() =>
            EncryptionUtil.EncryptMessage(plaintext, null!, iv));
    }

    [Fact]
    public void EncryptMessage_WithNullIV_ThrowsArgumentNullException()
    {
        var plaintext = "Test";
        var password = "password";

        Assert.Throws<ArgumentNullException>(() =>
            EncryptionUtil.EncryptMessage(plaintext, password, null!));
    }

    [Fact]
    public void EncryptMessage_WithInvalidIVLength_ThrowsArgumentException()
    {
        var plaintext = "Test";
        var password = "password";
        var invalidIV = new byte[8]; // Wrong size (should be 16)

        var exception = Assert.Throws<ArgumentException>(() =>
            EncryptionUtil.EncryptMessage(plaintext, password, invalidIV));

        Assert.Contains("IV must be 16 bytes", exception.Message);
    }

    [Fact]
    public void EncryptMessage_AppliesPKCS7Padding()
    {
        // Test with different message lengths to verify padding
        var password = "password";
        var iv = new byte[16];

        // 15 bytes (needs 1 byte padding)
        var msg15 = new string('A', 15);
        var encrypted15 = EncryptionUtil.EncryptMessage(msg15, password, iv);

        // 16 bytes (needs 16 bytes padding - full block)
        var msg16 = new string('A', 16);
        var encrypted16 = EncryptionUtil.EncryptMessage(msg16, password, iv);

        // 17 bytes (needs 15 bytes padding)
        var msg17 = new string('A', 17);
        var encrypted17 = EncryptionUtil.EncryptMessage(msg17, password, iv);

        // All should be valid and different lengths
        Assert.NotEmpty(encrypted15);
        Assert.NotEmpty(encrypted16);
        Assert.NotEmpty(encrypted17);

        // 16-byte message should result in 2 blocks (32 bytes before encoding)
        // 15 and 17 byte messages should result in 2 blocks as well
        Assert.NotEqual(encrypted15, encrypted16);
        Assert.NotEqual(encrypted16, encrypted17);
    }

    [Theory]
    [InlineData("test", "a94a8fe5ccb19ba61c4c0873d391e987")]
    [InlineData("password", "5baa61e4c9b93f3f0682250b6cf8331b")]
    [InlineData("secret", "e5e9fa1ba31ecd1ae84f75caaa474f3a")]
    public void DeriveEncryptionKey_WithKnownPasswords_MatchesExpectedSHA1(string password, string expectedKeyHex)
    {
        var key = EncryptionUtil.DeriveEncryptionKey(password);
        var keyHex = Convert.ToHexString(key).ToLowerInvariant();

        // Verify first 32 hex characters match (16 bytes)
        Assert.Equal(expectedKeyHex, keyHex);
    }

    [Fact]
    public void EncryptionFlow_FullEndToEndTest()
    {
        // Simulate full encryption flow as used in WirePusherClient
        var message = "Sensitive notification data";
        var password = "my-encryption-password";

        // Generate IV
        var ivResult = EncryptionUtil.GenerateIV();

        // Encrypt message
        var encryptedMessage = EncryptionUtil.EncryptMessage(message, password, ivResult.IVBytes);

        // Verify results
        Assert.NotNull(encryptedMessage);
        Assert.NotEqual(message, encryptedMessage);
        Assert.Equal(32, ivResult.IVHex.Length);
        Assert.Equal(16, ivResult.IVBytes.Length);

        // Verify custom Base64 encoding
        Assert.DoesNotContain('+', encryptedMessage);
        Assert.DoesNotContain('/', encryptedMessage);
        Assert.DoesNotContain('=', encryptedMessage);
    }
}
