using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Merchant.WebApp.ViewModels;

namespace Voupon.Merchant.WebApp.Common.Services.ProductOutlets.Commands
{
    public class UpdateProductDiscountsStatusCommand : IRequest<ApiResponseViewModel>
    {
        public int ProductId { get; set; }
        public List<int> ProductDiscountId { get; set; }
        public DateTime LastUpdatedAt { get; set; }
        public Guid LastUpdatedByUserId { get; set; }

    }

    public class UpdateProductDiscountsStatusCommandHandler : IRequestHandler<UpdateProductDiscountsStatusCommand, ApiResponseViewModel>
    {
        RewardsDBContext rewardsDBContext;
        public UpdateProductDiscountsStatusCommandHandler(RewardsDBContext rewardsDBContext)
        {
            this.rewardsDBContext = rewardsDBContext;
        }

        public async Task<ApiResponseViewModel> Handle(UpdateProductDiscountsStatusCommand request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();
            try
            {
                var existingProductDiscount = rewardsDBContext.ProductDiscounts.Where(x => x.ProductId == request.ProductId);
                foreach(var item in existingProductDiscount)
                {
                    if (request.ProductDiscountId != null && request.ProductDiscountId.Count > 0)
                    {
                        if (request.ProductDiscountId.Contains(item.Id))
                            item.IsActivated = true;
                        else
                            item.IsActivated = false;
                    }
                    else
                    {
                        item.IsActivated = false;
                    }
                    item.LastUpdatedAt = request.LastUpdatedAt;
                    item.LastUpdatedByUserId = request.LastUpdatedByUserId;
                }
                await rewardsDBContext.SaveChangesAsync();      
                response.Successful = true;
                response.Message = "Update Product Discount Status Successfully";
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }
            return response;
        }
    }
}
