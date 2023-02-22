using MediatR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Merchant.WebApp.Common.Services.Products.Models;
using Voupon.Merchant.WebApp.ViewModels;

namespace Voupon.Merchant.WebApp.Common.Services.Products.Queries
{
    public class ProductPendingChangesQuery : IRequest<ApiResponseViewModel>
    {
        public int ProductId { get; set; }
    }
    public class ProductPendingChangesQueryHandler : IRequestHandler<ProductPendingChangesQuery, ApiResponseViewModel>
    {
        RewardsDBContext rewardsDBContext;
        public ProductPendingChangesQueryHandler(RewardsDBContext rewardsDBContext)
        {
            this.rewardsDBContext = rewardsDBContext;
        }

        public async Task<ApiResponseViewModel> Handle(ProductPendingChangesQuery request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();
            try
            {
                var product = await rewardsDBContext.Products.FirstOrDefaultAsync(x => x.Id == request.ProductId);
                if (product != null)
                {
                    var jsonString = "";
                    var dealExpiration = await rewardsDBContext.DealExpirations.AsNoTracking().FirstOrDefaultAsync(x => x.ProductId == request.ProductId);
                    if (dealExpiration != null)
                    {
                        product.DealExpirationId = dealExpiration.Id;
                    }

                    if (string.IsNullOrEmpty(product.PendingChanges))
                    {
                        jsonString = JsonConvert.SerializeObject(product);
                        product.PendingChanges = jsonString;
                        rewardsDBContext.SaveChanges();
                    }
                    else
                    {
                        jsonString = product.PendingChanges;
                        jsonString = jsonString.Replace("DealExpirations\":[]:", "DealExpirations\":null");
                        jsonString = jsonString.Replace("\"DealExpirations\":[],", "");
                        jsonString = jsonString.Replace("DealExpirations:[],", "");
                        jsonString = jsonString.Replace("DealExpirations", "DealExpiration");
                    }

                    var item = JsonConvert.DeserializeObject<Voupon.Database.Postgres.RewardsEntities.Products>(jsonString);
                    if (item.StatusTypeId != 4)
                    {
                        ProductModel newItem = new ProductModel();
                        newItem.Id = item.Id;
                        newItem.MerchantId = item.MerchantId;
                        var merchant = rewardsDBContext.Merchants.AsNoTracking().First(x => x.Id == item.MerchantId);
                        newItem.MerchantCode = merchant.Code;
                        newItem.MerchantName = merchant.DisplayName;
                        newItem.Title = item.Title;
                        newItem.Subtitle = item.Subtitle;
                        newItem.Description = item.Description;
                        newItem.AdditionInfo = item.AdditionInfo;
                        newItem.FinePrintInfo = item.FinePrintInfo;
                        newItem.RedemptionInfo = item.RedemptionInfo;
                        newItem.ImageFolderUrl = item.ImageFolderUrl;
                        newItem.ProductCategoryId = item.ProductCategoryId;
                        if (item.ProductCategoryId != null && item.ProductCategoryId != 0)
                            newItem.ProductCategory = item.ProductCategoryId.HasValue ? rewardsDBContext.ProductCategories.AsNoTracking().First(x => x.Id == item.ProductCategoryId).Name : "";
                        newItem.ProductSubCategoryId = item.ProductSubCategoryId;
                        if (item.ProductSubCategoryId != null && item.ProductSubCategoryId != 0)
                            newItem.ProductSubCategory = item.ProductSubCategoryId.HasValue ? rewardsDBContext.ProductSubCategories.AsNoTracking().First(x => x.Id == item.ProductSubCategoryId).Name : "";
                        newItem.DealTypeId = item.DealTypeId;
                        newItem.DealType = item.DealTypeId.HasValue ? rewardsDBContext.DealTypes.AsNoTracking().First(x => x.Id == item.DealTypeId).Name : "";
                        newItem.StartDate = item.StartDate;
                        newItem.EndDate = item.EndDate;
                        newItem.Price = item.Price;
                        newItem.ActualPriceForVpoints = item.ActualPriceForVpoints;
                        newItem.DiscountedPrice = item.DiscountedPrice;
                        newItem.DiscountRate = item.DiscountRate;
                        newItem.PointsRequired = item.PointsRequired;
                        newItem.AvailableQuantity = item.AvailableQuantity;
                        newItem.DealExpirationId = product.DealExpirationId;// item.DealExpirationId;
                        newItem.LuckyDrawId = item.LuckyDrawId;
                        newItem.StatusTypeId = item.StatusTypeId;
                        newItem.StatusType = rewardsDBContext.StatusTypes.AsNoTracking().First(x => x.Id == item.StatusTypeId).Name;
                        newItem.IsActivated = item.IsActivated;
                        newItem.IsDiscountedPriceEnabled = item.IsDiscountedPriceEnabled;
                        newItem.IsProductVariationEnabled = item.IsProductVariationEnabled;
                        newItem.CreatedAt = item.CreatedAt;
                        newItem.CreatedByUserId = item.CreatedByUserId;
                        newItem.IsActivated = item.IsActivated;
                        newItem.LastUpdatedAt = item.LastUpdatedAt;
                        newItem.LastUpdatedByUser = item.LastUpdatedByUser;
                        newItem.TotalBought = item.TotalBought;
                        newItem.Remarks = item.Remarks;
                        newItem.DefaultCommission = item.DefaultCommission;
                        newItem.ThirdPartyTypeId = (item.ThirdPartyTypeId.HasValue ? item.ThirdPartyTypeId.Value : null);
                        newItem.ThirdPartyProductId = (item.ThirdPartyProductId.HasValue ? item.ThirdPartyProductId.Value : null);
                        newItem.IsShareShippingDifferentItem = item.IsShareShippingDifferentItem;
                        newItem.ShareShippingCostSameItem = item.ShareShippingCostSameItem;


                        newItem.IsPublished = item.IsPublished;

                        response.Successful = true;
                        response.Message = "Get Product Successfully";
                        response.Data = newItem;
                    }
                    else
                    {
                        try
                        {
                            var productItem = await rewardsDBContext.Products.Include(x => x.Merchant).Include(x => x.ProductSubCategory).Include(x => x.ProductCategory).Include(x => x.StatusType).Include(x => x.DealType).FirstOrDefaultAsync(x => x.Id == request.ProductId);
                            //var dealExpiration = await rewardsDBContext.DealExpirations.FirstOrDefaultAsync(x => x.ProductId == request.ProductId);
                            if (dealExpiration != null)
                                productItem.DealExpirationId = dealExpiration.Id;
                            ProductModel newItem = new ProductModel();
                            newItem.Id = productItem.Id;
                            newItem.MerchantId = productItem.MerchantId;
                            newItem.MerchantCode = productItem.Merchant.Code;
                            newItem.MerchantName = productItem.Merchant.DisplayName;
                            newItem.Title = productItem.Title;
                            newItem.Subtitle = productItem.Subtitle;
                            newItem.Description = productItem.Description;
                            newItem.AdditionInfo = productItem.AdditionInfo;
                            newItem.FinePrintInfo = productItem.FinePrintInfo;
                            newItem.RedemptionInfo = productItem.RedemptionInfo;
                            newItem.ImageFolderUrl = productItem.ImageFolderUrl;
                            newItem.ProductCategoryId = productItem.ProductCategoryId;
                            newItem.ProductCategory = productItem.ProductCategory != null ? productItem.ProductCategory.Name : "";
                            newItem.ProductSubCategoryId = productItem.ProductSubCategoryId;
                            newItem.ProductSubCategory = productItem.ProductSubCategory != null ? productItem.ProductSubCategory.Name : "";
                            newItem.DealTypeId = productItem.DealTypeId;
                            newItem.DealType = productItem.DealType != null ? productItem.DealType.Name : "";
                            newItem.StartDate = productItem.StartDate;
                            newItem.EndDate = productItem.EndDate;
                            newItem.Price = productItem.Price;
                            newItem.ActualPriceForVpoints = productItem.ActualPriceForVpoints;
                            newItem.DiscountedPrice = productItem.DiscountedPrice;
                            newItem.DiscountRate = productItem.DiscountRate;
                            newItem.PointsRequired = productItem.PointsRequired;
                            newItem.AvailableQuantity = productItem.AvailableQuantity;
                            newItem.DealExpirationId = productItem.DealExpirationId;
                            newItem.LuckyDrawId = productItem.LuckyDrawId;
                            newItem.StatusTypeId = productItem.StatusTypeId;
                            newItem.StatusType = productItem.StatusType.Name;
                            newItem.IsActivated = productItem.IsActivated;
                            newItem.CreatedAt = productItem.CreatedAt;
                            newItem.CreatedByUserId = productItem.CreatedByUserId;
                            newItem.IsActivated = productItem.IsActivated;
                            newItem.IsDiscountedPriceEnabled = productItem.IsDiscountedPriceEnabled;
                            newItem.IsProductVariationEnabled = productItem.IsProductVariationEnabled;
                            newItem.LastUpdatedAt = productItem.LastUpdatedAt;
                            newItem.LastUpdatedByUser = productItem.LastUpdatedByUser;
                            newItem.TotalBought = productItem.TotalBought;
                            newItem.Remarks = productItem.Remarks;
                            newItem.DefaultCommission = productItem.DefaultCommission;
                            newItem.ThirdPartyTypeId = (productItem.ThirdPartyTypeId.HasValue ? productItem.ThirdPartyTypeId.Value : null);
                            newItem.ThirdPartyProductId = (productItem.ThirdPartyProductId.HasValue ? productItem.ThirdPartyProductId.Value : null);
                            newItem.IsShareShippingDifferentItem = productItem.IsShareShippingDifferentItem;
                            newItem.ShareShippingCostSameItem = productItem.ShareShippingCostSameItem;

                            newItem.IsPublished = productItem.IsPublished;

                            response.Successful = true;

                        

                            response.Message = "Get Product Successfully";
                            response.Data = newItem;
                        }
                        catch (Exception ex)
                        {
                            response.Message = ex.Message;
                        }

                        return response;
                    }

                }
                else
                {
                    response.Successful = false;
                    response.Message = "Product not found";
                }

            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }

            return response;
        }
    }
}
