using System;
using System.Collections.Generic;

namespace Voupon.Database.MSSQL.RewardsEntities
{
    public partial class Refunds
    {
        public Refunds()
        {
            RefundPayments = new HashSet<RefundPayments>();
        }

        public Guid Id { get; set; }
        public string ShortId { get; set; }
        public Guid OrderItemId { get; set; }
        public int MasterMemberProfileId { get; set; }
        public byte Type { get; set; }
        public byte Status { get; set; }
        public int PointsRefunded { get; set; }
        public decimal MoneyRefunded { get; set; }
        public string Remark { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid CreatedBy { get; set; }
        public byte RefundMethod { get; set; }
        public string RefundTransactionId { get; set; }
        public string RefundJsonResponse { get; set; }

        public virtual OrderItems OrderItem { get; set; }
        public virtual ICollection<RefundPayments> RefundPayments { get; set; }
    }
}
