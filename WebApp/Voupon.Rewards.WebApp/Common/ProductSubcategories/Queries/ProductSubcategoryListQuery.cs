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
using Voupon.Rewards.WebApp.Common.ProductSubcategories.Models;

namespace Voupon.Rewards.WebApp.Common.ProductSubcategories.Queries
{   
    public class ProductSubcategoryListQuery : IRequest<ApiResponseViewModel>
    {
        public int CategoryId { get; set; }
    }
    public class ProductSubcategoryListQueryHandler : IRequestHandler<ProductSubcategoryListQuery, ApiResponseViewModel>
    {
        RewardsDBContext rewardsDBContext;
        public ProductSubcategoryListQueryHandler(RewardsDBContext rewardsDBContext)
        {
            this.rewardsDBContext = rewardsDBContext;
        }

        public async Task<ApiResponseViewModel> Handle(ProductSubcategoryListQuery request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();
            try
            {
                List<ProductSubCategories> items = null;
                if (request.CategoryId==0)
                {
                    items = await rewardsDBContext.ProductSubCategories.Include(x => x.Category).ToListAsync();
                }
                else
                    items = await rewardsDBContext.ProductSubCategories.Include(x => x.Category).Where(x=>x.CategoryId== request.CategoryId).ToListAsync();
                List<ProductSubcategoryModel> list = new List<ProductSubcategoryModel>();
                foreach (var item in items)
                {
                    ProductSubcategoryModel newItem = new ProductSubcategoryModel();
                    newItem.Id = item.Id;
                    newItem.Name = item.Name;
                    newItem.CategoryId = item.CategoryId;
                    newItem.Category = item.Category.Name;
                    newItem.CreatedAt = item.CreatedAt;
                    newItem.CreatedByUserId = item.CreatedByUserId;
                    newItem.IsActivated = item.IsActivated;
                    newItem.LastUpdatedAt = item.LastUpdatedAt;
                    newItem.LastUpdatedByUserId = item.LastUpdatedByUserId;
                    list.Add(newItem);
                }
                response.Successful = true;
                response.Message = "Get Product Subcategory List Successfully";
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
