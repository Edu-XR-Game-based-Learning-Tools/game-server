using Ardalis.Specification;
using Core.Entity;

namespace Core.Specification
{
    public sealed class QuizCollectionOnEid : Specification<TQuizCollection>, ISingleResultSpecification<TQuizCollection>
    {
        public QuizCollectionOnEid(int eid)
        {
            Query.Where(u => u.EId == eid)
                .Include(u => u.Quizzes)
                .Include(u => u.Owner);
        }
    }

    public sealed class QuizOnEid : Specification<TQuiz>, ISingleResultSpecification<TQuiz>
    {
        public QuizOnEid(int eid)
        {
            Query.Where(u => u.EId == eid);
        }
    }

    public sealed class QuizCollectionOnUserSpecification : Specification<TQuizCollection>, ISingleResultSpecification<TQuizCollection>
    {
        public QuizCollectionOnUserSpecification(string userId)
        {
            Query.Where(u => u.Owner.IdentityId == userId)
                .Include(u => u.Quizzes);
        }
    }
}