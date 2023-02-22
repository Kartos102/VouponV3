using System;
using System.Collections.Generic;

namespace Voupon.Database.Postgres.VodusEntities
{
    public partial class Orders
    {
        public Orders()
        {
            OrderItems = new HashSet<OrderItems>();
        }

        public int Id { get; set; }
        public int MasterMemberId { get; set; }
        public DateTime CreatedAt { get; set; }
        public int TotalPoints { get; set; }
        public short NumberOfItems { get; set; }
        public string ContactPersonFirstName { get; set; } = null!;
        public string ContactPersonLastName { get; set; } = null!;
        public string ContactEmail { get; set; } = null!;
        public string ContactMobileNumber { get; set; } = null!;
        public string ContactMobileCountryCode { get; set; } = null!;
        public string AddressLine1 { get; set; } = null!;
        public string? AddressLine2 { get; set; }
        public string Postcode { get; set; } = null!;
        public string City { get; set; } = null!;
        public string State { get; set; } = null!;
        public string CountryName { get; set; } = null!;
        public bool IsDelivered { get; set; }
        public string? DeliveryRemark { get; set; }
        public DateTime? DeliveryLastUpdated { get; set; }
        public string? DeliveryLastUpdatedBy { get; set; }

        public virtual MasterMemberProfiles MasterMember { get; set; } = null!;
        public virtual ICollection<OrderItems> OrderItems { get; set; }
    }
}
