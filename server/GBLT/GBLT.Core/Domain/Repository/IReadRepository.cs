using Ardalis.Specification;

namespace Core.Entity
{

    public interface IReadRepository<T> : IReadRepositoryBase<T> where T : class, IAggregateRoot
    {
    }
}
