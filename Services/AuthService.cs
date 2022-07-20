using TweetAPI.Domain;
using Microsoft.AspNetCore.Identity;
using System.IdentityModel.Tokens.Jwt;
using TweetAPI.Options;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using TweetAPI.Data;
using Microsoft.EntityFrameworkCore;

namespace TweetAPI.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly JwtSettings _jwtSettings;
        private readonly DataContext _dbContext;
        private readonly TokenValidationParameters _tokenValidationPrep;

        private const string LoginErrorMessage = "Invalid Username or Password";


        public AuthService(UserManager<IdentityUser> manager, JwtSettings settings, TokenValidationParameters tokenParams, DataContext dbModels)
        {
            _userManager = manager;
            _jwtSettings = settings;
            _tokenValidationPrep = tokenParams;
            _dbContext = dbModels;
        }

        public async Task<AuthResult> LoginAsync(string email, string password)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                return new AuthResult { Errors = new[] { LoginErrorMessage } };
            }

            var userCheckedPass = await _userManager.CheckPasswordAsync(user, password);

            if (!userCheckedPass)
            {
                return new AuthResult { Errors = new[] { LoginErrorMessage } };
            }

            return await UserAuthResultAsync(user);
        }

        //Asynchronously register user and generate authetication token (jwt)
        public async Task<AuthResult> RegisterAsync(string email, string password)
        {
            var exists = await _userManager.FindByEmailAsync(email);

            if (exists != null)
            {
                return new AuthResult { Errors = new[] { "Entered email is already in use" } };
            }

            var newRegisteredUser = new IdentityUser
            {
                Email = email,
                UserName = email
            };

            //This auto hashes and salts the password (yay not raw encryption)
            var createdUser = await _userManager.CreateAsync(newRegisteredUser, password);

            if (!createdUser.Succeeded)
                return new AuthResult { Errors = createdUser.Errors.Select(x => x.Description) };

            return await UserAuthResultAsync(newRegisteredUser);
        }

        private async Task<AuthResult> UserAuthResultAsync(IdentityUser User)
        {
            var newhandler = new JwtSecurityTokenHandler();
            var tokenCredentialKey = Encoding.ASCII.GetBytes(_jwtSettings.Secret);

            //Descriptor for the jwt token
            var tokenDesc = new SecurityTokenDescriptor
            {
                //Claims are a bunch of props/data in the token which gave data about the current user
                //This could be email, name, etc
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, User.Email),

                    //Jti is a unique id for this jwt
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),

                    new Claim(JwtRegisteredClaimNames.Email, User.Email),
                    new Claim("id", User.Id)
                }),
                // Set token expiry
                Expires = DateTime.UtcNow.Add(_jwtSettings.TokenTimeLimit),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(tokenCredentialKey), SecurityAlgorithms.HmacSha256Signature)
            };

            var generatedToken = newhandler.CreateToken(tokenDesc);

            var newRefresh = new RefreshToken
            {
                Token = Guid.NewGuid().ToString(),
                JwtId = generatedToken.Id,
                UserId = User.Id,
                CreationDate = DateTime.UtcNow,
                Expiration = DateTime.UtcNow.AddMonths(6)
            };

            await _dbContext.RefreshTokens.AddAsync(newRefresh);
            await _dbContext.SaveChangesAsync();

            return new AuthResult
            {
                Success = true,
                Token = newhandler.WriteToken(generatedToken),
                RefreshToken = newRefresh.Token
            };
        }

        public async Task<AuthResult> RefreshAsync(string token, string RefreshToken)
        {
            var validToken = GetTokenPrincipals(token);

            if (validToken == null)
            {
                return new AuthResult { Errors = new[] { "Invalid token" } };
            }

            var expiry = long.Parse(validToken.Claims.Single(x => x.Type == JwtRegisteredClaimNames.Exp).Value);

            var utcExpiry = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(expiry);
            if (utcExpiry > DateTime.UtcNow)
            {
                return new AuthResult { Errors = new[] { "Invalid token" } };
            }

            var jti = validToken.Claims.Single(x => x.Type == JwtRegisteredClaimNames.Jti).Value;

            var dbRefreshToken = await _dbContext.RefreshTokens.SingleOrDefaultAsync(x => x.Token.Equals(RefreshToken));
            if (dbRefreshToken == null || DateTime.UtcNow > dbRefreshToken.Expiration || dbRefreshToken.IsInvalid || dbRefreshToken.IsUsed || !dbRefreshToken.JwtId.Equals(jti))
            {
                return new AuthResult { Errors = new[] { "Invalid token" } };
            }

            dbRefreshToken.IsUsed = true;
            _dbContext.RefreshTokens.Update(dbRefreshToken);
            await _dbContext.SaveChangesAsync();

            var user = await _userManager.FindByIdAsync(validToken.Claims.Single(x => x.Type == "id").Value);

            return await UserAuthResultAsync(user);
        }

        private ClaimsPrincipal GetTokenPrincipals(string token)
        {
            var handler = new JwtSecurityTokenHandler();

            try
            {
                var principals = handler.ValidateToken(token, _tokenValidationPrep, out var validated);
                if (!TokenSecurityCheck(validated))
                {
                    return null;
                }
                return principals;
            }
            catch
            {
                return null;
            }
        }

        private bool TokenSecurityCheck(SecurityToken token)
        {
            return (token is JwtSecurityToken secureToken) && secureToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256Signature, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
