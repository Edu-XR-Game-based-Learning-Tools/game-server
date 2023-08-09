using Shared.Network;

namespace Core.Service
{
    public interface IQuizService
    {
        Task<QuizCollectionListDto> GetQuizCollectionList(string userId);
        Task<QuizCollectionDto> GetQuizCollection(int entityId);

        Task<QuizDto> GetQuiz(int quizId);

        Task<QuizCollectionDto> UpdateCollection(string userId, QuizCollectionDto message);
        Task<QuizCollectionDto> DeleteCollection(QuizCollectionDto message);
    }
}