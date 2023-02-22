using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Voupon.Database.Postgres.RewardsEntities
{
    public partial class Users
    {
        public Users()
        {
            UserClaims = new HashSet<UserClaims>();
            UserLogins = new HashSet<UserLogins>();
            UserRoles = new HashSet<UserRoles>();
            UserTokens = new HashSet<UserTokens>();
        }

        [NotMapped]
        public DateTime? LockoutEnd { get; set; }
      
        public DateTime CreatedAt { get; set; }

        public virtual ICollection<UserClaims> UserClaims { get; set; }
        public virtual ICollection<UserLogins> UserLogins { get; set; }
        public virtual ICollection<UserRoles> UserRoles { get; set; }
        public virtual ICollection<UserTokens> UserTokens { get; set; }
    }
}
