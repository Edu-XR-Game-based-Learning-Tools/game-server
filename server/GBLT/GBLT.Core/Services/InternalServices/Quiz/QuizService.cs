using AutoMapper;
using Core.Entity;
using Microsoft.Extensions.Logging;
using Shared.Network;

namespace Core.Service
{
    public class QuizService : IQuizService
    {
        private readonly ILogger<QuizService> _logger;
        private readonly IMapper _mapper;
        private readonly IQuizDataService _quizDataService;
        private readonly IUserDataService _userDataService;

        public QuizService(
            ILogger<QuizService> logger,
            IMapper mapper,
            IQuizDataService quizDataService,
            IUserDataService userDataService)
        {
            _logger = logger;
            _mapper = mapper;
            _quizDataService = quizDataService;
            _userDataService = userDataService;
        }

        public async Task<QuizCollectionListDto> GetQuizCollectionList(string userId)
        {
            try
            {
                var collections = await _quizDataService.FindCollectionOnUser(userId);
                var response = new QuizCollectionListDto
                {
                    Collections = collections.Select(_mapper.Map<QuizCollectionDto>).OrderBy(ele => ele.EId).ToArray(),
                };
                return response;
            }
            catch (Exception ex)
            {
                return new QuizCollectionListDto { Success = false, Message = ex.Message };
            }
        }

        public async Task<QuizCollectionDto> GetQuizCollection(int entityId)
        {
            try
            {
                var collection = await _quizDataService.FindCollection(entityId);
                QuizCollectionDto response = _mapper.Map<QuizCollectionDto>(collection);
                return response;
            }
            catch (Exception ex)
            {
                return new QuizCollectionDto { Success = false, Message = ex.Message };
            }
        }

        public async Task<QuizDto> GetQuiz(int quizId)
        {
            try
            {
                var quiz = await _quizDataService.Find(quizId);
                var response = _mapper.Map<QuizDto>(quiz);
                return response;
            }
            catch (Exception ex)
            {
                return new QuizDto { Success = false, Message = ex.Message };
            }
        }

        public async Task<QuizCollectionDto> UpdateCollection(string userId, QuizCollectionDto message)
        {
            try
            {
                TUser user = await _userDataService.Find(userId);
                TQuizCollection collection = _mapper.Map<TQuizCollection>(message);
                if (message.EId == null)
                {
                    collection.Owner = user;
                    collection = await _quizDataService.CreateCollection(collection);
                }
                else
                    collection = await _quizDataService.UpdateCollection(collection);
                var response = _mapper.Map<QuizCollectionDto>(collection);
                return response;
            }
            catch (Exception ex)
            {
                return new QuizCollectionDto { Success = false, Message = ex.Message };
            }
        }

        public async Task<QuizCollectionDto> DeleteCollection(QuizCollectionDto message)
        {
            try
            {
                var collection = await _quizDataService.DeleteCollection(_mapper.Map<TQuizCollection>(message));
                var response = _mapper.Map<QuizCollectionDto>(collection);
                return response;
            }
            catch (Exception ex)
            {
                return new QuizCollectionDto { Success = false, Message = ex.Message };
            }
        }
    }
}