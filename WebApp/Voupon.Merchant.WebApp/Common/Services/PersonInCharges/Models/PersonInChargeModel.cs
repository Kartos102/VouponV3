using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Voupon.Merchant.WebApp.Common.Services.PersonInCharges.Models
{
    public class PersonInChargeModel
    {
        public int Id { get; set; }
        public int MerchantId { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        [Display(Name = "Contact Number")]
        public string Contact { get; set; }
        [Required]
        [Display(Name = "Identity Number")]
        public string IdentityNumber { get; set; }
        [Required]
        public string Position { get; set; }
        //[Required(ErrorMessage = "The Identity document is required")]
        [Display(Name = "Upload your Identity document")]
        public string DocumentUrl { get; set; }
        public int StatusTypeId { get; set; }
        public string Status { get; set; }
        public string Remarks { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid CreatedByUserId { get; set; }
        public DateTime? LastUpdatedAt { get; set; }
        public Guid? LastUpdatedByUser { get; set; }

    }
}
