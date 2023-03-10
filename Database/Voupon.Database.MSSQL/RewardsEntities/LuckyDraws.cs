using System;
using System.Collections.Generic;

namespace Voupon.Database.MSSQL.RewardsEntities
{
    public partial class LuckyDraws
    {
        public LuckyDraws()
        {
            Products = new HashSet<Products>();
        }

        public int Id { get; set; }
        public int ProductId { get; set; }
        public DateTime? LuckyDrawDate { get; set; }
        public int? LuckyDrawTicketIssued { get; set; }
        public int? WinningTicketId { get; set; }
        public int? StatusTypeId { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid CreatedByUserId { get; set; }
        public DateTime? LastUpdatedAt { get; set; }
        public Guid? LastUpdatedByUserId { get; set; }

        public virtual Products Product { get; set; }
        public virtual StatusTypes StatusType { get; set; }
        public virtual ICollection<Products> Products { get; set; }
    }
}
