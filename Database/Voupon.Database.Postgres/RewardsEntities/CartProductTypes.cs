using System;
using System.Collections.Generic;

namespace Voupon.Database.Postgres.RewardsEntities
{
    public partial class CartProductTypes
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;

        public virtual CartProductTypes IdNavigation { get; set; } = null!;
        public virtual CartProductTypes? InverseIdNavigation { get; set; }
    }
}
