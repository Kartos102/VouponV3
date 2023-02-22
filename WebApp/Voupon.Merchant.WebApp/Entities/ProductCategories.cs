using System;
using System.Collections.Generic;

namespace Voupon.Merchant.WebApp.Entities
{
    public partial class ProductCategories
    {
        public ProductCategories()
        {
            ProductSubCategories = new HashSet<ProductSubCategories>();
            Products = new HashSet<Products>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsActivated { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid CreatedByUserId { get; set; }
        public DateTime? LastUpdatedAt { get; set; }
        public Guid? LastUpdatedByUserId { get; set; }

        public virtual ICollection<ProductSubCategories> ProductSubCategories { get; set; }
        public virtual ICollection<Products> Products { get; set; }
    }
}
