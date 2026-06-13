using Basalt.Core.Interfaces;
using Basalt.Core.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace Basalt.Core.Tests
{
    public class PasswordServicesTests
    {
        public static IEnumerable<object[]> GetPasswordServices()
        {
            yield return new object[] { new Argon2Service() };
            yield return new object[] { new BCryptService() };
            yield return new object[] { new BuiltInHashService() };
            yield return new object[] { new LegacyHashService() };
        }

        [Theory]
        [MemberData(nameof(GetPasswordServices))]
        public void Hash_ShouldReturnValidHashAndCorrectAlgorithmName(IPasswordService service)
        {
            // Arrange
            string password = "SecurePassword123!";

            // Act
            var result = service.Hash(password);

            // Assert
            Assert.NotNull(result);
            Assert.False(string.IsNullOrWhiteSpace(result.Hash));
            Assert.NotEqual(password, result.Hash);
            Assert.NotNull(service.AlgorithmName);
        }

        [Theory]
        [MemberData(nameof(GetPasswordServices))]
        public void Hash_SamePassword_ShouldGenerateDifferentHashesAndSalts(IPasswordService service)
        {
            // Arrange
            string password = "TestPassword";

            // Act
            var result1 = service.Hash(password);
            var result2 = service.Hash(password);

            // Assert
            Assert.NotEqual(result1.Hash, result2.Hash);

            if (service.AlgorithmName != "BCrypt")
            {
                Assert.NotEqual(result1.Salt, result2.Salt);
            }
        }

        [Fact]
        public void LegacyHashPlain_ShouldNotUseSalt()
        {
            // Arrange
            var legacyService = new LegacyHashService();
            string password = "PlainPassword";

            // Act
            var result = legacyService.HashPlain(password);

            // Assert
            Assert.Null(result.Salt);
            Assert.False(string.IsNullOrWhiteSpace(result.Hash));
        }

        [Theory]
        [MemberData(nameof(GetPasswordServices))]
        public void Hash_WithNullOrEmptyPassword_ShouldThrowArgumentException(IPasswordService service)
        {
            // Assert & Act
            Assert.ThrowsAny<System.ArgumentException>(() => service.Hash(null!));
            Assert.ThrowsAny<System.ArgumentException>(() => service.Hash(""));
        }
    }
}
