using System;
using System.Collections.Generic;

namespace Voupon.Database.MSSQL.VodusEntities
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
        public byte NumberOfItems { get; set; }
        public string ContactPersonFirstName { get; set; }
        public string ContactPersonLastName { get; set; }
        public string ContactEmail { get; set; }
        public string ContactMobileNumber { get; set; }
        public string ContactMobileCountryCode { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string Postcode { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string CountryName { get; set; }
        public bool IsDelivered { get; set; }
        public string DeliveryRemark { get; set; }
        public DateTime? DeliveryLastUpdated { get; set; }
        public string DeliveryLastUpdatedBy { get; set; }

        public virtual ICollection<OrderItems> OrderItems { get; set; }
    }
}
