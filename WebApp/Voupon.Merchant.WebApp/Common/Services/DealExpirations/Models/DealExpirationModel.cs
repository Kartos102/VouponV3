using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Voupon.Merchant.WebApp.Common.Services.DealExpirations.Models
{
    public class DealExpirationModel
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int? ExpirationTypeId { get; set; }
        public string ExpirationType { get; set; }
        public int? TotalValidDays { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? ExpiredDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid CreatedByUserId { get; set; }
        public DateTime? LastUpdatedAt { get; set; }
        public Guid? LastUpdatedByUserId { get; set; }
    }
}
