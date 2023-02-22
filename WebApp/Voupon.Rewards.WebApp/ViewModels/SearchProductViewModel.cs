using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Voupon.Rewards.WebApp.ViewModels
{
    public class SearchResultViewModel
    {
        public int SortBy { get; set; }
        public int TotalResult { get; set; }
        public int PageNumber { get; set; }

        public string SearchQuery { get; set; }

        public string NextPageUrl { get; set; }
        public List<SearchProductViewModel> Items { get; set; }
    }
    public class SearchProductViewModel
    {
        public int Id { get; set; }       
        public string Title { get; set; }      
        public string ProductImage { get; set; }
        public int? ProductCategoryId { get; set; }
        public string ProductCategory { get; set; }
        public int? ProductSubCategoryId { get; set; }
        public string ProductSubCategory { get; set; }
        public int? DealTypeId { get; set; }
        public string DealType { get; set; }
        public decimal? Price { get; set; }
        public decimal? DiscountedPrice { get; set; }
        public int? DiscountRate { get; set; }
        public int? PointsRequired { get; set; }
        public int? DealExpirationId { get; set; }
        public int? ExpirationTypeId { get; set; }
       
        public int TotalSold { get; set; }       
        public decimal Rating { get; set; }
        public short ExternalTypeId { get; set; }
        public string ExternalShopId { get; set; }

        public string ExternalItemId { get; set; }

        public string ExternalUrl { get; set; }
        public string OutletLocation { get; set; }

        public decimal? DiscountedPriceMin { get; set; }

        public decimal? DiscountedPriceMax { get; set; }

        public decimal? BeforeDiscountedPriceMin { get; set; }

        public decimal? BeforeDiscountedPriceMax { get; set; }

        public bool IsOriginalGuaranteeProduct { get; set; }

        public string Brand { get; set; }

        public string Description { get; set; }

        public string Language { get; set; }

        public decimal ShippingCost { get; set; }

        public int Rand { get; set; }
    }

    public class SearchProductViewModelFilterTitles : SearchProductViewModel
    {
        public List<string> FilterTitle { get; set; }
    }
}
