using System;
using System.Collections.Generic;

namespace Voupon.Database.Postgres.RewardsEntities
{
    public partial class RefundsExternalOrderItems
    {
        public Guid Id { get; set; }
        public string ShortId { get; set; } = null!;
        public Guid OrderItemId { get; set; }
        public int MasterMemberProfileId { get; set; }
        public short Type { get; set; }
        public short Status { get; set; }
        public int PointsRefunded { get; set; }
        public decimal MoneyRefunded { get; set; }
        public string? Remark { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid CreatedBy { get; set; }
        public short RefundMethod { get; set; }
        public string? RefundTransactionId { get; set; }
        public string? RefundJsonResponse { get; set; }
    }
}
