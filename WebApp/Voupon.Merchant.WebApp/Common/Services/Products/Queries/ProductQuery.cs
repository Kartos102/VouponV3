using MediatR;
using Microsoft.EntityFrameworkCore;
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
    public class ProductQuery : IRequest<ApiResponseViewModel>
    {
        public int ProductId { get; set; }
    }
    public class ProductQueryHandler : IRequestHandler<ProductQuery, ApiResponseViewModel>
    {
        RewardsDBContext rewardsDBContext;
        public ProductQueryHandler(RewardsDBContext rewardsDBContext)
        {
            this.rewardsDBContext = rewardsDBContext;
        }

        public async Task<ApiResponseViewModel> Handle(ProductQuery request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();
            try
            {
                var item = await rewardsDBContext.Products.Include(x => x.Merchant).Include(x => x.ProductSubCategory).Include(x => x.ProductCategory).Include(x => x.StatusType).Include(x => x.DealType).FirstOrDefaultAsync(x => x.Id == request.ProductId);
                var dealExpiration = await rewardsDBContext.DealExpirations.FirstOrDefaultAsync(x => x.ProductId == request.ProductId);
                if (dealExpiration != null)
                    item.DealExpirationId = dealExpiration.Id;
                ProductModel newItem = new ProductModel();
                newItem.Id = item.Id;
                newItem.MerchantId = item.MerchantId;
                newItem.MerchantCode = item.Merchant.Code;
                newItem.MerchantName = item.Merchant.DisplayName;
                newItem.Title = item.Title;
                newItem.Subtitle = item.Subtitle;
                newItem.Description = item.Description;
                newItem.AdditionInfo = item.AdditionInfo;
                newItem.FinePrintInfo = item.FinePrintInfo;
                newItem.RedemptionInfo = item.RedemptionInfo;
                newItem.ImageFolderUrl = item.ImageFolderUrl;
                newItem.ProductCategoryId = item.ProductCategoryId;
                newItem.ProductCategory = item.ProductCategory != null ? item.ProductCategory.Name : "";
                newItem.ProductSubCategoryId = item.ProductSubCategoryId;
                newItem.ProductSubCategory = item.ProductSubCategory != null ? item.ProductSubCategory.Name : "";
                newItem.DealTypeId = item.DealTypeId;
                newItem.DealType = item.DealType != null ? item.DealType.Name : "";
                newItem.StartDate = item.StartDate;
                newItem.EndDate = item.EndDate;
                newItem.Price = item.Price;
                newItem.ActualPriceForVpoints = item.ActualPriceForVpoints;
                newItem.DiscountedPrice = item.DiscountedPrice;
                newItem.DiscountRate = item.DiscountRate;
                newItem.PointsRequired = item.PointsRequired;
                newItem.AvailableQuantity = item.AvailableQuantity;
                newItem.DealExpirationId = item.DealExpirationId;
                newItem.LuckyDrawId = item.LuckyDrawId;
                newItem.StatusTypeId = item.StatusTypeId;
                newItem.StatusType = item.StatusType.Name;
                newItem.IsActivated = item.IsActivated;
                newItem.CreatedAt = item.CreatedAt;
                newItem.CreatedByUserId = item.CreatedByUserId;
                newItem.IsActivated = item.IsActivated;
                newItem.IsDiscountedPriceEnabled = item.IsDiscountedPriceEnabled;
                newItem.IsProductVariationEnabled = item.IsProductVariationEnabled;
                newItem.LastUpdatedAt = item.LastUpdatedAt;
                newItem.LastUpdatedByUser = item.LastUpdatedByUser;
                newItem.TotalBought = item.TotalBought;
                newItem.Remarks = item.Remarks;
                newItem.DefaultCommission = item.DefaultCommission;
                newItem.ThirdPartyTypeId = (item.ThirdPartyTypeId.HasValue ? item.ThirdPartyTypeId.Value: null);
                newItem.ThirdPartyProductId = (item.ThirdPartyProductId.HasValue ? item.ThirdPartyProductId.Value : null);
                newItem.IsShareShippingDifferentItem = item.IsShareShippingDifferentItem;
                newItem.ShareShippingCostSameItem = item.ShareShippingCostSameItem;
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
}
