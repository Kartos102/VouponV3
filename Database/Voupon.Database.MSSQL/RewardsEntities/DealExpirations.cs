using System;
using System.Collections.Generic;

namespace Voupon.Database.MSSQL.RewardsEntities
{
    public partial class DealExpirations
    {
        public DealExpirations()
        {
            CartProducts = new HashSet<CartProducts>();
            Products = new HashSet<Products>();
        }

        public int Id { get; set; }
        public int ProductId { get; set; }
        public int? ExpirationTypeId { get; set; }
        public int? TotalValidDays { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? ExpiredDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid CreatedByUserId { get; set; }
        public DateTime? LastUpdatedAt { get; set; }
        public Guid? LastUpdatedByUserId { get; set; }

        public virtual ExpirationTypes ExpirationType { get; set; }
        public virtual Products Product { get; set; }
        public virtual ICollection<CartProducts> CartProducts { get; set; }
        public virtual ICollection<Products> Products { get; set; }
    }
}
