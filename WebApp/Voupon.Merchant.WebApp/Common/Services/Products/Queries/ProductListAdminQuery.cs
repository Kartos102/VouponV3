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
    public class ProductListAdminQuery : IRequest<ApiResponseViewModel>
    {
        public string method { get; set; }
        public string searchValue { get; set; }
    }
    public class ProductListAdminQueryHandler : IRequestHandler<ProductListAdminQuery, ApiResponseViewModel>
    {
        RewardsDBContext rewardsDBContext;
        public ProductListAdminQueryHandler(RewardsDBContext rewardsDBContext)
        {
            this.rewardsDBContext = rewardsDBContext;
        }

        public async Task<ApiResponseViewModel> Handle(ProductListAdminQuery request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();
            try
            {
                var items = await rewardsDBContext.Products.AsNoTracking().Include(x => x.Merchant).Include(x => x.ProductSubCategory).Include(x => x.ProductCategory).Include(x => x.StatusType).Include(x => x.DealType).Where(x => x.Merchant.IsTestAccount == false).Take(1).ToListAsync();


                if (request.method.Equals("full"))
                {
                    if (!string.IsNullOrEmpty(request.searchValue))
                    {
                        items = await rewardsDBContext.Products.AsNoTracking().Include(x => x.Merchant).Include(x => x.ProductSubCategory).Include(x => x.ProductCategory).Include(x => x.StatusType).Include(x => x.DealType).Where(x => x.Merchant.IsTestAccount == false && x.Title.ToLower().Contains(request.searchValue)).ToListAsync();
                    }

                    else
                    {
                        items = await rewardsDBContext.Products.AsNoTracking().Include(x => x.Merchant).Include(x => x.ProductSubCategory).Include(x => x.ProductCategory).Include(x => x.StatusType).Include(x => x.DealType).Where(x => x.Merchant.IsTestAccount == false).ToListAsync();

                    }


                }

                else
                {
                    items = await rewardsDBContext.Products.AsNoTracking().Include(x => x.Merchant).Include(x => x.ProductSubCategory).Include(x => x.ProductCategory).Include(x => x.StatusType).Include(x => x.DealType).Where(x => x.Merchant.IsTestAccount == false && x.StatusTypeId == 2).ToListAsync();

                }


                List<ProductAdminModel> list = new List<ProductAdminModel>();
                foreach (var item in items)
                {
                    ProductAdminModel newItem = new ProductAdminModel();
                    newItem.Id = item.Id;
                    newItem.MerchantId = item.MerchantId;
                    newItem.MerchantName = item.Merchant.DisplayName;
                    newItem.Title = item.Title;
                    newItem.ProductCategory = item.ProductCategory != null ? item.ProductCategory.Name : "";
                    newItem.ProductSubCategory = item.ProductSubCategory != null ? item.ProductSubCategory.Name : "";
                    newItem.DiscountedPrice = item.DiscountedPrice;
                    newItem.StatusTypeId = item.StatusTypeId;
                    newItem.StatusType = item.StatusType.Name;
                    newItem.CreatedAt = item.CreatedAt;
                    newItem.IsActivated = item.IsActivated;
                    newItem.IsPublished = item.IsPublished;
                    newItem.IsDeleted = item.IsDeleted;
                    newItem.DefaultCommission = item.DefaultCommission;

                    var jsonString = "";
                    if (!string.IsNullOrEmpty(item.PendingChanges))
                    {
                        jsonString = item.PendingChanges;
                        jsonString = jsonString.Replace("DealExpirations\":[]:", "DealExpirations\":null");
                        jsonString = jsonString.Replace("\"DealExpirations\":[],", "");
                        jsonString = jsonString.Replace("DealExpirations:[],", "");
                        jsonString = jsonString.Replace("DealExpirations", "DealExpiration");
                        var pendingChanges = JsonConvert.DeserializeObject<Voupon.Database.Postgres.RewardsEntities.Products>(jsonString);
                        newItem.StatusTypeId = pendingChanges.StatusTypeId;
                        newItem.StatusType = rewardsDBContext.StatusTypes.AsNoTracking().First(x => x.Id == pendingChanges.StatusTypeId).Name;
                    }

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
    public class ProductAdminModel
    {
        public int Id { get; set; }
        public int MerchantId { get; set; }
        public string MerchantName { get; set; }
        public string Title { get; set; } 
        public string ProductCategory { get; set; }
        public string ProductSubCategory { get; set; }
        public decimal? DiscountedPrice { get; set; }
        public int StatusTypeId { get; set; }
        public string StatusType { get; set; }
        public bool IsActivated { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsPublished { get; set; }
        public DateTime CreatedAt { get; set; }
        public decimal? DefaultCommission { get; set; }
        public string StatusTypePendingChanges { get; set; }

    }
}
