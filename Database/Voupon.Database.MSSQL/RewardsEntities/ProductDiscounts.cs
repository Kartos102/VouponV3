using System;
using System.Collections.Generic;

namespace Voupon.Database.MSSQL.RewardsEntities
{
    public partial class ProductDiscounts
    {
        public ProductDiscounts()
        {
            CartProducts = new HashSet<CartProducts>();
        }

        public int Id { get; set; }
        public int ProductId { get; set; }
        public string Name { get; set; }
        public int DiscountTypeId { get; set; }
        public decimal PriceValue { get; set; }
        public int PointRequired { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActivated { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid CreatedByUserId { get; set; }
        public DateTime? LastUpdatedAt { get; set; }
        public Guid? LastUpdatedByUserId { get; set; }
        public decimal PercentageValue { get; set; }

        public virtual DiscountTypes DiscountType { get; set; }
        public virtual Products Product { get; set; }
        public virtual ICollection<CartProducts> CartProducts { get; set; }
    }
}
