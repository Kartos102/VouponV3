using System;
using System.Collections.Generic;

namespace Voupon.Database.MSSQL.RewardsEntities
{
    public partial class Variations
    {
        public Variations()
        {
            VariationOptions = new HashSet<VariationOptions>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsFirstVariation { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid CreatedByUserId { get; set; }
        public int ProductId { get; set; }
        public DateTime? LastUpdatedAt { get; set; }
        public Guid? LastUpdateByUserId { get; set; }

        public virtual Products Product { get; set; }
        public virtual ICollection<VariationOptions> VariationOptions { get; set; }
    }
}
