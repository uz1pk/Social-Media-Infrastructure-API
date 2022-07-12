using TweetAPI.Domain;
using Microsoft.AspNetCore.Identity;
using System.IdentityModel.Tokens.Jwt;
using TweetAPI.Options;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace TweetAPI.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly JwtSettings _jwtSettings;
         
        public AuthService(UserManager<IdentityUser> manager, JwtSettings settings)
        {
            _userManager = manager;
            _jwtSettings = settings;
        }

        //Asynchronously register user and generate authetication token (jwt)
        public async Task<AuthResult> RegisterAsync(string email, string password)
        {
            var exists = await _userManager.FindByEmailAsync(email);

            if (exists != null)
            {
                return new AuthResult
                {
                    Errors = new []{"Entered email is already in use"}
                };
            }

            var newRegisteredUser = new IdentityUser
            {
                Email = email,
                UserName = email
            };

            //This auto hashes and salts the password (yay not raw encryption)
            var createdUser = await _userManager.CreateAsync(newRegisteredUser, password);

            if (!createdUser.Succeeded)
            {
                return new AuthResult
                {
                    Errors = createdUser.Errors.Select(x => x.Description)
                };
            }

            var newhandler = new JwtSecurityTokenHandler();
            var tokenCredentialKey = Encoding.ASCII.GetBytes(_jwtSettings.Secret);
            
            //Descriptor for the jwt token
            var tokenDesc = new SecurityTokenDescriptor
            {
                //Claims are a bunch of props/data in the token which gave data about the current user
                //This could be email, name, etc
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, newRegisteredUser.Email),

                    //Jti is a unique id for this jwt
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),

                    new Claim(JwtRegisteredClaimNames.Email, newRegisteredUser.Email),
                    new Claim("id", newRegisteredUser.Id)
                }),

                // Set token expiry
                Expires = DateTime.UtcNow.AddHours(2),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(tokenCredentialKey), SecurityAlgorithms.HmacSha256Signature)
            };

            var generatedToken = newhandler.CreateToken(tokenDesc);

            return new AuthResult
            {
                Success = true,
                Token = newhandler.WriteToken(generatedToken)
            };
        }
    }
}
