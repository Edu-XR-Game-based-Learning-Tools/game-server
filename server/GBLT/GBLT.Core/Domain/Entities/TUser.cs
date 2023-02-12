using MessagePack;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace Core.Entity
{
    public enum UserRole
    {
        Player,
        Tester
    }

    [Index(nameof(UserId))]
    public class TUser : BaseEntity, IAggregateRoot
    {
        public string UserId { get; set; } // Unique Id for each User in client
        public string Name { get; set; }
        public UserRole Role { get; set; }

        [IgnoreMember]
        public ICollection<TUserAccount> Accounts { get; set; }

        public TUser()
        {
            Role = UserRole.Player;
        }
    }
}