using Ardalis.Specification;
using Core.Entity;

namespace Core.Specification
{
    public sealed class UserSpecification : Specification<TUser>, ISingleResultSpecification<TUser>
    {
        public UserSpecification(string identityId)
        {
            Query.Where(u => u.IdentityId == identityId)
                .Include(u => u.RefreshTokens);
        }
    }
}