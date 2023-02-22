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
    public class UpdateProductOutletsCommand : IRequest<ApiResponseViewModel>
    {
        public int ProductId { get; set; }
        public List<int> OutletsId { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid CreatedByUserId { get; set; }

    }

    public class UpdateProductOutletsCommandHandler : IRequestHandler<UpdateProductOutletsCommand, ApiResponseViewModel>
    {
        RewardsDBContext rewardsDBContext;
        public UpdateProductOutletsCommandHandler(RewardsDBContext rewardsDBContext)
        {
            this.rewardsDBContext = rewardsDBContext;
        }

        public async Task<ApiResponseViewModel> Handle(UpdateProductOutletsCommand request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();
            try
            {
                var existingOutLet = rewardsDBContext.ProductOutlets.Where(x => x.ProductId == request.ProductId);
                rewardsDBContext.ProductOutlets.RemoveRange(existingOutLet);
                if (request.OutletsId != null)
                    foreach (int outletId in request.OutletsId)
                    {
                        rewardsDBContext.ProductOutlets.Add(new Voupon.Database.Postgres.RewardsEntities.ProductOutlets() { ProductId = request.ProductId, OutletId = outletId, CreatedAt = request.CreatedAt, CreatedByUserId = request.CreatedByUserId });
                    }
                rewardsDBContext.SaveChanges();
                response.Successful = true;
                response.Message = "Update Product Outlets Successfully";
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }
            return response;
        }
    }
}
