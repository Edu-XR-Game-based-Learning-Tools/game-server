using Ardalis.Specification;
using Core.Entity;

namespace Core.Specification
{
    public sealed class MetaSpecification : Specification<TMeta>, ISingleResultSpecification
    {
        public MetaSpecification(string metaKey)
        {
            Query.Where(m => m.MetaKey == metaKey);
        }
    }
}