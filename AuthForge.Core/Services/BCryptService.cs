using AuthForge.Core.Interfaces;
using AuthForge.Core.Models;
using BCrypt.Net;
using System;
using System.Collections.Generic;
using System.Text;

namespace AuthForge.Core.Services
{
    public class BCryptService : IPasswordService
    {
        public string AlgorithmName => "BCrypt";
        public int WorkFactor { get; set; } = 11;

        public HashResult Hash(string password)
        {
            string complexHash = BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);

            return new HashResult(complexHash, "Embedded");
        }
    }
}
