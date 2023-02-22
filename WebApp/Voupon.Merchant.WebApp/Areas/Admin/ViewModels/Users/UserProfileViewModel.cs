using System;
using System.Collections.Generic;

namespace Voupon.Merchant.WebApp.Areas.Admin.ViewModels.Users
{
    public class BonusPoint
    {
        public int BonusVpoint { get; set; }
        public string Remarks { get; set; }
        public string AwardedBy { get; set; }
        public string DateOfAward{ get; set; }
    }
    public class UserProfileViewModel
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public string UserToken { get; set; }
        public string UserId { get; set; }
        public int? VPoints { get; set; }
        public DateTime? JoinDate { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? Ethinicity { get; set; }
        public string State { get; set; }
        public string? Age { get; set; }
        public string? Education { get; set; }
        public string? Address { get; set; }
        public string? MobilePhone { get; set; }
        public List<BonusPoint> BonusPointList { get; set; }
    }
}