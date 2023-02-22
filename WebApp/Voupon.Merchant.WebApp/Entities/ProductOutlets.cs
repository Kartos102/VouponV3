using System;
using System.Collections.Generic;

namespace Voupon.Merchant.WebApp.Entities
{
    public partial class ProductOutlets
    {
        public int ProductId { get; set; }
        public int OutletId { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid CreatedByUserId { get; set; }
        public DateTime? LastUpdatedAt { get; set; }
        public Guid? LastUpdatedByUserId { get; set; }

        public virtual Outlets Outlet { get; set; }
        public virtual Products Product { get; set; }
    }
}
