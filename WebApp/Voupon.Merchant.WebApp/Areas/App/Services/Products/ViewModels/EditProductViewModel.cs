using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Voupon.Merchant.WebApp.Areas.App.Services.Products.ViewModels
{
    public class EditProductViewModel1
    {
        public int Id { get; set; }
        public int MerchantId { get; set; }

        [Required(ErrorMessage = "Please input product title")]
        [Display(Name = "Title")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Please input product subtitle")]
        [Display(Name = "Subtitle")]
        public string Subtitle { get; set; }
        [Required(ErrorMessage = "Please input product description")]
        [Display(Name = "Description")]
        public string Description { get; set; }
        [Required(ErrorMessage = "Please input product Addition Info")]
        [Display(Name = "Addition Info")]
        public string AdditionInfo { get; set; }
        [Required(ErrorMessage = "Please input product FinePrint Info")]
        [Display(Name = "FinePrint Info")]
        public string FinePrintInfo { get; set; }
        [Required(ErrorMessage = "Please input product Redemption Info")]
        [Display(Name = "Redemption Info")]
        public string RedemptionInfo { get; set; }
        public string ImageFolderUrl { get; set; }
        [Required(ErrorMessage = "Please input Product Category")]
        [Display(Name = "Product Category")]
        public int ProductCategoryId { get; set; }
        [Required(ErrorMessage = "Please input Product Subcategory")]
        [Display(Name = "Product Subcategory")]
        public int ProductSubCategoryId { get; set; }
        [Required(ErrorMessage = "Please input Product Type")]
        [Display(Name = "Product Type")]
        public int DealTypeId { get; set; }
        [Required(ErrorMessage = "Please input Start Date")]
        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; }
        [Required(ErrorMessage = "Please input End Date")]
        [Display(Name = "End Date")]
        public DateTime EndDate { get; set; }
        [Display(Name = "Price")]
        public decimal? Price { get; set; }
        [Display(Name = "Discounted Price")]
        public decimal? DiscountedPrice { get; set; }
        //public int? DiscountRate { get; set; }
        public int? PointsRequired { get; set; }
        [Required(ErrorMessage = "Please input Available Quantity")]
        [Display(Name = "Available Quantity")]       
        public int AvailableQuantity { get; set; }
        [Required(ErrorMessage = "Please input Product Expiration")]
        [Display(Name = "Product Expiration")]
        public int DealExpirationId { get; set; }
        //public int LuckyDrawId { get; set; }
        //public int StatusTypeId { get; set; }
        //public bool IsActivated { get; set; }
        //public string PendingChanges { get; set; }
        //public DateTime CreatedAt { get; set; }
        //public Guid CreatedByUserId { get; set; }
        //public DateTime? LastUpdatedAt { get; set; }
        //public Guid? LastUpdatedByUser { get; set; }
        //public int TotalBought { get; set; }
        //public string Remarks { get; set; }
        //public bool IsPublished { get; set; }
    }
}
