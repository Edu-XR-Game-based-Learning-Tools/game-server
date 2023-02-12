using Ardalis.Specification;
using Core.Entity;
using System.Linq;

namespace Core.Specification
{
    public sealed class UserSpecification : Specification<TUser>, ISingleResultSpecification
    {
        public UserSpecification(int userId)
        {
            Query.Where(u => u.Id == userId);
        }

        public UserSpecification(string typeAccountId)
        {
            Query.Where(u => u.Accounts.Any(r => r.AccountId == typeAccountId));
        }
    }
}