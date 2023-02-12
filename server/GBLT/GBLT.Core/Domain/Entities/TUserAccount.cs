using Microsoft.EntityFrameworkCore;
using System;

namespace Core.Entity
{
    public enum AccountType
    {
        NONE = -1,
        PASSWORD = 0,
        GOOGLE = 1,
        FACEBOOK = 2,
        FIREBASE = 3
    }

    [Index(nameof(AccountId))]
    [Index(nameof(Type), nameof(AccountId))]
    public class TUserAccount : BaseEntity, IAggregateRoot
    {
        public AccountType Type { get; set; }
        public string AccountId { get; set; }
        public string MetaData { get; set; }
        public DateTime? LastLogin { get; set; }
        public int? UserId { get; set; }
        public TUser User { get; set; }

        public TUserAccount(AccountType type, string accountId)
        {
            Type = type;
            AccountId = accountId;
        }

        public TUserAccount(AccountType type, string accountId, int userId, string metaData)
        {
            Type = type;
            AccountId = accountId;
            UserId = userId;
            MetaData = metaData;
        }
    }
}