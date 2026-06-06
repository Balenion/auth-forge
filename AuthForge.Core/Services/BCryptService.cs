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
        private int _workFactor = 11;
        public int WorkFactor
        {
            get => _workFactor;
            set
            {
                if (value < 4 || value > 31)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "Work Factor для BCrypt должен быть в диапазоне от 4 до 31.");
                }
                _workFactor = value;
            }
        }

        public HashResult Hash(string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentException("Пароль не может быть пустым или null.", nameof(password));
            }

            string complexHash = BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);

            return new HashResult(complexHash, "Embedded");
        }
    }
}
