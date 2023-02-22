using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Voupon.Merchant.WebApp.Common.Services.Outlets.Models
{
    public class OutletModel
    {
        public int Id { get; set; }
        public int MerchantId { get; set; }
        public string Name { get; set; }
        public string StreetName { get; set; }
        public string Address_1 { get; set; }
        public string Address_2 { get; set; }
        public int? CountryId { get; set; }
        public string Country { get; set; }
        public int? ProvinceId { get; set; }
        public string Province { get; set; }
        public int? DistrictId { get; set; }
        public string District { get; set; }
        public int? PostCodeId { get; set; }
        public string Postcode { get; set; }
        public string Contact { get; set; }
        public string ImgUrl { get; set; }
        public string OpeningHour_1 { get; set; }
        public string OpeningHour_2 { get; set; }
        public string OpeningHour_3 { get; set; }
        public string OpeningHour_4 { get; set; }
        public string OpeningHour_5 { get; set; }
        public string OpeningHour_6 { get; set; }
        public string OpeningHour_7 { get; set; }
        public string OpeningHour_8 { get; set; }
        public bool IsActivated { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid CreatedByUserId { get; set; }
        public DateTime? LastUpdatedAt { get; set; }
        public Guid? LastUpdatedByUserId { get; set; }
    }
}
