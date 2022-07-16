using Microsoft.AspNetCore.Mvc;
using TweetAPI.Services;
using TweetAPI.Contracts.v1;
using TweetAPI.Contracts.v1.Requests;
using TweetAPI.Contracts.v1.Responses;
using TweetAPI.Domain;

namespace TweetAPI.Controllers.v1
{
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService service)
        {
            _authService = service;
        }

        [HttpPost(APIRoutes.AuthRoutes.Register)]
        public async Task<IActionResult> Register([FromBody]UserRegistrationRequest req)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new AuthFailureResponse { Errors = ModelState.Values.SelectMany(err => err.Errors.Select(msg => msg.ErrorMessage)) });
            }

            var registrationResposne = await _authService.RegisterAsync(req.Email, req.Password);

            if (!registrationResposne.Success)
            {
                return BadRequest(new AuthFailureResponse { Errors = registrationResposne.Errors });
            }

            return Ok(new AuthSuccessResponse
            {
                Token = registrationResposne.Token,
            });
        }

        [HttpPost(APIRoutes.AuthRoutes.Login)]
        public async Task<IActionResult> Login([FromBody] UserLoginRequest req)
        {
            var registrationResposne = await _authService.LoginAsync(req.Email, req.Password);

            if (!registrationResposne.Success)
            {
                return BadRequest(new AuthFailureResponse { Errors = registrationResposne.Errors });
            }

            return Ok(new AuthSuccessResponse
            {
                Token = registrationResposne.Token,
            });
        }

    }
}
