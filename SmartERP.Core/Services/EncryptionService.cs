using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace SmartERP.Core.Services
{
    /// <summary>
    /// Provides encryption and decryption services for sensitive data
    /// Uses Windows Data Protection API (DPAPI) for machine-level encryption
    /// </summary>
    public interface IEncryptionService
    {
        string Encrypt(string plainText);
        string Decrypt(string encryptedText);
        string EncryptForUser(string plainText);
        string DecryptForUser(string encryptedText);
        byte[] EncryptBytes(byte[] data);
        byte[] DecryptBytes(byte[] encryptedData);
        string HashPassword(string password);
        bool VerifyPassword(string password, string hash);
    }

    public class EncryptionService : IEncryptionService
    {
        private readonly byte[]? _additionalEntropy;

        public EncryptionService(IConfiguration? configuration = null)
        {
            // Optional additional entropy for extra security
            var entropyKey = configuration?["Security:EntropyKey"];
            if (!string.IsNullOrEmpty(entropyKey))
            {
                _additionalEntropy = Encoding.UTF8.GetBytes(entropyKey);
            }
        }

        /// <summary>
        /// Encrypts data using DPAPI with LocalMachine scope
        /// Data can be decrypted by any user on the same machine
        /// </summary>
        public string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                return string.Empty;

            try
            {
                var data = Encoding.UTF8.GetBytes(plainText);
                var encrypted = ProtectedData.Protect(data, _additionalEntropy, DataProtectionScope.LocalMachine);
                return Convert.ToBase64String(encrypted);
            }
            catch (Exception ex)
            {
                throw new CryptographicException("Encryption failed", ex);
            }
        }

        /// <summary>
        /// Decrypts data encrypted with LocalMachine scope
        /// </summary>
        public string Decrypt(string encryptedText)
        {
            if (string.IsNullOrEmpty(encryptedText))
                return string.Empty;

            try
            {
                var data = Convert.FromBase64String(encryptedText);
                var decrypted = ProtectedData.Unprotect(data, _additionalEntropy, DataProtectionScope.LocalMachine);
                return Encoding.UTF8.GetString(decrypted);
            }
            catch (Exception ex)
            {
                throw new CryptographicException("Decryption failed", ex);
            }
        }

        /// <summary>
        /// Encrypts data using DPAPI with CurrentUser scope
        /// Data can only be decrypted by the same user who encrypted it
        /// </summary>
        public string EncryptForUser(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                return string.Empty;

            try
            {
                var data = Encoding.UTF8.GetBytes(plainText);
                var encrypted = ProtectedData.Protect(data, _additionalEntropy, DataProtectionScope.CurrentUser);
                return Convert.ToBase64String(encrypted);
            }
            catch (Exception ex)
            {
                throw new CryptographicException("User-level encryption failed", ex);
            }
        }

        /// <summary>
        /// Decrypts data encrypted with CurrentUser scope
        /// </summary>
        public string DecryptForUser(string encryptedText)
        {
            if (string.IsNullOrEmpty(encryptedText))
                return string.Empty;

            try
            {
                var data = Convert.FromBase64String(encryptedText);
                var decrypted = ProtectedData.Unprotect(data, _additionalEntropy, DataProtectionScope.CurrentUser);
                return Encoding.UTF8.GetString(decrypted);
            }
            catch (Exception ex)
            {
                throw new CryptographicException("User-level decryption failed", ex);
            }
        }

        /// <summary>
        /// Encrypts byte array data
        /// </summary>
        public byte[] EncryptBytes(byte[] data)
        {
            if (data == null || data.Length == 0)
                return Array.Empty<byte>();

            return ProtectedData.Protect(data, _additionalEntropy, DataProtectionScope.LocalMachine);
        }

        /// <summary>
        /// Decrypts byte array data
        /// </summary>
        public byte[] DecryptBytes(byte[] encryptedData)
        {
            if (encryptedData == null || encryptedData.Length == 0)
                return Array.Empty<byte>();

            return ProtectedData.Unprotect(encryptedData, _additionalEntropy, DataProtectionScope.LocalMachine);
        }

        /// <summary>
        /// Hashes password using BCrypt (already used in your app)
        /// </summary>
        public string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password, BCrypt.Net.BCrypt.GenerateSalt(12));
        }

        /// <summary>
        /// Verifies password against BCrypt hash
        /// </summary>
        public bool VerifyPassword(string password, string hash)
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
    }

    /// <summary>
    /// AES encryption for sensitive fields (alternative to DPAPI)
    /// Use when you need portable encryption (data moves between machines)
    /// </summary>
    public class AesEncryptionService
    {
        private readonly byte[] _key;
        private readonly byte[] _iv;

        public AesEncryptionService(string secretKey)
        {
            // Derive key and IV from secret key using PBKDF2
            using var deriveBytes = new Rfc2898DeriveBytes(
                secretKey,
                Encoding.UTF8.GetBytes("SmartERP_Salt_2024"),
                100000,
                HashAlgorithmName.SHA256);

            _key = deriveBytes.GetBytes(32); // 256-bit key
            _iv = deriveBytes.GetBytes(16);  // 128-bit IV
        }

        public string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                return string.Empty;

            using var aes = Aes.Create();
            aes.Key = _key;
            aes.IV = _iv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using var encryptor = aes.CreateEncryptor();
            var plainBytes = Encoding.UTF8.GetBytes(plainText);
            var encryptedBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

            return Convert.ToBase64String(encryptedBytes);
        }

        public string Decrypt(string encryptedText)
        {
            if (string.IsNullOrEmpty(encryptedText))
                return string.Empty;

            using var aes = Aes.Create();
            aes.Key = _key;
            aes.IV = _iv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using var decryptor = aes.CreateDecryptor();
            var encryptedBytes = Convert.FromBase64String(encryptedText);
            var decryptedBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);

            return Encoding.UTF8.GetString(decryptedBytes);
        }
    }
}
