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
    public class ProductListByStatusQueryAndMerchantId : IRequest<ApiResponseViewModel>
    {
        public int Status { get; set; }
        public int MerchantId { get; set; }
    }
    public class ProductListByStatusQueryAndMerchantIdHandler : IRequestHandler<ProductListByStatusQueryAndMerchantId, ApiResponseViewModel>
    {
        RewardsDBContext rewardsDBContext;
        public ProductListByStatusQueryAndMerchantIdHandler(RewardsDBContext rewardsDBContext)
        {
            this.rewardsDBContext = rewardsDBContext;
        }

        public async Task<ApiResponseViewModel> Handle(ProductListByStatusQueryAndMerchantId request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();
            try
            {
                var items = await rewardsDBContext.Products.Where(x=>x.MerchantId==request.MerchantId).Include(x => x.Merchant).Include(x => x.ProductSubCategory).Include(x => x.ProductCategory).Include(x => x.StatusType).Include(x => x.DealType).ToListAsync();
                List<ProductModel> list = new List<ProductModel>();
                foreach (var item in items)
                {
                    if(request.Status== Voupon.Common.Enum.StatusTypeEnum.APPROVED)
                    {
                        if(item.StatusTypeId== Voupon.Common.Enum.StatusTypeEnum.APPROVED)
                        {
                            goto AddItem;                          
                        }
                        continue;
                    }

                    bool IsMatch = false;
                    Voupon.Database.Postgres.RewardsEntities.Products jsonProductItem = null;
                    var jsonString = "";
                    if (!String.IsNullOrEmpty(item.PendingChanges))
                    {
                        jsonString = item.PendingChanges;
                        jsonString = jsonString.Replace("DealExpirations\":[]:", "DealExpirations\":null");
                        jsonString = jsonString.Replace("\"DealExpirations\":[],", "");
                        jsonString = jsonString.Replace("DealExpirations:[],", "");
                        jsonString = jsonString.Replace("DealExpirations", "DealExpiration");
                        jsonProductItem = JsonConvert.DeserializeObject<Voupon.Database.Postgres.RewardsEntities.Products>(jsonString);
                    }

                    if (jsonProductItem != null)
                    {
                        if (jsonProductItem.StatusTypeId == request.Status)
                        {
                            IsMatch = true;
                        }
                    }
                    else
                    {
                        if (item.StatusTypeId == request.Status)
                        {
                            IsMatch = true;
                        }
                    }

                    if (!IsMatch)
                    {
                        continue;
                    }
                AddItem:
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
                    newItem.Remarks = item.Remarks;
                    newItem.IsPublished = item.IsPublished;
                    newItem.DefaultCommission = item.DefaultCommission;
                    list.Add(newItem);
                }
                response.Successful = true;
                response.Message = "Get Product List Successfully";
                response.Data = list;
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }

            return response;
        }
    }
}
