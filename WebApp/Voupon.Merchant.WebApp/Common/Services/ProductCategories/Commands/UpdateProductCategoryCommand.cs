using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Merchant.WebApp.ViewModels;

namespace Voupon.Merchant.WebApp.Common.Services.ProductCategories.Commands
{
    public class UpdateProductCategoryCommand : IRequest<ApiResponseViewModel>
    {
        public int ProductCategoryId { get; set; }
        public string Name { get; set; }
        public DateTime LastUpdatedAt { get; set; }
        public Guid LastUpdatedByUserId { get; set; }
    }

    public class UpdateProductCategoryCommandHandler : IRequestHandler<UpdateProductCategoryCommand, ApiResponseViewModel>
    {
        RewardsDBContext rewardsDBContext;
        public UpdateProductCategoryCommandHandler(RewardsDBContext rewardsDBContext)
        {
            this.rewardsDBContext = rewardsDBContext;
        }

        public async Task<ApiResponseViewModel> Handle(UpdateProductCategoryCommand request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();

            var merchant = await rewardsDBContext.ProductCategories.FirstAsync(x => x.Id == request.ProductCategoryId);
            if (merchant != null)
            {
                merchant.Name = request.Name;
                merchant.LastUpdatedAt = request.LastUpdatedAt;
                merchant.LastUpdatedByUserId = request.LastUpdatedByUserId;
                rewardsDBContext.SaveChanges();
                response.Successful = true;
                response.Message = "Update Product Category  Successfully";
            }
            else
            {
                response.Message = "Product Category not found";
            }
            return response;
        }
    }
}
