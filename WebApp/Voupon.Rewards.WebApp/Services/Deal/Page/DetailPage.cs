using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Common.Azure.Blob;
using Voupon.Common.Services.ActivityLogs.Commands;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Rewards.WebApp.ViewModels;

namespace Voupon.Rewards.WebApp.Services.Deal.Page
{
    public class DetailPage : IRequest<ApiResponseViewModel>
    {
        public int Id { get; set; }
        public byte ExternalTypeId { get; set; }
        public string ExternaId { get; set; }
        public string ExternalMerchantId { get; set; }

        private class DetailPageHandler : IRequestHandler<DetailPage, ApiResponseViewModel>
        {
            RewardsDBContext rewardsDBContext;
            IAzureBlobStorage azureBlobStorage;
            public DetailPageHandler(RewardsDBContext rewardsDBContext, IAzureBlobStorage azureBlobStorage)
            {
                this.rewardsDBContext = rewardsDBContext;
                this.azureBlobStorage = azureBlobStorage;
            }

            private static string StripHTML(string htmlString)
            {

                string pattern = @"<(.|\n)*?>";

                return Regex.Replace(htmlString, pattern, string.Empty);
            }

            public async Task<ApiResponseViewModel> Handle(DetailPage request, CancellationToken cancellationToken)
            {
                var apiResponseViewModel = new ApiResponseViewModel();

                DetailPageViewModel product = null;
                if (request.Id != 0)
                {
                    product = await rewardsDBContext.Products.Include(x => x.DealExpirations).Include(x => x.Merchant).Include(x => x.ProductDiscounts).Include(x => x.ProductSubCategory).Include(x => x.ProductSubCategory).Include(x => x.ProductOutlets).ThenInclude(x => x.Outlet).Where(x => x.Id == request.Id && x.Merchant.IsTestAccount == false).Select(x => new DetailPageViewModel
                    {
                        ProductInternalId = x.Id,
                        Rating = x.Rating,
                        Id = x.Id,
                        AdditionInfo = x.AdditionInfo,
                        AvailableQuantity = x.AvailableQuantity,
                        DealExpirationId = x.DealExpirationId,
                        DealTypeId = x.DealTypeId,
                        Description = x.Description,
                        DiscountedPrice = x.DiscountedPrice,
                        DiscountRate = x.DiscountRate,
                        FinePrintInfo = x.FinePrintInfo,
                        ImageFolderUrl = x.ImageFolderUrl,
                        IsActivated = x.IsActivated,
                        IsPublished = x.IsPublished,
                        IsProductVariationEnabled = x.IsProductVariationEnabled,
                        LuckyDrawId = x.LuckyDrawId,
                        MerchantId = x.MerchantId,
                        PointsRequired = x.PointsRequired,
                        ActualPriceForVpoints = x.ActualPriceForVpoints,
                        Price = x.Price,
                        ProductCategoryId = x.ProductCategoryId,
                        ProductSubCategoryId = x.ProductSubCategoryId,
                        RedemptionInfo = x.RedemptionInfo,
                        StartDate = x.StartDate,
                        EndDate = x.EndDate,
                        StatusTypeId = x.StatusTypeId,
                        Remarks = x.Remarks,
                        Subtitle = x.Subtitle,
                        Title = x.Title,
                        TotalBought = x.TotalBought,
                        ShareShippingCostSameItem = x.ShareShippingCostSameItem,
                        DetailPageExpirationType = (x.DealExpirations != null ? new DetailPageExpirationType
                        {
                            Id = x.DealExpiration.Id,
                            Name = x.DealExpiration.ExpirationType.Name,
                            Type = x.DealExpiration.ExpirationTypeId.HasValue ? x.DealExpiration.ExpirationTypeId.Value : 0,
                            TotalValidDays = x.DealExpiration.TotalValidDays.HasValue ? x.DealExpiration.TotalValidDays.Value : 0,
                            StartDate = x.DealExpiration.StartDate.HasValue ? x.DealExpiration.StartDate.Value : new DateTime(1800, 1, 1),
                            ExpiredDate = x.DealExpiration.ExpiredDate.HasValue ? x.DealExpiration.ExpiredDate.Value : new DateTime(1800, 1, 1)
                        } : null),
                        Merchant = new DetailPageMerchant
                        {
                            Id = x.Merchant.Id,
                            Code = x.Merchant.Code,
                            DisplayName = x.Merchant.DisplayName,
                            LogoUrl = (x.Merchant.LogoUrl != null ? x.Merchant.LogoUrl.Replace("http://", "https://").Replace(":80", "") : ""),
                            OnVacation = false
                        },
                        ProductCategory = new DetailPageProductCategory
                        {
                            Id = x.ProductCategory.Id,
                            Name = x.ProductCategory.Name
                        },
                        ProductSubCategory = new DetailPageProductSubCategory
                        {
                            Id = x.ProductSubCategory.Id,
                            Name = x.ProductSubCategory.Name
                        },
                        ProductOutlets = x.ProductOutlets.Select(z => new DetailProductOutlets
                        {
                            Id = z.Outlet.Id,
                            Name = z.Outlet.Name
                        }).ToList(),
                        ProductDiscounts = x.ProductDiscounts.Select(z => new DetailPageProductDiscounts
                        {
                            Id = z.Id,
                            Name = z.Name,
                            PriceValue = z.PriceValue,
                            PercentageValue = z.PercentageValue,
                            DiscountTypeId = z.DiscountTypeId,
                            PointRequired = z.PointRequired,
                            IsActivated = z.IsActivated

                        }).Where(z => z.IsActivated == true).ToList()

                    }).FirstOrDefaultAsync();

                    if(product != null)
                    {
                        if(!string.IsNullOrEmpty(product.ExternalItemId))
                        {
                            product = null;
                        }
                    }

                }
                else
                {
                    product = await rewardsDBContext.Products.Include(x => x.DealExpirations).Include(x => x.Merchant).Include(x => x.ProductDiscounts).Include(x => x.ProductSubCategory).Include(x => x.ProductSubCategory).Include(x => x.ProductOutlets).ThenInclude(x => x.Outlet).Where(x => x.ExternalId == request.ExternaId && x.ExternalMerchantId == request.ExternalMerchantId && x.ExternalTypeId == request.ExternalTypeId && x.Merchant.IsTestAccount == false).Select(x => new DetailPageViewModel
                    {
                        ProductInternalId = x.Id,
                        ExternalTypeId = x.ExternalTypeId.Value,
                        ExternalItemId = x.ExternalId,
                        ExternalShopId = x.ExternalMerchantId,
                        Rating = x.Rating,
                        Id = x.Id,
                        AdditionInfo = x.AdditionInfo,
                        AvailableQuantity = x.AvailableQuantity,
                        DealExpirationId = x.DealExpirationId,
                        DealTypeId = x.DealTypeId,
                        Description = x.Description,
                        DiscountedPrice = x.DiscountedPrice,
                        DiscountRate = x.DiscountRate,
                        FinePrintInfo = x.FinePrintInfo,
                        ImageFolderUrl = x.ImageFolderUrl,
                        IsActivated = x.IsActivated,
                        IsPublished = x.IsPublished,
                        IsProductVariationEnabled = x.IsProductVariationEnabled,
                        LuckyDrawId = x.LuckyDrawId,
                        MerchantId = x.MerchantId,
                        PointsRequired = x.PointsRequired,
                        ActualPriceForVpoints = x.ActualPriceForVpoints,
                        Price = x.Price,
                        ProductCategoryId = x.ProductCategoryId,
                        ProductSubCategoryId = x.ProductSubCategoryId,
                        RedemptionInfo = x.RedemptionInfo,
                        StartDate = x.StartDate,
                        EndDate = x.EndDate,
                        StatusTypeId = x.StatusTypeId,
                        Remarks = x.Remarks,
                        Subtitle = x.Subtitle,
                        Title = x.Title,
                        TotalBought = x.TotalBought,
                        ShareShippingCostSameItem = x.ShareShippingCostSameItem,
                        DetailPageExpirationType = (x.DealExpirations != null ? new DetailPageExpirationType
                        {
                            Id = x.DealExpiration.Id,
                            Name = x.DealExpiration.ExpirationType.Name,
                            Type = x.DealExpiration.ExpirationTypeId.HasValue ? x.DealExpiration.ExpirationTypeId.Value : 0,
                            TotalValidDays = x.DealExpiration.TotalValidDays.HasValue ? x.DealExpiration.TotalValidDays.Value : 0,
                            StartDate = x.DealExpiration.StartDate.HasValue ? x.DealExpiration.StartDate.Value : new DateTime(1800, 1, 1),
                            ExpiredDate = x.DealExpiration.ExpiredDate.HasValue ? x.DealExpiration.ExpiredDate.Value : new DateTime(1800, 1, 1)
                        } : null),
                        Merchant = new DetailPageMerchant
                        {
                            Id = x.Merchant.Id,
                            Code = x.Merchant.Code,
                            DisplayName = x.Merchant.DisplayName,
                            LogoUrl = (x.Merchant.LogoUrl != null ? x.Merchant.LogoUrl.Replace("http://", "https://").Replace(":80", "") : ""),
                            OnVacation = false
                        },
                        ProductCategory = new DetailPageProductCategory
                        {
                            Id = x.ProductCategory.Id,
                            Name = x.ProductCategory.Name
                        },
                        ProductSubCategory = new DetailPageProductSubCategory
                        {
                            Id = x.ProductSubCategory.Id,
                            Name = x.ProductSubCategory.Name
                        },
                        ProductOutlets = x.ProductOutlets.Select(z => new DetailProductOutlets
                        {
                            Id = z.Outlet.Id,
                            Name = z.Outlet.Name
                        }).ToList(),
                        ProductDiscounts = x.ProductDiscounts.Select(z => new DetailPageProductDiscounts
                        {
                            Id = z.Id,
                            Name = z.Name,
                            PriceValue = z.PriceValue,
                            PercentageValue = z.PercentageValue,
                            DiscountTypeId = z.DiscountTypeId,
                            PointRequired = z.PointRequired,
                            IsActivated = z.IsActivated

                        }).Where(z => z.IsActivated == true).ToList()

                    }).FirstOrDefaultAsync();

                }

                if (product == null)
                {
                    apiResponseViewModel.Successful = false;
                    apiResponseViewModel.Message = "Invalid Product";
                    return apiResponseViewModel;
                }

                if (!product.Title.ToLower().Contains("voucher"))
                {
                    DetailPageProductDiscounts noneDiscount =
                    new DetailPageProductDiscounts
                    {
                        Id = 0,
                        Name = "None",
                        PriceValue = 0,
                    };

                    product.ProductDiscounts.Add(noneDiscount);
                }

                //product.Description = StripHTML(product.Description);

                var pendingOrderQuantity = rewardsDBContext.OrderItems.Where(x => x.ProductId == product.Id && x.Order.OrderStatus == 1).Count();
                if (product.AvailableQuantity <= pendingOrderQuantity)
                {
                    product.AvailableQuantity = 0;
                }

                if (product == null)
                {
                    apiResponseViewModel.Successful = false;
                    apiResponseViewModel.Message = "Invalid Deal";
                    return apiResponseViewModel;
                }
                if ((product.IsActivated == false || product.IsPublished == false))
                {
                    apiResponseViewModel.Successful = false;
                    apiResponseViewModel.Message = "Invalid Deal";
                    apiResponseViewModel.Code = -2;
                    return apiResponseViewModel;
                }

                var command = new Common.Blob.Queries.BlobNormalImagesListQuery()
                {
                    Id = product.Id,
                    ContainerName = ContainerNameEnum.Products,
                    FilePath = FilePathEnum.Products_Images
                };

                var azureBlobResult = await azureBlobStorage.ListBlobsAsync(ContainerNameEnum.Products, product.Id + "/" + FilePathEnum.Products_Images);
                if (azureBlobResult != null && azureBlobResult.Any())
                {
                    var fileList = new List<string>();
                    //product.ProductImageList = new List<SimgleProductImageInfo>();

                    foreach (var file in azureBlobResult)
                    {
                        if (file.StorageUri.PrimaryUri.OriginalString.Contains("big") || file.StorageUri.PrimaryUri.OriginalString.Contains("small") || file.StorageUri.PrimaryUri.OriginalString.Contains("org"))
                        {
                            continue;
                        }
                        fileList.Add(file.StorageUri.PrimaryUri.OriginalString.Replace("http://", "https://"));
                        //var FileWithProperties = (Microsoft.Azure.Storage.Blob.CloudBlockBlob)file;
                        //var createdAt = FileWithProperties.Properties.Created;
                        //var fileInfo = new SimgleProductImageInfo();
                        //fileInfo.ImageUrl = FileWithProperties.StorageUri.PrimaryUri.OriginalString;
                        //fileInfo.createdAt= createdAt;
                        //product.ProductImageList.Add(fileInfo);

                    }
                    product.ProductImageList = fileList;
                    product.ProductCartPreviewSmallImage = azureBlobResult.Where(x => x.StorageUri.PrimaryUri.OriginalString.Contains("small")).FirstOrDefault().StorageUri.PrimaryUri.OriginalString;
                    if (product.ProductCartPreviewSmallImage == "" || product.ProductCartPreviewSmallImage == null)
                    {
                        product.ProductCartPreviewSmallImage = fileList.FirstOrDefault().Replace("http://", "https://");

                    }
                }
                product.ProductReviewList = new List<ProductReview>();

                var productReviewList = await rewardsDBContext.ProductReview.Where(x => x.ProductId == product.Id).OrderByDescending(x => x.CreatedAt).ToListAsync();
                foreach (var review in productReviewList)
                {
                    ProductReview newReview = new ProductReview();
                    newReview.CreatedAt = review.CreatedAt;
                    newReview.Comment = review.Comment;
                    newReview.MemberName = String.IsNullOrEmpty(review.MemberName) ? "Vodus Customer" : review.MemberName;
                    newReview.ProductId = review.ProductId;
                    newReview.ProductTitle = review.ProductTitle;
                    newReview.Rating = review.Rating;
                    product.ProductReviewList.Add(newReview);
                }
                product.ProductReviewList = product.ProductReviewList.OrderByDescending(x => x.CreatedAt).ToList();

                //  Get VPoints multiplier
                if (!product.Title.Contains("voucher"))
                {
                    var appConfig = await rewardsDBContext.AppConfig.FirstOrDefaultAsync();
                    if (appConfig.IsVPointsMultiplierEnabled)
                    {
                        if (product.ProductDiscounts != null && product.ProductDiscounts.Any())
                        {
                            var newProductDiscountList = new List<DetailPageProductDiscounts>();
                            foreach (var item in product.ProductDiscounts)
                            {
                                var newProductDiscount = new DetailPageProductDiscounts();
                                if (item.PercentageValue > 0)
                                {
                                    var newMultiplier = appConfig.VPointsMultiplier * item.PercentageValue;
                                    if (newMultiplier > appConfig.VPointsMultiplierCap)
                                    {
                                        item.PercentageValue = appConfig.VPointsMultiplierCap;
                                        item.Name = $"Extra {item.PercentageValue.ToString("0.00")}% OFF ({item.PointRequired} VPoints)";
                                    }
                                    else if (newMultiplier <= appConfig.VPointsMultiplierCap)
                                    {
                                        item.PercentageValue = newMultiplier;
                                    }
                                    item.IsMultiplierEnabled = true;
                                    newProductDiscount = item;

                                    if (newProductDiscountList.Where(x => x.PercentageValue == newProductDiscount.PercentageValue).Any())
                                    {
                                        continue;
                                    }

                                    newProductDiscountList.Add(newProductDiscount);
                                }

                            }
                            product.ProductDiscounts = newProductDiscountList;

                        }
                    }
                }



                apiResponseViewModel.Successful = true;

                if (!string.IsNullOrEmpty(request.ExternaId))
                {
                    product.Id = 0;
                    product.MerchantId = 0;
                    product.Merchant.Id = 0;
                }

                apiResponseViewModel.Data = product;
                return apiResponseViewModel;
            }
        }
        public class DetailPageViewModel
        {
            public int ProductInternalId { get; set; }
            public string ExternalItemId { get; set; }
            public string ExternalShopId { get; set; }
            public short ExternalTypeId { get; set; }
            public string ExternalShopUrl { get; set; }
            public string ExternalItemUrl { get; set; }
            public int Id { get; set; }
            public int MerchantId { get; set; }
            public string Title { get; set; }
            public string Subtitle { get; set; }
            public string Description { get; set; }
            public string AdditionInfo { get; set; }
            public string FinePrintInfo { get; set; }
            public string RedemptionInfo { get; set; }
            public string ImageFolderUrl { get; set; }
            public int? ProductCategoryId { get; set; }
            public int? ProductSubCategoryId { get; set; }
            public int? DealTypeId { get; set; }
            public DateTime? StartDate { get; set; }
            public DateTime? EndDate { get; set; }
            public decimal? Price { get; set; }
            public decimal? ActualPriceForVpoints { get; set; }
            public decimal? DiscountedPrice { get; set; }
            public int? DiscountRate { get; set; }
            public int? PointsRequired { get; set; }
            public int? AvailableQuantity { get; set; }
            public int? DealExpirationId { get; set; }
            public int? LuckyDrawId { get; set; }
            public int StatusTypeId { get; set; }
            public bool IsActivated { get; set; }
            public bool IsProductVariationEnabled { get; set; }
            public string PendingChanges { get; set; }
            public DateTime CreatedAt { get; set; }
            public Guid CreatedByUserId { get; set; }
            public DateTime? LastUpdatedAt { get; set; }
            public Guid? LastUpdatedByUser { get; set; }
            public int? TotalBought { get; set; }
            public string Remarks { get; set; }
            public bool IsPublished { get; set; }
            public decimal Rating { get; set; }
            public List<string> ProductImageList { get; set; }
            public string ProductCartPreviewSmallImage { get; set; }

            public bool IsOriginalGuaranteeProduct { get; set; }

            public int MaxPurchaseLimit { get; set; }
            //public List<SimgleProductImageInfo> ProductImageList { get; set; }

            public DetailPageMerchant Merchant { get; set; }

            public DetailPageProductCategory ProductCategory { get; set; }
            public DetailPageProductSubCategory ProductSubCategory { get; set; }

            public List<DetailProductOutlets> ProductOutlets { get; set; }
            public List<DetailPageProductDiscounts> ProductDiscounts { get; set; }

            public DetailPageExpirationType DetailPageExpirationType { get; set; }

            public List<ProductReview> ProductReviewList { get; set; }

            public VariationModel VariationModel { get; set; }

            public List<string> VariationImageList { get; set; }

            public bool IsVPointsMultiplierEnabled { get; set; }
            public decimal VPointsMultiplier { get; set; }
            public decimal VPointsMultiplierCap { get; set; }

            public List<Config.Queries.AdditionalDiscounts> AdditionalDiscounts { get; set; }

            public int ShareShippingCostSameItem { get; set; }

            /*
            public virtual DealExpirations DealExpiration { get; set; }
            public virtual DealTypes DealType { get; set; }
            public virtual LuckyDraws LuckyDraw { get; set; }
            
            public virtual ProductCategories ProductCategory { get; set; }
            public virtual ProductSubCategories ProductSubCategory { get; set; }
            public virtual StatusTypes StatusType { get; set; }
            public virtual ICollection<DealExpirations> DealExpirations { get; set; }
            public virtual ICollection<LuckyDraws> LuckyDraws { get; set; }
           
            public virtual ICollection<ProductOutlets> ProductOutlets { get; set; }
            */

        }
        public class Video
        {
            public string Id { get; set; }

            public Uri Cover { get; set; }

            public Uri Url { get; set; }

            public long? Duration { get; set; }

            public object UploadTime { get; set; }
        }

        public class VariationModel
        {
            public int ProductId { get; set; }
            public List<VariationList> VariationList { get; set; }
            public List<ProductVariationDetailsList> ProductVariationDetailsList { get; set; }
        }

        public class VariationList
        {
            public string Name { get; set; }
            public int ProductId { get; set; }
            public bool IsFirstVariation { get; set; }
            public List<VariationOptions> VariationOptions { get; set; }

        }

        public class VariationOptions
        {
            public string Name { get; set; }
            public int Order { get; set; }
        }

        public class ProductVariationDetailsList
        {
            public string ExternalVariationId { get; set; }
            public int Id { get; set; }
            public string Sku { get; set; }
            public int AvailableQuantity { get; set; }
            public string Order { get; set; }
            public decimal Price { get; set; }
            public decimal PriceBeforeDiscount { get; set; }
            public decimal DiscountedPrice { get; set; }
            public bool IsDiscountedPriceEnabled { get; set; }
        }

        public class ProductReview
        {
            //public Guid Id { get; set; }
            public int ProductId { get; set; }
            //public int MerchantId { get; set; }
            //public Guid OrderItemId { get; set; }
            public decimal Rating { get; set; }
            public string Comment { get; set; }
            public string MemberName { get; set; }
            public string ProductTitle { get; set; }
            // public int MasterMemberProfileId { get; set; }
            public DateTime CreatedAt { get; set; }

            public string VariationText { get; set; }

            public List<string> ImageList { get; set; }
            public List<Video> VideoList { get; set; }
        }
        public class SingleProductImageInfo
        {
            public string ImageUrl { get; set; }
            public DateTimeOffset? createdAt { get; set; }
        }

        public class DetailPageExpirationType
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public int Type { get; set; }
            public int TotalValidDays { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime ExpiredDate { get; set; }
        }

        public class DetailPageMerchant
        {
            public int Id { get; set; }
            public string Code { get; set; }
            public string DisplayName { get; set; }
            public string LogoUrl { get; set; }
            public bool OnVacation { get; set; }
        }

        public class DetailPageProductCategory
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        public class DetailPageProductSubCategory
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        public class DetailProductOutlets
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        public class DetailPageProductDiscounts
        {
            public int Id { get; set; }
            public int DiscountTypeId { get; set; }
            public string Name { get; set; }
            public decimal PercentageValue { get; set; }
            public decimal PriceValue { get; set; }
            public int PointRequired { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public bool IsActivated { get; set; }
            public bool IsMultiplierEnabled { get; set; }

        }
    }


}
