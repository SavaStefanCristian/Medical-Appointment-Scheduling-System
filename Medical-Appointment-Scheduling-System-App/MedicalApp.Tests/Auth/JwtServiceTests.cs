using Medical_Appointment_Scheduling_System_App.Services;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using Xunit;

namespace MedicalApp.Tests.Auth
{
    public class JwtServiceTests
    {
        private readonly JwtService _jwtService;

        public JwtServiceTests()
        {
            IConfiguration config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "Jwt:Issuer", "TestIssuer" },
                    { "Jwt:Audience", "TestAudience" },
                    { "Jwt:SecurityKey", "SuperSecretKeyForTesting1234567890_MustBeLongEnough!" },
                    { "Jwt:AccessTokenExpiryMinutes", "60" }
                }!)
                .Build();

            _jwtService = new JwtService(config);
        }

        [Fact]
        // PASS: GenerateToken returns a non-empty JWT string
        // FAIL: Token generation broken — all logins fail
        public void GenerateToken_ValidInputs_ReturnsNonEmptyString()
        {
            var token = _jwtService.GenerateToken("1", "user@test.com", "Patient");
            Assert.NotNull(token);
            Assert.NotEmpty(token);
        }

        [Fact]
        // PASS: Token contains "sub" claim matching the userId passed in
        // FAIL: Backend cannot identify which user made a request
        public void GenerateToken_ValidInputs_ContainsSubClaim()
        {
            var token = _jwtService.GenerateToken("42", "user@test.com", "Patient");
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
            var sub = jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub);

            Assert.NotNull(sub);
            Assert.Equal("42", sub.Value);
        }

        [Fact]
        // PASS: Token contains a claim with value "Doctor" somewhere in the claims list
        // FAIL: [Authorize(Roles = "Doctor")] returns 403 for valid doctors
        public void GenerateToken_DoctorRole_ContainsDoctorRoleClaim()
        {
            var token = _jwtService.GenerateToken("1", "doc@test.com", "Doctor");
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

            // Role claim can appear under different type URIs depending on the JWT library version
            var hasRoleClaim = jwt.Claims.Any(c => c.Value == "Doctor");

            Assert.True(hasRoleClaim);
        }

        [Fact]
        // PASS: Tokens for two different users are different strings
        // FAIL: All users get the same token — anyone can impersonate anyone
        public void GenerateToken_DifferentUsers_ReturnDifferentTokens()
        {
            var t1 = _jwtService.GenerateToken("1", "a@test.com", "Patient");
            var t2 = _jwtService.GenerateToken("2", "b@test.com", "Doctor");
            Assert.NotEqual(t1, t2);
        }

        [Fact]
        // PASS: Token has a future expiry date (not already expired)
        // FAIL: Token expires immediately — every request fails with 401
        public void GenerateToken_ValidInputs_TokenHasFutureExpiry()
        {
            var token = _jwtService.GenerateToken("1", "user@test.com", "Patient");
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
            Assert.True(jwt.ValidTo > DateTime.UtcNow);
        }
    }
}