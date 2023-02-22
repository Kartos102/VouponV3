using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Voupon.Rewards.WebApp.ViewModels.ThirdParty.RevenueMonster
{
    public class RevenueMonsterRequestViewModel
    {
        public Order Order { get; set; }
        public Customer Customer { get; set; }
        public string[] Method { get; set; }

        //  WEB_PAYMENT | MOBILE_PAYMENT
        public string Type { get; set; }
        public string StoreId { get; set; }
        public string RedirectUrl { get; set; }
        public string NotifyUrl { get; set; }
        public string LayoutVersion { get; set; }
    }

    public class Order
    {
        public string Title { get; set; }
        public string Detail { get; set; }
        public string AdditionalData { get; set; }
        public int Amount { get; set; }
        public string CurrencyType { get; set; }
        public string Id { get; set; }
    }

    public class Customer
    {
        public string UserId { get; set; }
        public string Email { get; set; }
        public string CountryCode { get; set; }
        public string PhoneNumber { get; set; }
    }
}
