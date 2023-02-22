using System;
using System.Collections.Generic;

namespace Voupon.Database.MSSQL.RewardsEntities
{
    public partial class ProductVariation
    {
        public ProductVariation()
        {
            CartProducts = new HashSet<CartProducts>();
        }

        public int Id { get; set; }
        public int ProductId { get; set; }
        public int VariationCombinationId { get; set; }
        public int AvailableQuantity { get; set; }
        public decimal Price { get; set; }
        public decimal? DiscountedPrice { get; set; }
        public string SKU { get; set; }
        public Guid CreatedByUserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsDiscountedPriceEnabled { get; set; }
        public DateTime? LastUpdatedAt { get; set; }
        public Guid? LastUpdateByUserId { get; set; }

        public virtual Products Product { get; set; }
        public virtual VariationCombination VariationCombination { get; set; }
        public virtual ICollection<CartProducts> CartProducts { get; set; }
    }
}
