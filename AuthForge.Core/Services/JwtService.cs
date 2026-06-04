using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace AuthForge.Core.Services
{
    public class JwtService
    {
        public string GenerateSecretKey(int bits = 512)
        {
            if (bits <= 0)
            {
                throw new ArgumentException("Количество бит должно быть больше нуля.", nameof(bits));
            }

            var bytes = new byte[bits / 8];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes);
        }
    }
}
