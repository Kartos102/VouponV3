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
using Voupon.Merchant.WebApp.Common.Services.Merchants.Models;
using Voupon.Merchant.WebApp.ViewModels;

namespace Voupon.Merchant.WebApp.Common.Services.Merchants.Commands
{
    public class UpdateMerchantDefaultCommissionCommand : IRequest<ApiResponseViewModel>
    {
        public int MerchantId { get; set; }
        public decimal DefaultCommission { get; set; }
        public DateTime LastUpdatedAt { get; set; }
        public Guid LastUpdatedByUserId { get; set; }
    }

    public class UpdateMerchantDefaultCommissionCommandHandler : IRequestHandler<UpdateMerchantDefaultCommissionCommand, ApiResponseViewModel>
    {
        RewardsDBContext rewardsDBContext;
        public UpdateMerchantDefaultCommissionCommandHandler(RewardsDBContext rewardsDBContext)
        {
            this.rewardsDBContext = rewardsDBContext;
        }

        public async Task<ApiResponseViewModel> Handle(UpdateMerchantDefaultCommissionCommand request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();

            var merchant = await rewardsDBContext.Merchants.FirstAsync(x => x.Id == request.MerchantId);
            if (merchant != null)
            {
                merchant.DefaultCommission = request.DefaultCommission;
                merchant.LastUpdatedAt = request.LastUpdatedAt;
                merchant.LastUpdatedByUserId = request.LastUpdatedByUserId;
                var productList = await rewardsDBContext.Products.Where(x => x.MerchantId == request.MerchantId).ToListAsync();
                if(productList != null)
                {
                    foreach(var product in productList)
                    {
                        product.DefaultCommission = request.DefaultCommission;
                    }
                }
                rewardsDBContext.SaveChanges();
                response.Successful = true;
                response.Message = "Update Merchant Default Commission Successfully";
            }
            else
            {
                response.Message = "Merchant not found";
            }
            return response;
        }
    }
}
