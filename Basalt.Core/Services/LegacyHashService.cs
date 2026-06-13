using Basalt.Core.Interfaces;
using Basalt.Core.Models;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Basalt.Core.Services
{
    public class LegacyHashService : IPasswordService
    {
        public string AlgorithmName => "SHA256 (Legacy)";

        public HashResult Hash(string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentException("Пароль не может быть пустым или null.", nameof(password));
            }

            byte[] salt = RandomNumberGenerator.GetBytes(16);
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
            byte[] combinedBytes = [.. salt, .. passwordBytes];

            byte[] hashBytes = SHA256.HashData(combinedBytes);

            return new HashResult(
                Hash: Convert.ToBase64String(hashBytes),
                Salt: Convert.ToBase64String(salt)
            );
        }

        public HashResult HashPlain(string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentException("Пароль не может быть пустым или null.", nameof(password));
            }

            byte[] hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
            return new HashResult(Convert.ToBase64String(hashBytes), null);
        }
    }
}
