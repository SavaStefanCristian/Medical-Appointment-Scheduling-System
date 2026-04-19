using Medical_Appointment_Scheduling_System_App.Utilities;
using Xunit;

namespace MedicalApp.Tests.Auth
{
    public class PasswordHasherTests
    {
        [Fact]
        // PASS: HashPassword returns a non-empty string
        // FAIL: Returns null or empty — hashing function broken
        public void HashPassword_ValidInput_ReturnsNonEmptyHash()
        {
            var hash = PasswordHasher.HashPassword("TestPass123");
            Assert.NotNull(hash);
            Assert.NotEmpty(hash);
        }

        [Fact]
        // PASS: Hash is not equal to the original plaintext password
        // FAIL: Plaintext stored — critical security bug
        public void HashPassword_ValidInput_DoesNotReturnPlaintext()
        {
            var password = "TestPass123";
            Assert.NotEqual(password, PasswordHasher.HashPassword(password));
        }

        [Fact]
        // PASS: Same input always produces the same hash (deterministic SHA256)
        // FAIL: Hash is random — VerifyPassword will never match
        public void HashPassword_SameInput_ReturnsSameHash()
        {
            Assert.Equal(
                PasswordHasher.HashPassword("Consistent!"),
                PasswordHasher.HashPassword("Consistent!")
            );
        }

        [Fact]
        // PASS: Different passwords produce different hashes
        // FAIL: Hash collision — users can log in with any password
        public void HashPassword_DifferentPasswords_ProduceDifferentHashes()
        {
            Assert.NotEqual(
                PasswordHasher.HashPassword("Password1"),
                PasswordHasher.HashPassword("Password2")
            );
        }

        [Fact]
        // PASS: Returns true when password matches stored hash
        // FAIL: Correct password rejected — users locked out
        public void VerifyPassword_CorrectPassword_ReturnsTrue()
        {
            var hash = PasswordHasher.HashPassword("SecurePass99");
            Assert.True(PasswordHasher.VerifyPassword("SecurePass99", hash));
        }

        [Fact]
        // PASS: Returns false for wrong password
        // FAIL: Wrong password accepted — authentication bypass
        public void VerifyPassword_WrongPassword_ReturnsFalse()
        {
            var hash = PasswordHasher.HashPassword("CorrectPass");
            Assert.False(PasswordHasher.VerifyPassword("WrongPass", hash));
        }

        [Fact]
        // PASS: Returns false for empty string
        // FAIL: Empty string accepted as valid password
        public void VerifyPassword_EmptyString_ReturnsFalse()
        {
            var hash = PasswordHasher.HashPassword("SomePass");
            Assert.False(PasswordHasher.VerifyPassword("", hash));
        }

        [Fact]
        // PASS: ArgumentException thrown for null input
        // FAIL: NullReferenceException crashes the app instead of a handled error
        public void HashPassword_NullInput_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => PasswordHasher.HashPassword(null!));
        }
    }
}
