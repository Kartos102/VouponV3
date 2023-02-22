using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Common.Azure.Blob;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Rewards.WebApp.ViewModels;

namespace Voupon.Rewards.WebApp.Services.Deal.Commands
{
    public class GetRelatedOffers : IRequest<ApiResponseViewModel>
    {
        public int Id { get; set; }
        public int ProductCategoryId { get; set; }
        public int ProductSubCategoryId { get; set; }

        private class GetRelatedOffersHandler : IRequestHandler<GetRelatedOffers, ApiResponseViewModel>
        {
            Voupon.Database.Postgres.RewardsEntities.RewardsDBContext rewardsDBContext;
            IAzureBlobStorage azureBlobStorage;
            public GetRelatedOffersHandler(RewardsDBContext rewardsDBContext, IAzureBlobStorage azureBlobStorage)
            {
                this.rewardsDBContext = rewardsDBContext;
                this.azureBlobStorage = azureBlobStorage;
            }

            public async Task<ApiResponseViewModel> Handle(GetRelatedOffers request, CancellationToken cancellationToken)
            {
                var apiResponseViewModel = new ApiResponseViewModel();

                var productList = await rewardsDBContext.Products.Include(x => x.DealExpirations).Include(x => x.Merchant).Include(x => x.ProductDiscounts).Include(x => x.ProductSubCategory).Include(x => x.ProductSubCategory).Include(x => x.ProductOutlets).ThenInclude(x => x.Outlet).Where(x => x.ProductSubCategoryId == request.ProductSubCategoryId && x.Id != request.Id && x.Merchant.IsTestAccount == false && x.IsActivated == true && x.IsPublished == true).Select(x => new RelatedOffersViewModel
                {
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
                    LuckyDrawId = x.LuckyDrawId,
                    MerchantId = x.MerchantId,
                    PointsRequired = x.PointsRequired,
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
                    Rating=x.Rating,
                    RelatedOffersExpirationType = (x.DealExpirations != null ? new RelatedOffersExpirationType
                    {
                        Id = x.DealExpiration.ExpirationType.Id,
                        Name = x.DealExpiration.ExpirationType.Name,
                        Type = x.DealExpiration.ExpirationTypeId.HasValue ? x.DealExpiration.ExpirationTypeId.Value : 0,
                        TotalValidDays = x.DealExpiration.TotalValidDays.HasValue ? x.DealExpiration.TotalValidDays.Value : 0,
                        StartDate = x.DealExpiration.StartDate.HasValue ? x.DealExpiration.StartDate.Value : new DateTime(1800, 1, 1),
                        ExpiredDate = x.DealExpiration.ExpiredDate.HasValue ? x.DealExpiration.ExpiredDate.Value : new DateTime(1800, 1, 1)
                    } : null),
                    Merchant = new RelatedOffersMerchant
                    {
                        Id = x.Merchant.Id,
                        Code = x.Merchant.Code,
                        DisplayName = x.Merchant.DisplayName,
                        LogoUrl = x.Merchant.LogoUrl
                    },
                    ProductCategory = new RelatedOffersProductCategory
                    {
                        Id = x.ProductCategory.Id,
                        Name = x.ProductCategory.Name
                    },
                    ProductSubCategory = new RelatedOffersProductSubCategory
                    {
                        Id = x.ProductSubCategory.Id,
                        Name = x.ProductSubCategory.Name
                    },
                    ProductOutlets = x.ProductOutlets.Select(z => new DetailProductOutlets
                    {
                        Id = z.Outlet.Id,
                        Name = z.Outlet.Name
                    }).ToList(),
                    ProductDiscounts = x.ProductDiscounts.Select(z => new RelatedOffersProductDiscounts
                    {
                        Id = z.Id,
                        Name = z.Name,
                        PriceValue = z.PriceValue,
                        PercentageValue = z.PercentageValue,
                        DiscountTypeId = z.DiscountTypeId,
                        PointRequired = z.PointRequired

                    }).ToList()

                }).ToListAsync();
                if(productList == null || productList.Count == 0)
                {
                     productList = await rewardsDBContext.Products.Include(x => x.DealExpirations).Include(x => x.Merchant).Include(x => x.ProductDiscounts).Include(x => x.ProductSubCategory).Include(x => x.ProductSubCategory).Include(x => x.ProductOutlets).ThenInclude(x => x.Outlet).Where(x => x.ProductCategoryId == request.ProductCategoryId && x.Id != request.Id && x.IsActivated == true && x.IsPublished == true).Select(x => new RelatedOffersViewModel
                    {
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
                        LuckyDrawId = x.LuckyDrawId,
                        MerchantId = x.MerchantId,
                        PointsRequired = x.PointsRequired,
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
                        RelatedOffersExpirationType = (x.DealExpirations != null ? new RelatedOffersExpirationType
                        {
                            Id = x.DealExpiration.ExpirationType.Id,
                            Name = x.DealExpiration.ExpirationType.Name,
                            Type = x.DealExpiration.ExpirationTypeId.HasValue ? x.DealExpiration.ExpirationTypeId.Value : 0,
                            TotalValidDays = x.DealExpiration.TotalValidDays.HasValue ? x.DealExpiration.TotalValidDays.Value : 0,
                            StartDate = x.DealExpiration.StartDate.HasValue ? x.DealExpiration.StartDate.Value : new DateTime(1800, 1, 1),
                            ExpiredDate = x.DealExpiration.ExpiredDate.HasValue ? x.DealExpiration.ExpiredDate.Value : new DateTime(1800, 1, 1)
                        } : null),
                        Merchant = new RelatedOffersMerchant
                        {
                            Id = x.Merchant.Id,
                            Code = x.Merchant.Code,
                            DisplayName = x.Merchant.DisplayName,
                            LogoUrl = x.Merchant.LogoUrl
                        },
                        ProductCategory = new RelatedOffersProductCategory
                        {
                            Id = x.ProductCategory.Id,
                            Name = x.ProductCategory.Name
                        },
                        ProductSubCategory = new RelatedOffersProductSubCategory
                        {
                            Id = x.ProductSubCategory.Id,
                            Name = x.ProductSubCategory.Name
                        },
                        ProductOutlets = x.ProductOutlets.Select(z => new DetailProductOutlets
                        {
                            Id = z.Outlet.Id,
                            Name = z.Outlet.Name
                        }).ToList(),
                        ProductDiscounts = x.ProductDiscounts.Select(z => new RelatedOffersProductDiscounts
                        {
                            Id = z.Id,
                            Name = z.Name,
                            PriceValue = z.PriceValue,
                            PercentageValue = z.PercentageValue,
                            DiscountTypeId = z.DiscountTypeId,
                            PointRequired = z.PointRequired

                        }).ToList()

                    }).ToListAsync();
                }
                if(productList.Count() == 0)
                {
                    productList = await rewardsDBContext.Products.Include(x => x.DealExpirations).Include(x => x.Merchant).Include(x => x.ProductDiscounts).Include(x => x.ProductDiscounts).Include(x => x.ProductSubCategory).Include(x => x.ProductSubCategory).Include(x => x.ProductOutlets).ThenInclude(x => x.Outlet).Where( x=> x.Id != request.Id && x.IsActivated == true && x.IsPublished == true).Select(x => new RelatedOffersViewModel
                    {
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
                        LuckyDrawId = x.LuckyDrawId,
                        MerchantId = x.MerchantId,
                        PointsRequired = x.PointsRequired,
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
                        Rating=x.Rating,
                        RelatedOffersExpirationType = (x.DealExpirations != null ? new RelatedOffersExpirationType
                        {
                            Id = x.DealExpiration.ExpirationType.Id,
                            Name = x.DealExpiration.ExpirationType.Name,
                            Type = x.DealExpiration.ExpirationTypeId.HasValue ? x.DealExpiration.ExpirationTypeId.Value : 0,
                            TotalValidDays = x.DealExpiration.TotalValidDays.HasValue ? x.DealExpiration.TotalValidDays.Value : 0,
                            StartDate = x.DealExpiration.StartDate.HasValue ? x.DealExpiration.StartDate.Value : new DateTime(1800, 1, 1),
                            ExpiredDate = x.DealExpiration.ExpiredDate.HasValue ? x.DealExpiration.ExpiredDate.Value : new DateTime(1800, 1, 1)
                        } : null),
                        Merchant = new RelatedOffersMerchant
                        {
                            Id = x.Merchant.Id,
                            Code = x.Merchant.Code,
                            DisplayName = x.Merchant.DisplayName,
                            LogoUrl = x.Merchant.LogoUrl
                        },
                        ProductCategory = new RelatedOffersProductCategory
                        {
                            Id = x.ProductCategory.Id,
                            Name = x.ProductCategory.Name
                        },
                        ProductSubCategory = new RelatedOffersProductSubCategory
                        {
                            Id = x.ProductSubCategory.Id,
                            Name = x.ProductSubCategory.Name
                        },
                        ProductOutlets = x.ProductOutlets.Select(z => new DetailProductOutlets
                        {
                            Id = z.Outlet.Id,
                            Name = z.Outlet.Name
                        }).ToList(),
                        ProductDiscounts = x.ProductDiscounts.Select(z => new RelatedOffersProductDiscounts
                        {
                            Id = z.Id,
                            Name = z.Name,
                            PriceValue = z.PriceValue,
                            PercentageValue = z.PercentageValue,
                            DiscountTypeId = z.DiscountTypeId,
                            PointRequired = z.PointRequired

                        }).ToList()

                    }).ToListAsync();
                }
                if (productList != null && productList.Count() > 0)
                {
                    var product = productList.OrderBy(x => Guid.NewGuid()).FirstOrDefault();

                    if (product == null)
                    {
                        apiResponseViewModel.Successful = false;
                        apiResponseViewModel.Message = "Invalid Deal";
                        return apiResponseViewModel;
                    }


                    // Adding the Higher discount value to the discount rate alonge with the difference between price and discounted price
                    if (product.ProductDiscounts != null && product.ProductDiscounts.Count > 0)
                    {
                        var higherDiscount = product.ProductDiscounts.OrderByDescending(x => x.PointRequired).ToList().FirstOrDefault();

                        if (product.Price != 0 && product.DiscountedPrice != 0)
                        {
                            if (higherDiscount.DiscountTypeId == 1)
                            {
                                var totalDiscountPrice = ((product.DiscountedPrice * higherDiscount.PercentageValue) / 100) + (product.Price - product.DiscountedPrice);
                                product.DiscountRate = (int)(totalDiscountPrice * 100 / product.Price);
                                product.DiscountedPrice = product.Price - totalDiscountPrice;
                            }
                            else
                            {
                                var totalDiscountPrice = (higherDiscount.PriceValue) + (product.Price - product.DiscountedPrice);
                                product.DiscountRate = (int)(totalDiscountPrice * 100 / product.Price);
                                product.DiscountedPrice = product.Price - totalDiscountPrice;
                            }
                        }
                    }

                    var command = new Common.Blob.Queries.BlobSmallImagesListQuery()
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
                            fileList.Add(file.StorageUri.PrimaryUri.OriginalString);
                            //var FileWithProperties = (Microsoft.Azure.Storage.Blob.CloudBlockBlob)file;
                            //var createdAt = FileWithProperties.Properties.Created;
                            //var fileInfo = new SimgleProductImageInfo();
                            //fileInfo.ImageUrl = FileWithProperties.StorageUri.PrimaryUri.OriginalString;
                            //fileInfo.createdAt= createdAt;
                            //product.ProductImageList.Add(fileInfo);

                        }
                        product.ProductImageList = fileList;
                    }
                    apiResponseViewModel.Successful = true;
                    apiResponseViewModel.Data = product;
                    return apiResponseViewModel;
                }
                else
                {
                    apiResponseViewModel.Successful = false;
                    return apiResponseViewModel;
                }
            }
        }
        public class RelatedOffersViewModel
        {
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
            public decimal? DiscountedPrice { get; set; }
            public int? DiscountRate { get; set; }
            public int? PointsRequired { get; set; }
            public int? AvailableQuantity { get; set; }
            public int? DealExpirationId { get; set; }
            public int? LuckyDrawId { get; set; }
            public int StatusTypeId { get; set; }
            public bool IsActivated { get; set; }
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

            //public List<SimgleProductImageInfo> ProductImageList { get; set; }

            public RelatedOffersMerchant Merchant { get; set; }

            public RelatedOffersProductCategory ProductCategory { get; set; }
            public RelatedOffersProductSubCategory ProductSubCategory { get; set; }

            public List<DetailProductOutlets> ProductOutlets { get; set; }
            public List<RelatedOffersProductDiscounts> ProductDiscounts { get; set; }

            public RelatedOffersExpirationType RelatedOffersExpirationType { get; set; }

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

        public class SimgleProductImageInfo
        {
            public string ImageUrl { get; set; }
            public DateTimeOffset? createdAt { get; set; }
        }

        public class RelatedOffersExpirationType
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public int Type { get; set; }
            public int TotalValidDays { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime ExpiredDate { get; set; }
        }

        public class RelatedOffersMerchant
        {
            public int Id { get; set; }
            public string Code { get; set; }
            public string DisplayName { get; set; }
            public string LogoUrl { get; set; }
        }

        public class RelatedOffersProductCategory
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        public class RelatedOffersProductSubCategory
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        public class DetailProductOutlets
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        public class RelatedOffersProductDiscounts
        {
            public int Id { get; set; }
            public int DiscountTypeId { get; set; }
            public string Name { get; set; }
            public decimal PriceValue { get; set; }
            public decimal PercentageValue { get; set; }
            public int PointRequired { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
        }
    }

}
