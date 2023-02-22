using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Merchant.WebApp.Common.Services.ProductSubcategories.Models;
using Voupon.Merchant.WebApp.ViewModels;

namespace Voupon.Merchant.WebApp.Common.Services.ProductSubcategories.Queries
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
                    items = await rewardsDBContext.ProductSubCategories.AsNoTracking().Include(x => x.Category).Where(x => x.IsActivated == true).ToListAsync();
                }
                else
                    items = await rewardsDBContext.ProductSubCategories.AsNoTracking().Include(x => x.Category).Where(x=>x.CategoryId== request.CategoryId && x.IsActivated == true).ToListAsync();
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
