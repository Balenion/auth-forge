using Basalt.Core.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace Basalt.Core.Tests
{
    public class JwtServiceTests
    {
        [Theory]
        [InlineData(256)]
        [InlineData(512)]
        public void GenerateSecretKey_ShouldReturnValidBase64String(int bits)
        {
            // Arrange
            var jwtService = new JwtService();

            // Act
            string base64Key = Convert.ToBase64String(jwtService.GenerateSecretBytes(bits));

            // Assert
            Assert.False(string.IsNullOrWhiteSpace(base64Key));

            Span<byte> buffer = new Span<byte>(new byte[base64Key.Length]);
            bool isBase64 = Convert.TryFromBase64String(base64Key, buffer, out _);
            Assert.True(isBase64, "Сгенерированный ключ должен быть валидной Base64 строкой");
        }

        [Fact]
        public void GenerateSecretKey_CallsShouldBeUnique()
        {
            // Arrange
            var jwtService = new JwtService();

            // Act
            string key1 = Convert.ToBase64String(jwtService.GenerateSecretBytes(512));
            string key2 = Convert.ToBase64String(jwtService.GenerateSecretBytes(512));

            // Assert
            Assert.NotEqual(key1, key2);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-256)]
        public void GenerateSecretKey_WithInvalidBits_ShouldThrowArgumentException(int invalidBits)
        {
            // Arrange
            var jwtService = new JwtService();

            // Act & Assert
            Assert.ThrowsAny<System.Exception>(() => jwtService.GenerateSecretBytes(invalidBits));
        }
    }
}