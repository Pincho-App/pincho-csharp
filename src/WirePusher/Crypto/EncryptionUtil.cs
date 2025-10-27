using System.Security.Cryptography;
using System.Text;

namespace WirePusher.Crypto;

/// <summary>
/// Utility class for AES-128-CBC encryption matching WirePusher app decryption.
/// </summary>
/// <remarks>
/// <para>
/// This class provides client-side encryption for notification messages using:
/// </para>
/// <list type="bullet">
/// <item><description>AES-128-CBC encryption</description></item>
/// <item><description>SHA1-based key derivation</description></item>
/// <item><description>PKCS7 padding</description></item>
/// <item><description>Custom Base64 encoding</description></item>
/// </list>
/// <para><strong>Important Security Notes:</strong></para>
/// <list type="bullet">
/// <item><description>Only the message body is encrypted; title, type, tags, and URLs remain unencrypted</description></item>
/// <item><description>Password must match the type configuration in the WirePusher app</description></item>
/// <item><description>Password is never sent to the API (used only for local encryption)</description></item>
/// <item><description>Each message uses a unique randomly generated initialization vector (IV)</description></item>
/// </list>
/// </remarks>
/// <example>
/// <code>
/// // Generate IV
/// var (ivBytes, ivHex) = EncryptionUtil.GenerateIV();
///
/// // Encrypt message
/// var encrypted = EncryptionUtil.EncryptMessage(
///     "Sensitive data",
///     "secret-password",
///     ivBytes
/// );
///
/// // Use encrypted message and IV hex in notification
/// </code>
/// </example>
public static class EncryptionUtil
{
    private const int IVLength = 16; // 128 bits
    private const int KeyLength = 16; // 128 bits

    /// <summary>
    /// Result object containing both IV bytes and hexadecimal string.
    /// </summary>
    public record IVResult(byte[] IVBytes, string IVHex);

    /// <summary>
    /// Encodes bytes using custom Base64 encoding matching WirePusher app.
    /// </summary>
    /// <remarks>
    /// Converts standard Base64 characters:
    /// <list type="bullet">
    /// <item><description>'+' → '-'</description></item>
    /// <item><description>'/' → '.'</description></item>
    /// <item><description>'=' → '_'</description></item>
    /// </list>
    /// </remarks>
    /// <param name="data">Bytes to encode.</param>
    /// <returns>Custom Base64 encoded string.</returns>
    public static string CustomBase64Encode(byte[] data)
    {
        var standard = Convert.ToBase64String(data);
        return standard
            .Replace('+', '-')
            .Replace('/', '.')
            .Replace('=', '_');
    }

    /// <summary>
    /// Derives AES encryption key from password using SHA1.
    /// </summary>
    /// <remarks>
    /// <para>Key derivation process:</para>
    /// <list type="number">
    /// <item><description>SHA1 hash of password</description></item>
    /// <item><description>Lowercase hexadecimal string</description></item>
    /// <item><description>Truncate to 32 characters</description></item>
    /// <item><description>Convert hex string to bytes</description></item>
    /// </list>
    /// <para>Returns 16-byte AES-128 key.</para>
    /// </remarks>
    /// <param name="password">The encryption password.</param>
    /// <returns>16-byte AES-128 encryption key.</returns>
    public static byte[] DeriveEncryptionKey(string password)
    {
        using var sha1 = SHA1.Create();
        var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(password));

        // Convert to lowercase hex and truncate to 32 characters
        var hashHex = Convert.ToHexString(hash).ToLowerInvariant();
        var keyHex = hashHex[..32];

        // Convert hex string to bytes
        return Convert.FromHexString(keyHex);
    }

    /// <summary>
    /// Encrypts a message using AES-128-CBC with custom Base64 encoding.
    /// </summary>
    /// <remarks>
    /// <para>Encryption process matching WirePusher app:</para>
    /// <list type="number">
    /// <item><description>Derive key from password using SHA1</description></item>
    /// <item><description>Apply PKCS7 padding to plaintext</description></item>
    /// <item><description>Encrypt using AES-128-CBC with provided IV</description></item>
    /// <item><description>Encode with custom Base64</description></item>
    /// </list>
    /// </remarks>
    /// <param name="plaintext">The message to encrypt.</param>
    /// <param name="password">The encryption password.</param>
    /// <param name="iv">16-byte initialization vector.</param>
    /// <returns>Encrypted and custom Base64 encoded string.</returns>
    /// <exception cref="ArgumentNullException">Thrown when parameters are null.</exception>
    /// <exception cref="ArgumentException">Thrown when IV length is not 16 bytes.</exception>
    public static string EncryptMessage(string plaintext, string password, byte[] iv)
    {
        ArgumentNullException.ThrowIfNull(plaintext);
        ArgumentNullException.ThrowIfNull(password);
        ArgumentNullException.ThrowIfNull(iv);

        if (iv.Length != IVLength)
            throw new ArgumentException($"IV must be {IVLength} bytes", nameof(iv));

        var key = DeriveEncryptionKey(password);

        using var aes = Aes.Create();
        aes.Key = key;
        aes.IV = iv;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        using var encryptor = aes.CreateEncryptor();
        var plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
        var encrypted = encryptor.TransformFinalBlock(plaintextBytes, 0, plaintextBytes.Length);

        return CustomBase64Encode(encrypted);
    }

    /// <summary>
    /// Generates a random 16-byte initialization vector.
    /// </summary>
    /// <remarks>
    /// Returns IV bytes and hexadecimal string representation (32 characters).
    /// </remarks>
    /// <returns>IVResult containing IV bytes and hex string.</returns>
    public static IVResult GenerateIV()
    {
        var iv = new byte[IVLength];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(iv);

        var ivHex = Convert.ToHexString(iv).ToLowerInvariant();
        return new IVResult(iv, ivHex);
    }
}
