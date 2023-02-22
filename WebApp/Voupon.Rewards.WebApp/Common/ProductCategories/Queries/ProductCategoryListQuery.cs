using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Rewards.WebApp.Common.Products.Models;
using Voupon.Rewards.WebApp.ViewModels;
using Voupon.Rewards.WebApp.Common.ProductCategories.Models;

namespace Voupon.Rewards.WebApp.Common.ProductCategories.Queries
{ 
    public class ProductCategoryListQuery : IRequest<ApiResponseViewModel>
    {
    }
    public class ProductCategoryListQueryHandler : IRequestHandler<ProductCategoryListQuery, ApiResponseViewModel>
    {
        RewardsDBContext rewardsDBContext;
        public ProductCategoryListQueryHandler(RewardsDBContext rewardsDBContext)
        {
            this.rewardsDBContext = rewardsDBContext;
        }

        public async Task<ApiResponseViewModel> Handle(ProductCategoryListQuery request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();
            try
            {
                var items = await rewardsDBContext.ProductCategories.ToListAsync();
                List<ProductCategoryModel> list = new List<ProductCategoryModel>();
                foreach (var item in items)
                {
                    ProductCategoryModel newItem = new ProductCategoryModel();
                    newItem.Id = item.Id;
                    newItem.Name = item.Name;
                    newItem.CreatedAt = item.CreatedAt;
                    newItem.CreatedByUserId = item.CreatedByUserId;
                    newItem.IsActivated = item.IsActivated;
                    newItem.LastUpdatedAt = item.LastUpdatedAt;
                    newItem.LastUpdatedByUserId = item.LastUpdatedByUserId;
                    list.Add(newItem);
                }
                response.Successful = true;
                response.Message = "Get Product Category List Successfully";
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
