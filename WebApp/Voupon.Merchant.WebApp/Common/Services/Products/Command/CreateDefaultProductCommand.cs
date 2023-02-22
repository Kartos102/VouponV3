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
    public class CreateDefaultProductCommand : IRequest<ApiResponseViewModel>
    {
        public int MerchantId { get; set; }
        public string Title { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid CreatedByUserId { get; set; }
    }
    public class CreateDefaultProductCommandHandler : IRequestHandler<CreateDefaultProductCommand, ApiResponseViewModel>
    {
        RewardsDBContext rewardsDBContext;
        public CreateDefaultProductCommandHandler(RewardsDBContext rewardsDBContext)
        {
            this.rewardsDBContext = rewardsDBContext;
        }

        public async Task<ApiResponseViewModel> Handle(CreateDefaultProductCommand request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();
            try
            {
                var merchant= await rewardsDBContext.Merchants.FirstOrDefaultAsync(x => x.Id == request.MerchantId);
                if(merchant==null)
                {
                    response.Successful = false;
                    response.Message = "Invalid Merchant Id";
                    return response;
                }
                var item = new Voupon.Database.Postgres.RewardsEntities.Products();
                item.MerchantId = request.MerchantId;
                item.Title = request.Title;
                item.Subtitle = "";
                item.Description = "";
                item.FinePrintInfo = "";
                item.AdditionInfo = "";
                item.AvailableQuantity = 0;
                item.PointsRequired = 0;
                item.TotalBought = 0;
                item.StatusTypeId = 1;
                item.RedemptionInfo = "";
                item.Price = 0;
                item.DiscountedPrice = 0;
                item.DiscountRate = 0;
                item.CreatedAt = request.CreatedAt;
                item.DealExpiration = null;
                item.DealExpirations = null;
                item.CreatedByUserId = request.CreatedByUserId;
                item.IsActivated = false;
                item.DefaultCommission = merchant.DefaultCommission;
                await rewardsDBContext.Products.AddAsync(item);
                rewardsDBContext.SaveChanges();
                item = await rewardsDBContext.Products.Include(x => x.Merchant).Include(x => x.ProductSubCategory).Include(x => x.ProductCategory).Include(x => x.StatusType).Include(x => x.DealType).FirstOrDefaultAsync(x => x.Id == item.Id);

                CreateDiscount(item.Id, 3, request.CreatedAt, request.CreatedByUserId);
                CreateDiscount(item.Id, 6, request.CreatedAt, request.CreatedByUserId);
                CreateDiscount(item.Id, 12, request.CreatedAt, request.CreatedByUserId);
                CreateDiscount(item.Id, 15, request.CreatedAt, request.CreatedByUserId);


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
                newItem.LastUpdatedAt = item.LastUpdatedAt;
                newItem.LastUpdatedByUser = item.LastUpdatedByUser;
                newItem.TotalBought = item.TotalBought;
                newItem.DefaultCommission = item.DefaultCommission;
                response.Successful = true;
                response.Message = "Get Product Successfully";
                response.Data = newItem.Id;
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }

            return response;
        }



        public async void CreateDiscount(int id, int point,DateTime created, Guid userID ) {
            DateTime now = DateTime.Now;
            var discount = new Voupon.Database.Postgres.RewardsEntities.ProductDiscounts();
            discount.ProductId = id;
            discount.Name = point + "% DISCOUNT ("+ point+" Vpoints)";
            discount.DiscountTypeId = 1;
            discount.PointRequired = point;
            discount.PriceValue = 0;
            discount.PercentageValue = point;
            discount.StartDate = now.Date;
            discount.EndDate = now.AddYears(2).Date;
            discount.CreatedAt = created;
            discount.CreatedByUserId = userID;
            discount.IsActivated = true;
            await rewardsDBContext.ProductDiscounts.AddAsync(discount);
            rewardsDBContext.SaveChanges();
        }

    }
}
