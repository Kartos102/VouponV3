using System;
using System.Collections.Generic;

namespace Voupon.Database.MSSQL.RewardsEntities
{
    public partial class VariationCombination
    {
        public int Id { get; set; }
        public int OptionOneId { get; set; }
        public int? OptionTwoId { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid CreatedByUserId { get; set; }
        public DateTime? LastUpdatedAt { get; set; }
        public Guid? LastUpdateByUserId { get; set; }

        public virtual VariationOptions OptionOne { get; set; }
        public virtual ProductVariation ProductVariation { get; set; }
    }
}
