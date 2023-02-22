using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Common.Azure.Blob;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Rewards.WebApp.Common.Blob.Queries;
using Voupon.Rewards.WebApp.Common.Products.Models;
using Voupon.Rewards.WebApp.ViewModels;

namespace Voupon.Rewards.WebApp.Services.Deal.Commands
{
    public class GetProductsFromSameShop : IRequest<ApiResponseViewModel>
    {
        public int MerchantId { get; set; }
        public string ExternalMerchantId { get; set; }
        public byte ExternalTypeId { get; set; }
    }
    public class GetProductsFromSameShopHandler : IRequestHandler<GetProductsFromSameShop, ApiResponseViewModel>
    {
        RewardsDBContext rewardsDBContext;
        private readonly IOptions<AppSettings> appSettings;
        public GetProductsFromSameShopHandler(RewardsDBContext rewardsDBContext, IOptions<AppSettings> appSettings)
        {
            this.rewardsDBContext = rewardsDBContext;
            this.appSettings = appSettings;
        }

        public async Task<ApiResponseViewModel> Handle(GetProductsFromSameShop request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();
            try
            {
                var items = new List<Voupon.Database.Postgres.RewardsEntities.Products>();

                if (!string.IsNullOrEmpty(request.ExternalMerchantId))
                {
                    items = await rewardsDBContext.Products
                    .Include(x => x.Merchant)
                    .Include(x => x.ProductDiscounts)
                    .Include(x => x.ProductSubCategory)
                    .Include(x => x.ProductCategory)
                    .Include(x => x.StatusType)
                    .Include(x => x.DealType).Include(x => x.DealExpirations)
                    .Where(x => x.ExternalMerchantId == request.ExternalMerchantId && x.ExternalTypeId == request.ExternalTypeId && x.IsPublished && x.IsActivated).Take(31).ToListAsync();
                }
                else
                {
                    items = await rewardsDBContext.Products
                    .Include(x => x.Merchant)
                    .Include(x => x.ProductDiscounts)
                    .Include(x => x.ProductSubCategory)
                    .Include(x => x.ProductCategory)
                    .Include(x => x.StatusType)
                    .Include(x => x.DealType).Include(x => x.DealExpirations)
                    .Where(x => x.MerchantId == request.MerchantId && x.IsPublished && x.IsActivated).Take(31).ToListAsync();
                }

                var provinces = await rewardsDBContext.Provinces.ToListAsync();

                List<ProductModel> list = new List<ProductModel>();
                foreach (var item in items)
                {
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
                    newItem.ImageFolderUrl = new List<string>();
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
                    newItem.Remarks = item.Remarks;
                    newItem.Rating = item.Rating;
                    newItem.IsPublished = item.IsPublished;
                    if (item.ExternalTypeId != null)
                    {
                        newItem.ExternalTypeId = (byte)item.ExternalTypeId;
                        newItem.ExternalId = item.ExternalId;
                        newItem.ExternalMerchantId = item.ExternalMerchantId;
                    }

                    if (item.DealExpiration != null)
                    {
                        newItem.ExpirationTypeId = item.DealExpiration.ExpirationTypeId;
                    }
                    else
                    {
                        newItem.ExpirationTypeId = 5;
                    }
                    // Adding the Higher discount value to the discount rate alonge with the difference between price and discounted price
                    if (item.ProductDiscounts != null && item.ProductDiscounts.Count > 0)
                    {
                        var higherDiscount = item.ProductDiscounts.OrderByDescending(x => x.PointRequired).ToList().FirstOrDefault();

                        if (newItem.Price != 0 && newItem.DiscountedPrice != 0)
                        {
                            if (higherDiscount.DiscountTypeId == 1)
                            {
                                var totalDiscountPrice = ((newItem.DiscountedPrice * higherDiscount.PercentageValue) / 100) + (newItem.Price - newItem.DiscountedPrice);
                                newItem.DiscountRate = (int)(totalDiscountPrice * 100 / newItem.Price);
                                newItem.DiscountedPrice = newItem.Price - totalDiscountPrice;
                            }
                            else
                            {
                                var totalDiscountPrice = (higherDiscount.PriceValue) + (newItem.Price - newItem.DiscountedPrice);
                                newItem.DiscountRate = (int)(totalDiscountPrice * 100 / newItem.Price);
                                newItem.DiscountedPrice = newItem.Price - totalDiscountPrice;
                            }
                        }
                    }
                    newItem.OutletName = new List<string>();
                    var provice = provinces.Where(x => x.Id == item.Merchant.ProvinceId).FirstOrDefault();
                    if (provice != null)
                    {
                        newItem.OutletName.Add(provice.Name);
                    }
                    list.Add(newItem);
                }

                if (list != null && list.Any())
                {
                    response.Successful = true;
                    response.Message = "Get Product List Successfully";
                    response.Data = list;
                }

                //  Get giftee products
                if (request.MerchantId >= 1000 && request.MerchantId <= 1004)
                {
                    var gifteeProducts = await GifteeProducts(request.MerchantId);

                    if (gifteeProducts != null)
                    {
                        response.Data = gifteeProducts;
                        response.Successful = true;
                    }
                }

            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }

            return response;
        }

        private async Task<List<ProductModel>> GifteeProducts(int merchantId)
        {
            var list = new List<ProductModel>();

            var idList = "";

            if (merchantId == 1000)
            {
                idList = appSettings.Value.ThirdPartyRedemptions.Giftee.Products.SushiKing;
            }
            else if (merchantId == 1001)
            {
                idList = appSettings.Value.ThirdPartyRedemptions.Giftee.Products.LaoLao;
            }
            else if (merchantId == 1002)
            {
                idList = appSettings.Value.ThirdPartyRedemptions.Giftee.Products.Hokkaido;
            }
            else if (merchantId == 1003)
            {
                idList = appSettings.Value.ThirdPartyRedemptions.Giftee.Products.BigApple;
            }
            else if (merchantId == 1004)
            {
                idList = appSettings.Value.ThirdPartyRedemptions.Giftee.Products.TeaLive;
            }

            if (string.IsNullOrEmpty(idList))
            {
                return null;
            }

            var itemId = idList.Split(",");

            for (var i = 0; i < itemId.Length; i++)
            {
                var id = int.Parse(itemId[i].ToString());

                var item = await rewardsDBContext.Products.Include(x => x.ProductOutlets).ThenInclude(x => x.Outlet).Include(x => x.Merchant).Include(x => x.ProductSubCategory).Include(x => x.ProductCategory).Include(x => x.StatusType).Include(x => x.DealType).Include(x => x.DealExpirations).Where(x => x.Id == id).FirstOrDefaultAsync();

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
                newItem.ImageFolderUrl = new List<string>();
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
                newItem.Remarks = item.Remarks;
                newItem.IsPublished = item.IsPublished;
                if ((item.DealExpirations != null && item.DealExpirations.Count > 0) && item.DealExpirations.FirstOrDefault().ExpirationType != null)
                {
                    newItem.ExpirationTypeId = item.DealExpirations.FirstOrDefault().ExpirationTypeId;
                }
                else
                {
                    newItem.ExpirationTypeId = 5;
                }
                //newItem.OutletName = item.ProductOutlets.Select(x => x.Outlet.Name).ToList();
                //newItem.OutletProvince = item.ProductOutlets.Select(x => x.Outlet.ProvinceId).ToList();
                //newItem.TotalOutlets = item.ProductOutlets.Count();
                list.Add(newItem);

            }
            return list;
        }
    }
}
