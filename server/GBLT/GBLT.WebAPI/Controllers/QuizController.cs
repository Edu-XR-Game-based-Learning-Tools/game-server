using Core.Service;
using Core.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Network;
using System.Net;
using System.Security.Claims;
using WebAPI.Dto;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class QuizController : ControllerBase
    {
        private readonly IQuizService _quizService;
        private readonly IJwtTokenValidator _jwtTokenValidator;

        public QuizController(
            IQuizService quizService, IJwtTokenValidator jwtTokenValidator)
        {
            _quizService = quizService;
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

        // POST api/quiz/getQuizCollectionList
        [HttpGet("getQuizCollectionList")]
        public async Task<ActionResult> GetQuizCollectionList()
        {
            if (!ModelState.IsValid) { return BadRequest(ModelState); }
            var response = await _quizService.GetQuizCollectionList(GetUserIdentityId());
            JsonContentResult contentResult = new()
            {
                StatusCode = (int)HttpStatusCode.OK,
                Content = JsonSerializer.SerializeObject(response)
            };
            return contentResult;
        }

        // GET api/quiz/getQuizCollection
        [HttpGet("getQuizCollection")]
        public async Task<ActionResult> GetQuizCollection(int id)
        {
            if (!ModelState.IsValid) { return BadRequest(ModelState); }
            var response = await _quizService.GetQuizCollection(id);
            JsonContentResult contentResult = new()
            {
                StatusCode = (int)HttpStatusCode.OK,
                Content = JsonSerializer.SerializeObject(response)
            };
            return contentResult;
        }

        // POST api/quiz/updateQuizCollection
        [HttpPost("updateQuizCollection")]
        public async Task<ActionResult> UpdateQuizCollection([FromBody] QuizCollectionDto request)
        {
            if (!ModelState.IsValid) { return BadRequest(ModelState); }
            var response = await _quizService.UpdateCollection(GetUserIdentityId(), request);
            JsonContentResult contentResult = new()
            {
                StatusCode = (int)HttpStatusCode.OK,
                Content = JsonSerializer.SerializeObject(response)
            };
            return contentResult;
        }

        // POST api/quiz/deleteQuizCollection
        [HttpDelete("deleteQuizCollection")]
        public async Task<ActionResult> DeleteQuizCollection([FromBody] QuizCollectionDto request)
        {
            if (!ModelState.IsValid) { return BadRequest(ModelState); }
            var response = await _quizService.DeleteCollection(request);
            JsonContentResult contentResult = new()
            {
                StatusCode = (int)HttpStatusCode.OK,
                Content = JsonSerializer.SerializeObject(response)
            };
            return contentResult;
        }
    }
}