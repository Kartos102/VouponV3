using System;
using System.Collections.Generic;

namespace Voupon.Database.Postgres.RewardsEntities
{
    public partial class VariationOptions
    {
        public VariationOptions()
        {
            VariationCombination = new HashSet<VariationCombination>();
        }

        public int Id { get; set; }
        public int VariationId { get; set; }
        public string Name { get; set; } = null!;
        public int Order { get; set; }
        public string? ImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTime? LastUpdatedAt { get; set; }
        public Guid? LastUpdateByUserId { get; set; }

        public virtual Variations Variation { get; set; } = null!;
        public virtual ICollection<VariationCombination> VariationCombination { get; set; }
    }
}
