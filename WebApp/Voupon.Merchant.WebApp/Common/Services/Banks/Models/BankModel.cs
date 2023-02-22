using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Voupon.Merchant.WebApp.Common.Services.Banks.Models
{
    public class BankModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string BankCode { get; set; }
        public string SWIFTCode { get; set; }
        public int CountryId { get; set; }
        public bool IsActivated { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid CreatedByUserId { get; set; }
        public DateTime? LastUpdatedAt { get; set; }
        public Guid? LastUpdatedByUserId { get; set; }
    }
}
