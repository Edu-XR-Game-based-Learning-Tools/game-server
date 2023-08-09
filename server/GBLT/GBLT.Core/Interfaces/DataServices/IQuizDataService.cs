using Core.Entity;
using Shared.Network;

namespace Core.Service
{
    public interface IQuizDataService
    {
        Task<TQuizCollection> CreateCollection(TQuizCollection collection);

        Task<TQuizCollection[]> FindCollectionOnUser(string userId);
        Task<TQuizCollection> FindCollection(int entityId);
        Task<TQuiz> Find(int entityId);

        Task<TQuizCollection> UpdateCollection(TQuizCollection collection);
        Task<TQuizCollection> DeleteCollection(TQuizCollection collection);
    }
}