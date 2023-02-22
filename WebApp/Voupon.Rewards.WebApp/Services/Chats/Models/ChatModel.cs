using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Voupon.Rewards.WebApp.Common.Services.Chats.Models
{
    public class ChatModel
    {
        public int Id { get; set; }
        public int MerchantId { get; set; }
        [Required(ErrorMessage = "Please provide the Bank Account Name")]
        [Display(Name = "Bank Account Name​")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Please provide the Account Number")]
        [Display(Name = "Account Number")]
        public string AccountNumber { get; set; }
        [Required(ErrorMessage = "Please select the Bank Type")]
        [Display(Name = "Bank Type​")]
        public int? BankId { get; set; }
        public string Bank { get; set; }
        //[Required(ErrorMessage = "Bank document is required for payout into your account.")]
        [DataType(DataType.Upload)]
        [Display(Name = "Upload your Bank document")]
        public string DocumentUrl { get; set; }
        public int StatusTypeId { get; set; }
        public string Status { get; set; }
        public string Remarks { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid CreatedByUserId { get; set; }
        public DateTime? LastUpdatedAt { get; set; }
        public Guid? LastUpdatedByUserId { get; set; }
    }
}
