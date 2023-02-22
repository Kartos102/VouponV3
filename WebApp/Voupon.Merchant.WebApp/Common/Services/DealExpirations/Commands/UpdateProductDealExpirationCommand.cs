using MediatR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Merchant.WebApp.ViewModels;

namespace Voupon.Merchant.WebApp.Common.Services.DealExpirations.Commands
{
    public class UpdateProductDealExpirationCommand : IRequest<ApiResponseViewModel>
    {
        public int ProductId { get; set; }
        public int ExpirationTypeId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime ExpiredDate { get; set; }
        public int TotalValidDays { get; set; }
        public DateTime LastUpdatedAt { get; set; }
        public Guid LastUpdatedByUserId { get; set; }
    }

    public class UpdateProductDealExpirationCommandHandler : IRequestHandler<UpdateProductDealExpirationCommand, ApiResponseViewModel>
    {
        RewardsDBContext rewardsDBContext;
        public UpdateProductDealExpirationCommandHandler(RewardsDBContext rewardsDBContext)
        {
            this.rewardsDBContext = rewardsDBContext;
        }

        public async Task<ApiResponseViewModel> Handle(UpdateProductDealExpirationCommand request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();

            var dealExpirations = await rewardsDBContext.DealExpirations.FirstOrDefaultAsync(x => x.ProductId == request.ProductId);

            if (dealExpirations != null)
            {
                dealExpirations.ExpirationTypeId = request.ExpirationTypeId;
                if (request.ExpirationTypeId == 1)
                    dealExpirations.TotalValidDays = request.TotalValidDays;
                else
                {
                    dealExpirations.StartDate = request.StartDate;
                    dealExpirations.ExpiredDate = request.ExpiredDate;
                }
                dealExpirations.LastUpdatedAt = request.LastUpdatedAt;
                dealExpirations.LastUpdatedByUserId = request.LastUpdatedByUserId;
                rewardsDBContext.SaveChanges();
                var product=await rewardsDBContext.Products.FirstOrDefaultAsync(x => x.Id == dealExpirations.ProductId);
                product.DealExpirationId = dealExpirations.Id;
                rewardsDBContext.SaveChanges();
                response.Successful = true;
                response.Message = "Update Product Expiration Successfully";
            }
            else
            {
                var product = await rewardsDBContext.Products.FirstOrDefaultAsync(x => x.Id == request.ProductId);
                var newItem = new Voupon.Database.Postgres.RewardsEntities.DealExpirations();
                newItem.Product = product;
                newItem.ExpirationTypeId = request.ExpirationTypeId;
                newItem.ProductId = request.ProductId;
                if (request.ExpirationTypeId == 1)
                    newItem.TotalValidDays = request.TotalValidDays;
                else
                {
                    newItem.StartDate = request.StartDate;
                    newItem.ExpiredDate = request.ExpiredDate;
                }
                newItem.CreatedAt = request.LastUpdatedAt;
                newItem.CreatedByUserId = request.LastUpdatedByUserId;
                product.DealExpiration = newItem;
                await rewardsDBContext.DealExpirations.AddAsync(newItem);
                rewardsDBContext.SaveChanges();
                var product1 = await rewardsDBContext.Products.FirstOrDefaultAsync(x => x.Id == request.ProductId);
                product1.DealExpirationId = newItem.Id;
                rewardsDBContext.SaveChanges();
                response.Successful = true;
                response.Message = "Update Product Expiration Successfully";
            }
            return response;
        }
    }
}
