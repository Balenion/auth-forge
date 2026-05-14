using AuthForge.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace AuthForge.Core.Interfaces
{
    public interface IPasswordService
    {
        // Название алгоритма
        string AlgorithmName { get; }

        // Основной метод хеширования
        HashResult Hash(string password);
    }
}
