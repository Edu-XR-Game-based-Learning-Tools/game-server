using Ardalis.Specification;

namespace Core.Entity
{

    public interface IRepository<T> : IRepositoryBase<T> where T : class, IAggregateRoot
    {
    }
}
