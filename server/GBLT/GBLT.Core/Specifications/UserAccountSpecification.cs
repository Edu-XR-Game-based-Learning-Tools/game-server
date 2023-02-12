using Ardalis.Specification;
using Core.Entity;

namespace Core.Specification
{
    public sealed class AccountSpecification : Specification<TUserAccount>, ISingleResultSpecification
    {
        public AccountSpecification(AccountType accountType, string accountId)
        {
            Query.Where(r => r.Type == accountType && r.AccountId == accountId);
        }
    }

    public sealed class AccountByMetaDataSpecification : Specification<TUserAccount>, ISingleResultSpecification
    {
        public AccountByMetaDataSpecification(string metaData)
        {
            Query.Where(r => r.MetaData == metaData);
        }
    }

    public sealed class UserAccountSpecification : Specification<TUserAccount>, ISingleResultSpecification
    {
        public UserAccountSpecification(AccountType accountType, string accountId)
        {
            Query
                .Where(r => r.Type == accountType && r.AccountId == accountId)
                .Include(r => r.User);
        }
    }
}