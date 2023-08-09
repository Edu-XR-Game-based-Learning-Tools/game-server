using Core.Entity;
using Core.Specification;
using Shared;

namespace Core.Service
{
    public class QuizDataService : IQuizDataService
    {
        private readonly IRepository<TQuizCollection> _quizCollectionRepository;
        private readonly IRepository<TQuiz> _quizRepository;
        private readonly IRedisDataService _redisDataService;

        public QuizDataService(
            IRepository<TQuizCollection> quizCollectionRepository,
            IRepository<TQuiz> quizRepository,
            IRedisDataService redisDataService)
        {
            _quizCollectionRepository = quizCollectionRepository;
            _quizRepository = quizRepository;
            _redisDataService = redisDataService;
        }

        public async Task<TQuizCollection> CreateCollection(TQuizCollection collection)
        {
            var result = await _quizCollectionRepository.AddAsync(collection);
            return result;
        }

        public async Task<TQuizCollection[]> FindCollectionOnUser(string userId)
        {
            var result = await _quizCollectionRepository.ListAsync(new QuizCollectionOnUserSpecification(userId));
            return result.ToArray();
        }

        public async Task<TQuizCollection> FindCollection(int entityId)
        {
            var result = await _quizCollectionRepository.FirstOrDefaultAsync(new QuizCollectionOnEid(entityId));
            return result;
        }

        public async Task<TQuiz> Find(int entityId)
        {
            var result = await _quizRepository.FirstOrDefaultAsync(new QuizOnEid(entityId));
            return result;
        }

        public async Task<TQuizCollection> UpdateCollection(TQuizCollection collection)
        {
            int[] newQuizEIds = collection.Quizzes.Select(quiz => quiz.EId).ToArray();
            await _quizCollectionRepository.UpdateAsync(collection);
            var collectionFromDb = await FindCollection(collection.EId);
            if (collectionFromDb.Quizzes.Count > newQuizEIds.Length)
            {
                await _quizRepository.DeleteRangeAsync(collectionFromDb.Quizzes.Where(quiz => !newQuizEIds.Contains(quiz.EId)));
            }
            return collection;
        }

        public async Task<TQuizCollection> DeleteCollection(TQuizCollection collection)
        {
            await _quizCollectionRepository.DeleteAsync(collection);
            return collection;
        }
    }
}