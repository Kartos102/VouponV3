using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Voupon.Merchant.WebApp.Areas.App.Services.Outlets.ViewModels
{
    public class OutletViewModel
    {
        public int Id { get; set; }
        public int MerchantId { get; set; }
        [Required(ErrorMessage = "Please input outlet name")]
        [Display(Name = "Outlet Name")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Please input street name")]
        [Display(Name = "Mall name/ Street name")]
        public string StreetName { get; set; }
        [Required(ErrorMessage = "Please input address 1")]
        [Display(Name = "Address 1")]
        public string Address_1 { get; set; }
        [Display(Name = "Address 2")]
        public string Address_2 { get; set; }
        [Required(ErrorMessage = "Please choose country")]
        [Display(Name = "Country")]
        public int CountryId { get; set; }
        [Required(ErrorMessage = "Please choose province")]
        [Display(Name = "Province")]
        public int ProvinceId { get; set; }
        [Required(ErrorMessage = "Please choose district")]
        [Display(Name = "District")]
        public int DistrictId { get; set; }
        [Required(ErrorMessage = "Please choose Postcode")]
        [Display(Name = "Postcode")]
        public int PostCodeId { get; set; }
        [Required(ErrorMessage = "Please input Contact")]
        [Display(Name = "Contact")]
        [DataType(DataType.PhoneNumber)]
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
