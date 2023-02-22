using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Voupon.Common.Enum
{
    public enum OrderItemStatusEnum : byte
    {
        [Description("Pending")]
        Pending = 1,
        [Description("Sent")]
        Sent = 2,
        [Description("Done")]
        Done = 3,
        [Description("Refund in progress")]
        RefundInProgress = 4,
        [Description("Refund")]
        Refunded = 5,
        [Description("Completed")]
        Completed = 6,
        [Description("Pending Payment")]
        PendingPayment = 7,
        [Description("Pending Refund")]
        PendingRefund = 8,
        [Description("Pending Refund")]
        RefundRejected = 9,

    }
}
