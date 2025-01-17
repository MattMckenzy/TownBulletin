﻿using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using Triggered.Models;

namespace Triggered.Services
{
    /// <summary>
    /// A singleton service that defines methods to encrypt and decrypt sensitive text.
    /// </summary>
    public class EncryptionService
    {
        private readonly IDbContextFactory<TriggeredDbContext> _dbContextFactory;
        private readonly IConfiguration _configuration;
        private readonly MessagingService _messagingService;

        /// <summary>
        /// Default contructor with injected services.
        /// </summary>
        /// <param name="dbContextFactory">Injected <see cref="IDbContextFactory{TContext}"/> of <see cref="TriggeredDbContext"/>.</param>
        /// <param name="configuration">Injected <see cref="IConfiguration"/>.</param>
        /// <param name="messagingService">Injected <see cref="Services.MessagingService"/>.</param>
        public EncryptionService(IDbContextFactory<TriggeredDbContext> dbContextFactory, IConfiguration configuration, MessagingService messagingService)
        {
            _dbContextFactory = dbContextFactory;
            _configuration = configuration;
            _messagingService = messagingService;
        }

        /// <summary>
        /// Encrypts the given <paramref name="value"/> <see cref="string"/> with the given <see cref="Vector"/> <paramref name="key"/>.
        /// </summary>
        /// <param name="key">The unique identifier of the saved <see cref="Vector"/>.</param>
        /// <param name="value">The text to encrypt.</param>
        /// <returns>The encrypted <see cref="string"/>.</returns>
        public async Task<string> Encrypt(string key, string value)
        {
            using Aes aes = Aes.Create();
            aes.Key = GetKey(_configuration["EncryptionKey"]);
            aes.Padding = PaddingMode.PKCS7;

            await SaveVector(key, Convert.ToBase64String(aes.IV));
            try
            {
                using MemoryStream encryptionStream = new();
                using CryptoStream cryptoStream = new(encryptionStream, aes.CreateEncryptor(), CryptoStreamMode.Write);
                using (StreamWriter encryptWriter = new(cryptoStream))
                {
                    encryptWriter.Write(value);
                }
                return Convert.ToBase64String(encryptionStream.ToArray());

            }
            catch (Exception _) when (_ is FormatException || _ is CryptographicException)
            {
                await _messagingService.AddMessage($"Could not encrypt {key}.", MessageCategory.Authentication, LogLevel.Error);
                return string.Empty;
            }
        }

        /// <summary>
        /// Decrypts the given <paramref name="value"/> <see cref="string"/> with the given <see cref="Vector"/> <paramref name="key"/>.
        /// </summary>
        /// <param name="key">The unique identifier of the saved <see cref="Vector"/>.</param>
        /// <param name="value">The text to decrypt.</param>
        /// <returns>The decrypted <see cref="string"/>.</returns>
        public async Task<string> Decrypt(string key, string value)
        {
            using TriggeredDbContext triggeredDbContext = await _dbContextFactory.CreateDbContextAsync();

            using Aes aes = Aes.Create();
            aes.Key = GetKey(_configuration["EncryptionKey"]);
            aes.Padding = PaddingMode.PKCS7;

            string? IV = triggeredDbContext.Vectors.FirstOrDefault(vector => vector.Key == key)?.Value;
            if (string.IsNullOrWhiteSpace(IV))
                return string.Empty;

            aes.IV = Convert.FromBase64String(IV);
            try
            {
                using MemoryStream decryptionStream = new(Convert.FromBase64String(value));
                using CryptoStream cryptoStream = new(decryptionStream, aes.CreateDecryptor(), CryptoStreamMode.Read);
                using StreamReader decryptReader = new(cryptoStream);
                return decryptReader.ReadToEnd();
            }
            catch (Exception _) when (_ is FormatException || _ is CryptographicException)
            {
                await _messagingService.AddMessage($"Could not decrypt {key}.", MessageCategory.Authentication, LogLevel.Error);
                return string.Empty;
            }
        }

        private async Task SaveVector(string key, string value)
        {
            using TriggeredDbContext triggeredDbContext = await _dbContextFactory.CreateDbContextAsync();

            Vector? vector = triggeredDbContext.Vectors.FirstOrDefault(setting => setting.Key.Equals(key));
            if (vector == null)
            {
                triggeredDbContext.Vectors.Add(new Vector(key, value)).Context.SaveChanges();
            }
            else
            {
                vector.Value = value;
                triggeredDbContext.Vectors.Update(vector);
            }

            await triggeredDbContext.SaveChangesAsync();
        }

        private static byte[] GetKey(string passcode, int keyBytes = 32)
        {
            Rfc2898DeriveBytes keyGenerator = new(passcode, Salt, Iterations);
            return keyGenerator.GetBytes(keyBytes);        
        }    

        private static readonly byte[] Salt = new byte[] { 45, 07, 10, 55, 72, 60, 32, 77 };
        private static readonly int Iterations = 121;
    }
}
