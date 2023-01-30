using System;
using Abp;

namespace DispatcherWeb.Authorization.Users.Cache
{
    public class UserCacheItem
    {
        public const string CacheName = "UserCache";
        public long Id { get; set; }
        public int? TenantId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public Guid? ProfilePictureId { get; set; }

        public UserIdentifier ToUserIdentifier()
        {
            return new UserIdentifier(TenantId, Id);
        }

        public string ToUserIdentifierString()
        {
            return ToUserIdentifier().ToUserIdentifierString();
        }
    }
}
