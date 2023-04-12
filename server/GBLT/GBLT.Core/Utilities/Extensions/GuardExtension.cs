using Ardalis.GuardClauses;
using Core.Entity;

namespace Core.Utility
{
    public static class GuardExtensions
    {
        public static void NullUser(this IGuardClause _, string walletAddress, TUser user)
        {
            if (user == null)
                throw new UserNotFoundException(walletAddress);
        }

        public static void NullUser(this IGuardClause _, TUser user)
        {
            if (user == null)
                throw new UserIsNull();
        }
    }
}