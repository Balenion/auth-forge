using AuthForge.Core.Interfaces;
using AuthForge.Core.Models;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace AuthForge.Core.Services
{
    public class BuiltInHashService : IPasswordService
    {
        public string AlgorithmName => "PBKDF2 (.NET Built-in)";

        public int Iterations { get; set; } = 100000;

        public HashAlgorithmName SelectedHashAlgorithm { get; set; } = HashAlgorithmName.SHA256;

        public HashResult Hash(string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentException("Пароль не может быть пустым или null.", nameof(password));
            }

            byte[] salt = RandomNumberGenerator.GetBytes(16);

            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
                Encoding.UTF8.GetBytes(password),
                salt,
                Iterations,
                SelectedHashAlgorithm,
                32);

            return new HashResult(
                Convert.ToBase64String(hash),
                Convert.ToBase64String(salt)
            );
        }
    }
}
