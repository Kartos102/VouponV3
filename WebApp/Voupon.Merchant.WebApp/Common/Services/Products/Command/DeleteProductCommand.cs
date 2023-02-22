using MediatR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Common.Enum;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Merchant.WebApp.Common.Services.Products.Models;
using Voupon.Merchant.WebApp.ViewModels;

namespace Voupon.Merchant.WebApp.Common.Services.Products.Command
{
    public class DeleteProductCommand : IRequest<ApiResponseViewModel>
    {
        public int ProductId { get; set; }
        public DateTime LastUpdatedAt { get; set; }
        public Guid LastUpdatedByUserId { get; set; }
    }

    public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, ApiResponseViewModel>
    {
        RewardsDBContext rewardsDBContext;
        public DeleteProductCommandHandler(RewardsDBContext rewardsDBContext)
        {
            this.rewardsDBContext = rewardsDBContext;
        }

        public async Task<ApiResponseViewModel> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();

            var product = await rewardsDBContext.Products.FirstOrDefaultAsync(x => x.Id == request.ProductId);
            if (product != null)
            {
                product.LastUpdatedAt = request.LastUpdatedAt;
                product.LastUpdatedByUser = request.LastUpdatedByUserId;
                product.IsDeleted = true;
                product.IsActivated = false;
                rewardsDBContext.SaveChanges();
                response.Successful = true;
                response.Message = "Deleted Product Successfully";
            }
            else
            {
                response.Message = "Product not found";
            }
            return response;
        }
    }
}
