using Core.Service;
using Core.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Network;
using System.Net;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(
            IAuthService authService)
        {
            _authService = authService;
        }

        // POST api/auth/login
        [HttpPost("login")]
        public async Task<ActionResult> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid) { return BadRequest(ModelState); }
            LoginResponse response = await _authService.Login<LoginResponse>(request);
            JsonContentResult contentResult = new()
            {
                StatusCode = (int)HttpStatusCode.OK,
                Content = JsonSerializer.SerializeObject(response)
            };
            return contentResult;
        }

        // POST api/auth/register
        [HttpPost("register")]
        public async Task<ActionResult> Register([FromBody] RegisterRequest request)
        {
            if (!ModelState.IsValid) { return BadRequest(ModelState); }
            LoginResponse response = await _authService.Register<LoginResponse>(request);
            JsonContentResult contentResult = new()
            {
                StatusCode = (int)HttpStatusCode.OK,
                Content = JsonSerializer.SerializeObject(response)
            };
            return contentResult;
        }

        // POST api/auth/refreshToken
        [HttpPost("refreshToken")]
        [Authorize]
        public async Task<ActionResult> RefreshToken([FromBody] ExchangeRefreshTokenRequest request)
        {
            if (!ModelState.IsValid) { return BadRequest(ModelState); }
            LoginResponse response = await _authService.RefreshToken<LoginResponse>(request);
            JsonContentResult contentResult = new()
            {
                StatusCode = (int)HttpStatusCode.OK,
                Content = JsonSerializer.SerializeObject(response)
            };
            return contentResult;
        }

        // GET api/auth/GetTest
        [HttpGet("GetTest")]
        public ActionResult GetTest()
        {
            return new JsonContentResult
            {
                StatusCode = (int)HttpStatusCode.OK,
                Content = "Test"
            };
        }
    }
}