using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.IdentityModel.Tokens;
using SimpleTokenProvider;

namespace GildedRoseWebApplication
{
    public partial class Startup
    {
        // The secret key every token will be signed with.
        // Keep this safe on the server!
        private static readonly string SecretKey = "gilded-rose-rocks!123-secret-key";
        private static readonly string TheIssuer = "gilded-rose-web-api";
        private static readonly string TheAudience = "gilded-rose-web-client";

        private void ConfigureAuth(IApplicationBuilder app)
        {
            var signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(SecretKey));

            app.UseSimpleTokenProvider(new TokenProviderOptions
            {
                Path = "/api/token",
                Audience = TheAudience,
                Issuer = TheIssuer,
                SigningCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256),
                IdentityResolver = GetIdentity
            });

            var tokenValidationParameters = new TokenValidationParameters
            {
                // The signing key must match!
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = signingKey,

                // Validate the JWT Issuer (iss) claim
                ValidateIssuer = true,
                ValidIssuer = TheIssuer,

                // Validate the JWT Audience (aud) claim
                ValidateAudience = true,
                ValidAudience = TheAudience,

                // Validate the token expiry
                ValidateLifetime = true,

                // If you want to allow a certain amount of clock drift, set that here:
                ClockSkew = TimeSpan.Zero
            };

            app.UseJwtBearerAuthentication(new JwtBearerOptions
            {
                AutomaticAuthenticate = true,
                AutomaticChallenge = true,
                TokenValidationParameters = tokenValidationParameters
            });
        }

        private Task<ClaimsIdentity> GetIdentity(string username, string password)
        {
            // Don't do this in production, obviously!
            if (username == "user" && password == "123")
            {
                return Task.FromResult(new ClaimsIdentity(new GenericIdentity(username, "Token"), new Claim[] { }));
            }

            // Credentials are invalid, or account doesn't exist
            return Task.FromResult<ClaimsIdentity>(null);
        }

    }
}
