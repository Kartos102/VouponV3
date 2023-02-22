using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Voupon.Database.Postgres.VodusEntities
{
    public partial class Users
    {
        public Users()
        {
            ClientProfiles = new HashSet<ClientProfiles>();
            MasterMemberProfiles = new HashSet<MasterMemberProfiles>();
            PartnerProfiles = new HashSet<PartnerProfiles>();
            UserClaims = new HashSet<UserClaims>();
            UserLogins = new HashSet<UserLogins>();
            UserRoles = new HashSet<UserRoles>();
        }
        [NotMapped]
        public DateTime? LockoutEndDateUtc { get; set; }
        public string? AuthType { get; set; }
        public string? SocialId { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? ActivationCode { get; set; }
        public DateTime? ActivatedAt { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string? NewPendingVerificationEmail { get; set; }
        
        [NotMapped]
        public DateTime? LockoutEnd { get; set; }
        public DateTime? LastLoggedInAt { get; set; }
        public DateTime? LastCheckedAt { get; set; }

        public virtual ICollection<ClientProfiles> ClientProfiles { get; set; }
        public virtual ICollection<MasterMemberProfiles> MasterMemberProfiles { get; set; }
        public virtual ICollection<PartnerProfiles> PartnerProfiles { get; set; }
        public virtual ICollection<UserClaims> UserClaims { get; set; }
        public virtual ICollection<UserLogins> UserLogins { get; set; }
        public virtual ICollection<UserRoles> UserRoles { get; set; }
    }
}
