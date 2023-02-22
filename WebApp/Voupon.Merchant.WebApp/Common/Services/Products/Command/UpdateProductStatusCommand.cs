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
    public class UpdateProductStatusCommand : IRequest<ApiResponseViewModel>
    {
        public int ProductId { get; set; }
        public int StatusTypeId { get; set; }
        public string Remarks { get; set; }
        public DateTime LastUpdatedAt { get; set; }
        public Guid LastUpdatedByUserId { get; set; }
    }

    public class UpdateProductStatusCommandHandler : IRequestHandler<UpdateProductStatusCommand, ApiResponseViewModel>
    {
        RewardsDBContext rewardsDBContext;
        public UpdateProductStatusCommandHandler(RewardsDBContext rewardsDBContext)
        {
            this.rewardsDBContext = rewardsDBContext;
        }

        public async Task<ApiResponseViewModel> Handle(UpdateProductStatusCommand request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();

            if (!string.IsNullOrEmpty(request.Remarks))
                request.Remarks = request.Remarks + "<i class='meesage-time'>" + request.LastUpdatedAt.ToString() + "</i>";
            var product = await rewardsDBContext.Products.FirstOrDefaultAsync(x => x.Id == request.ProductId);
            if (product != null)
            {
                product.StatusTypeId = request.StatusTypeId;
                if (!string.IsNullOrEmpty(product.Remarks) && !string.IsNullOrEmpty(request.Remarks))
                    product.Remarks = product.Remarks + "<br>" + request.Remarks;
                else if (!string.IsNullOrEmpty(request.Remarks))
                    product.Remarks = request.Remarks;
                product.LastUpdatedAt = request.LastUpdatedAt;
                product.LastUpdatedByUser = request.LastUpdatedByUserId;
                if (request.StatusTypeId == StatusTypeEnum.APPROVED)
                {
                    if (!String.IsNullOrEmpty(product.PendingChanges))
                    {
                        var item = JsonConvert.DeserializeObject<Voupon.Database.Postgres.RewardsEntities.Products>(product.PendingChanges);
                        product.AdditionInfo = item.AdditionInfo;
                        product.AvailableQuantity = item.AvailableQuantity;
                     //   product.DealExpirationId = item.DealExpirationId;
                        product.DealTypeId = item.DealTypeId;
                        product.Description = item.Description;
                        product.DiscountedPrice = item.DiscountedPrice;
                        product.DiscountRate = item.DiscountRate;
                        product.EndDate = item.EndDate;
                        product.FinePrintInfo = item.FinePrintInfo;
                        product.ImageFolderUrl = item.ImageFolderUrl;
                        product.LuckyDrawId = item.LuckyDrawId;
                        product.PointsRequired = item.PointsRequired;
                        product.Price = item.Price;
                        product.ActualPriceForVpoints = item.ActualPriceForVpoints;
                        product.ProductCategoryId = item.ProductCategoryId;
                        product.ProductSubCategoryId = item.ProductSubCategoryId;
                        product.IsDiscountedPriceEnabled = item.IsDiscountedPriceEnabled;
                        product.RedemptionInfo = item.RedemptionInfo;
                        product.StartDate = item.StartDate;
                        product.Subtitle = item.Subtitle;
                        product.IsShareShippingDifferentItem = item.IsShareShippingDifferentItem;
                        product.ShareShippingCostSameItem = item.ShareShippingCostSameItem;
                        product.Title = item.Title;   
                    }
                    product.PendingChanges = "";
                    product.PendingChanges = JsonConvert.SerializeObject(product);
                }
               
                rewardsDBContext.SaveChanges();               
                response.Successful = true;
                response.Message = "Update Product Status Successfully";
                response.Data = product.Remarks;

            }
            else
            {
                response.Message = "Product not found";
            }
            return response;
        }
    }
}
