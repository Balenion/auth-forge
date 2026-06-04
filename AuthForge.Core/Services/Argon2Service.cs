using AuthForge.Core.Interfaces;
using AuthForge.Core.Models;
using Konscious.Security.Cryptography;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace AuthForge.Core.Services
{
    public class Argon2Service : IPasswordService
    {
        public string AlgorithmName => "Argon2id";
        public int Iterations { get; set; } = 4;
        public int MemoryKb { get; set; } = 65536;
        public int Parallelism { get; set; } = 8;

        public HashResult Hash(string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentException("Пароль не может быть пустым или null.", nameof(password));
            }

            // Генерируем соль
            var salt = RandomNumberGenerator.GetBytes(16);

            using var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password));
            argon2.Salt = salt;
            argon2.DegreeOfParallelism = Parallelism;
            argon2.Iterations = Iterations;
            argon2.MemorySize = MemoryKb;

            var hash = argon2.GetBytes(32);

            return new HashResult(
                Convert.ToBase64String(hash),
                Convert.ToBase64String(salt)
            );
        }
    }
}
