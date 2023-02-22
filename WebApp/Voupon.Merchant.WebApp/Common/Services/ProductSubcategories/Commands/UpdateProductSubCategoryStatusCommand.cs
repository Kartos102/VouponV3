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

namespace Voupon.Merchant.WebApp.Common.Services.ProductSubcategories.Commandspdate
{
    public class UpdateProductSubCategoryStatusCommand : IRequest<ApiResponseViewModel>
    {
        public int ProductSubCategoryId { get; set; }
        public bool Status { get; set; }
        public DateTime LastUpdatedAt { get; set; }
        public Guid LastUpdatedByUserId { get; set; }

    }

    public class UpdateProductSubCategoryStatusCommandHandler : IRequestHandler<UpdateProductSubCategoryStatusCommand, ApiResponseViewModel>
    {
        RewardsDBContext rewardsDBContext;
        public UpdateProductSubCategoryStatusCommandHandler(RewardsDBContext rewardsDBContext)
        {
            this.rewardsDBContext = rewardsDBContext;
        }

        public async Task<ApiResponseViewModel> Handle(UpdateProductSubCategoryStatusCommand request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();
            
            var product = await rewardsDBContext.ProductSubCategories.FirstAsync(x => x.Id == request.ProductSubCategoryId);
            if(product != null)
            {
                product.LastUpdatedAt = request.LastUpdatedAt;
                product.LastUpdatedByUserId = request.LastUpdatedByUserId;
                product.IsActivated = request.Status;             
                rewardsDBContext.SaveChanges();
                response.Successful = true;
                response.Message = "Update Product Sub-Category Status Successfully";
            }
            else
            {
                response.Message = "Product Sub-Category not found";
            }         
            return response;
        }
    }
}
