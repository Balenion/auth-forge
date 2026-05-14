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
            var bytes = new byte[bits / 8];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes);
        }
    }
}
