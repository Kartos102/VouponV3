using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Merchant.WebApp.Areas.Admin.Services.Merchants.ViewModels;
using Voupon.Merchant.WebApp.ViewModels;

namespace Voupon.Merchant.WebApp.Common.Services.ProductSubcategories.Commands
{
    public class UpdateProductSubCategoryCommand : IRequest<ApiResponseViewModel>
    {
        public int ProductSubCategoryId { get; set; }
        public int ProductCategoryId { get; set; }
        public string Name { get; set; }
        public DateTime LastUpdatedAt { get; set; }
        public Guid LastUpdatedByUserId { get; set; }

    }

    public class UpdateProductSubCategoryCommandHandler : IRequestHandler<UpdateProductSubCategoryCommand, ApiResponseViewModel>
    {
        RewardsDBContext rewardsDBContext;
        public UpdateProductSubCategoryCommandHandler(RewardsDBContext rewardsDBContext)
        {
            this.rewardsDBContext = rewardsDBContext;
        }

        public async Task<ApiResponseViewModel> Handle(UpdateProductSubCategoryCommand request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();
            
            var product = await rewardsDBContext.ProductSubCategories.FirstAsync(x => x.Id == request.ProductSubCategoryId);
            if(product != null)
            {
                product.Name = request.Name;
                product.CategoryId = request.ProductCategoryId;
                product.LastUpdatedAt = request.LastUpdatedAt;
                product.LastUpdatedByUserId = request.LastUpdatedByUserId;
                rewardsDBContext.SaveChanges();
                response.Successful = true;
                response.Message = "Update Product Sub-Category Successfully";
            }
            else
            {
                response.Message = "Product Sub-Category not found";
            }         
            return response;
        }
    }
}
