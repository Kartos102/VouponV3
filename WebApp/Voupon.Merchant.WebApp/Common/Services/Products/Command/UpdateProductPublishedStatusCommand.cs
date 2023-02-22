using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Merchant.WebApp.ViewModels;

namespace Voupon.Merchant.WebApp.Common.Services.Products.Command
{
    public class UpdateProductPublishedStatusCommand : IRequest<ApiResponseViewModel>
    {
        public int ProductId { get; set; }
        public bool Status { get; set; }
        public DateTime LastUpdatedAt { get; set; }
        public Guid LastUpdatedByUserId { get; set; }

    }

    public class UpdateProductPublishedStatusCommandHandler : IRequestHandler<UpdateProductPublishedStatusCommand, ApiResponseViewModel>
    {
        RewardsDBContext rewardsDBContext;
        public UpdateProductPublishedStatusCommandHandler(RewardsDBContext rewardsDBContext)
        {
            this.rewardsDBContext = rewardsDBContext;
        }

        public async Task<ApiResponseViewModel> Handle(UpdateProductPublishedStatusCommand request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();            
            var merchant = await rewardsDBContext.Products.FirstAsync(x => x.Id == request.ProductId);
            if(merchant!=null)
            {
                merchant.LastUpdatedAt = request.LastUpdatedAt;
                merchant.LastUpdatedByUser = request.LastUpdatedByUserId;
                merchant.IsPublished = request.Status;             
                rewardsDBContext.SaveChanges();
                response.Successful = true;
                response.Message = "Update Product Published Successfully";
            }
            else
            {
                response.Message = "Product not found";
            }         
            return response;
        }
    }
}
