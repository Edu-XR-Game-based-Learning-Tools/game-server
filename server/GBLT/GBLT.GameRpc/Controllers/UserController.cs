using Ardalis.GuardClauses;
using Core.Entity;
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
    [Authorize]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserDataService _userDataService;
        private readonly IJwtTokenValidator _jwtTokenValidator;

        public UserController(
            IUserDataService userDataService,
            IJwtTokenValidator jwtTokenValidator)
        {
            _userDataService = userDataService;
            _jwtTokenValidator = jwtTokenValidator;
        }

        private string GetUserIdentityId()
        {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", string.Empty);
            var cp = _jwtTokenValidator.GetPrincipalFromToken(token);

            if (cp != null)
            {
                var id = cp.Claims.First(c => c.Type == JwtClaimIdentifiers.Id);
                return id.Value;
            }
            return null;
        }

        // POST api/user/syncUserData
        [HttpGet("syncUserData")]
        public async Task<ActionResult> SyncUserData()
        {
            if (!ModelState.IsValid) { return BadRequest(ModelState); }
            string id = GetUserIdentityId();
            Guard.Against.Null(id);
            TUser user = await _userDataService.Find(id);
            Guard.Against.NullUser(user); UserData userData = new()
            {
                UserId = user.EId,
                UserName = user.UserName,
            };
            JsonContentResult contentResult = new()
            {
                StatusCode = (int)HttpStatusCode.OK,
                Content = JsonConvert.SerializeObject(userData)
            };
            return contentResult;
        }
    }
}