using Core.Service;
using Core.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Shared.Network;
using System.Net;

namespace RpcService.Controllers
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
            AuthenticationData response = await _authService.Login<AuthenticationData>(request);
            JsonContentResult contentResult = new()
            {
                StatusCode = (int)HttpStatusCode.OK,
                Content = JsonConvert.SerializeObject(response)
            };
            return contentResult;
        }

        // POST api/auth/register
        [HttpPost("register")]
        public async Task<ActionResult> Register([FromBody] RegisterRequest request)
        {
            if (!ModelState.IsValid) { return BadRequest(ModelState); }
            AuthenticationData response = await _authService.Register<AuthenticationData>(request);
            JsonContentResult contentResult = new()
            {
                StatusCode = (int)HttpStatusCode.OK,
                Content = JsonConvert.SerializeObject(response)
            };
            return contentResult;
        }

        // POST api/auth/refreshToken
        [HttpPost("refreshToken")]
        [Authorize]
        public async Task<ActionResult> RefreshToken([FromBody] ExchangeRefreshTokenRequest request)
        {
            if (!ModelState.IsValid) { return BadRequest(ModelState); }
            AuthenticationData response = await _authService.RefreshToken<AuthenticationData>(request);
            JsonContentResult contentResult = new()
            {
                StatusCode = (int)HttpStatusCode.OK,
                Content = JsonConvert.SerializeObject(response)
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