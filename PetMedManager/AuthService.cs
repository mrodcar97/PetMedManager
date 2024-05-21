using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace PetMedManager
{
    internal class AuthService
    {
        public bool IsAuthenticated()
        {
            return !string.IsNullOrEmpty(SecureStorage.GetAsync("AuthToken").Result);
        }

        public async Task<string> GetAuthTokenAsync()
        {
            return await SecureStorage.GetAsync("AuthToken");
        }

        public async Task<ClaimsPrincipal> GetClaimsPrincipalAsync()
        {
            var token = await GetAuthTokenAsync();
            var handler = new JwtSecurityTokenHandler();
            var tokenS = handler.ReadToken(token) as JwtSecurityToken;
            var claims = tokenS.Claims;

            return new ClaimsPrincipal(new ClaimsIdentity(claims, "jwt"));
        }
    }
}
