using System;
using System.Collections.Generic;

namespace Voupon.Merchant.WebApp.Entities
{
    public partial class TempUsers
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public string MobileNumber { get; set; }
        public string BusinessName { get; set; }
        public DateTime CreatedAt { get; set; }
        public string TAC { get; set; }
        public DateTime TACRequestedAt { get; set; }
        public DateTime? TACVerifiedAt { get; set; }
        public Guid? UserId { get; set; }
        public int? CountryId { get; set; }
    }
}
