using System;
using System.Collections.Generic;

namespace Voupon.Database.MSSQL.RewardsEntities
{
    public partial class DiscountTypes
    {
        public DiscountTypes()
        {
            ProductDiscounts = new HashSet<ProductDiscounts>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsActivated { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid CreatedByUserId { get; set; }
        public DateTime? LastUpdatedAt { get; set; }
        public Guid? LastUpdatedByUserId { get; set; }

        public virtual ICollection<ProductDiscounts> ProductDiscounts { get; set; }
    }
}
