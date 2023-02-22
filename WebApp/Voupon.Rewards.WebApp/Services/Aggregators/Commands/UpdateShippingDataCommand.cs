using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Rewards.WebApp.ViewModels;

namespace Voupon.Rewards.WebApp.Services.Aggregators.Commands
{
    public class UpdateShippingDataCommand : IRequest<ApiResponseViewModel>
    {
        public string ExternalOrderId { get; set; }
        public string ShippingJson { get; set; }
        public string LatestShippingStatus { get; set; }

        public string AccountJson { get; set; }
    }
    public class AddToCartCommandHandler : IRequestHandler<UpdateShippingDataCommand, ApiResponseViewModel>
    {
        private readonly RewardsDBContext rewardsDBContext;

        public AddToCartCommandHandler(RewardsDBContext rewardsDBContext)
        {
            this.rewardsDBContext = rewardsDBContext;
        }

        public async Task<ApiResponseViewModel> Handle(UpdateShippingDataCommand request, CancellationToken cancellationToken)
        {
            var apiResponseViewModel = new ApiResponseViewModel();
            try
            {
                var order = await rewardsDBContext.OrderShopExternal.Include(x=> x.OrderItemExternal).Where(x => x.ExternalOrderId == request.ExternalOrderId).FirstOrDefaultAsync();
                if (order == null)
                {
                    apiResponseViewModel.Successful = false;
                    apiResponseViewModel.Message = "Order is not in the system ";
                    return apiResponseViewModel;
                }
                order.ShippingDetailsJson = request.ShippingJson;
                order.ShippingLatestStatus = request.LatestShippingStatus;
                order.AdminAccountDetail = request.AccountJson;
                order.LastUpdatedAt = DateTime.Now;
                if(order.ShippingLatestStatus == "Parcel has been delivered")
                {
                    order.OrderShippingExternalStatus = 6;
                    foreach (var item in order.OrderItemExternal)
                    {
                        if (item.OrderItemExternalStatus == 3) { 
                            item.OrderItemExternalStatus = 6; 
                        }
                    }
                }

                rewardsDBContext.OrderShopExternal.Update(order);
                await rewardsDBContext.SaveChangesAsync();

                apiResponseViewModel.Successful = true;
                return apiResponseViewModel;
            }
            catch (Exception ex)
            {
                apiResponseViewModel.Successful = false;
                apiResponseViewModel.Message = "Fail to Add to Cart";
                return apiResponseViewModel;

            }
        }
    }
}
