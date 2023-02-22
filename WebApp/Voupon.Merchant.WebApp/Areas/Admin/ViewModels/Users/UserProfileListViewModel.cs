using System;

namespace Voupon.Merchant.WebApp.Areas.Admin.ViewModels.Users
{
    public class UserProfileListViewModel
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public string? Address { get; set; }
        public DateTime? Dob { get; set; }
        
        public string MobilePhone { get; set; }
        public System.DateTime? CreatedAt { get; set; }
        public int? VPoints { get; set; }
    }
}