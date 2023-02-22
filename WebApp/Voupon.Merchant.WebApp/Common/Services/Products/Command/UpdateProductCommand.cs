using MediatR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Common.Enum;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Merchant.WebApp.Common.Services.Products.Models;
using Voupon.Merchant.WebApp.ViewModels;

namespace Voupon.Merchant.WebApp.Common.Services.Products.Command
{
    public class UpdateProductCommand : IRequest<ApiResponseViewModel>
    {
        public int ProductId { get; set; }
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public string Description { get; set; }
        public string AdditionInfo { get; set; }
        public string FinePrintInfo { get; set; }
        public string RedemptionInfo { get; set; }
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
        public string ImageFolderUrl { get; set; }
        public DateTime LastUpdatedAt { get; set; }
        public Guid LastUpdatedByUserId { get; set; }

        public Guid ThirdPartyTypeId { get; set; }

        public Guid ThirdPartyProductId { get; set; }
        public bool IsDiscountedPriceEnabled { get; set; }

        public bool IsShareShippingDifferentItem { get; set; }

        public int ShareShippingCostSameItem { get; set; }
    }

    public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, ApiResponseViewModel>
    {
        RewardsDBContext rewardsDBContext;
        public UpdateProductCommandHandler(RewardsDBContext rewardsDBContext)
        {
            this.rewardsDBContext = rewardsDBContext;
        }

        public async Task<ApiResponseViewModel> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();

            var product = await rewardsDBContext.Products.FirstOrDefaultAsync(x => x.Id == request.ProductId);
            if (product != null)
            {
                var dealExpiration = await rewardsDBContext.DealExpirations.FirstOrDefaultAsync(x => x.ProductId == request.ProductId);
                if (dealExpiration != null)
                {
                    product.DealExpirationId = dealExpiration.Id;
                }

                var product1 = await rewardsDBContext.Products.AsNoTracking().FirstOrDefaultAsync(x => x.Id == request.ProductId);
                if (dealExpiration != null)
                {
                    product1.DealExpirationId = dealExpiration.Id;
                }

                product1.LastUpdatedAt = request.LastUpdatedAt;
                product1.LastUpdatedByUser = request.LastUpdatedByUserId;
                product1.ImageFolderUrl = request.ImageFolderUrl;
                product1.Title = request.Title;
                product1.Subtitle = request.Subtitle;
                product1.Description = request.Description;
                product1.AdditionInfo = request.AdditionInfo;
                product1.FinePrintInfo = request.FinePrintInfo;
                product1.RedemptionInfo = request.RedemptionInfo;
                product1.ProductCategoryId = request.ProductCategoryId;
                product1.ProductSubCategoryId = request.ProductSubCategoryId;
                product1.DealTypeId = request.DealTypeId;
                product1.StartDate = request.StartDate;
                product1.EndDate = request.EndDate;
                product1.Price = request.Price;
                product1.ActualPriceForVpoints = request.ActualPriceForVpoints;
                if(request.DiscountedPrice == null || request.DiscountedPrice == 0)
                {
                    product1.DiscountedPrice = request.Price;
                }
                else
                {
                    product1.DiscountedPrice = request.DiscountedPrice;

                }
                product1.DiscountRate = request.DiscountRate;
                product1.PointsRequired = request.PointsRequired;
                product1.AvailableQuantity = request.AvailableQuantity;
                product1.IsDiscountedPriceEnabled = request.IsDiscountedPriceEnabled;
                product1.PendingChanges = "";

                if (request.ThirdPartyTypeId != null)
                {
                    product1.ThirdPartyTypeId = request.ThirdPartyTypeId;
                    if (request.ThirdPartyProductId != null)
                    {
                        product1.ThirdPartyProductId = request.ThirdPartyProductId;
                    }
                }

                product.LastUpdatedAt = request.LastUpdatedAt;
                product.LastUpdatedByUser = request.LastUpdatedByUserId;
                product.ImageFolderUrl = request.ImageFolderUrl;
                product.Title = request.Title;
                product.Subtitle = request.Subtitle;
                product.Description = request.Description;
                product.AdditionInfo = request.AdditionInfo;
                product.FinePrintInfo = request.FinePrintInfo;
                product.RedemptionInfo = request.RedemptionInfo;
                product.ProductCategoryId = request.ProductCategoryId;
                product.ProductSubCategoryId = request.ProductSubCategoryId;
                product.DealTypeId = request.DealTypeId;
                product.StartDate = request.StartDate;
                product.EndDate = request.EndDate;
                product.Price = request.Price;
                product.ActualPriceForVpoints = request.ActualPriceForVpoints;
                
                if (request.DiscountedPrice == null || request.DiscountedPrice == 0)
                {
                    product.DiscountedPrice = request.Price;
                }
                else
                {
                    product.DiscountedPrice = request.DiscountedPrice;

                }
                product.DiscountRate = request.DiscountRate;
                product.PointsRequired = request.PointsRequired;
                product.AvailableQuantity = request.AvailableQuantity;
                product.IsDiscountedPriceEnabled = request.IsDiscountedPriceEnabled;
                product.IsShareShippingDifferentItem = request.IsShareShippingDifferentItem;
                product.ShareShippingCostSameItem = request.ShareShippingCostSameItem;
                product.PendingChanges = "";

                if (request.ThirdPartyTypeId != null)
                {
                    product.ThirdPartyTypeId = request.ThirdPartyTypeId;
                    if (request.ThirdPartyProductId != null)
                    {
                        product.ThirdPartyProductId = request.ThirdPartyProductId;
                    }
                }

                product.PendingChanges = JsonConvert.SerializeObject(product1);
                rewardsDBContext.SaveChanges();
                response.Successful = true;
                response.Message = "Update Product Successfully";
            }
            else
            {
                response.Message = "Product not found";
            }
            return response;
        }
    }
}
