using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Merchant.WebApp.ViewModels;

namespace Voupon.Merchant.WebApp.Common.Services.DeliveryRedemptionTokens.Queries
{
    public class DeliveryRedemptionTokensViewModel : Voupon.Database.Postgres.RewardsEntities.DeliveryRedemptionTokens
    {
        public string ShortOrderItemId { get; set; }
        public string ShortOrderId { get; set; }
    }

    public class DeliveryRedemptionTokensWithMerchantIdQuery : IRequest<ApiResponseViewModel>
    {
        public int MerchantId { get; set; }
    }
    public class DeliveryRedemptionTokensWithMerchantIdQueryHandler : IRequestHandler<DeliveryRedemptionTokensWithMerchantIdQuery, ApiResponseViewModel>
    {
        RewardsDBContext rewardsDBContext;
        public DeliveryRedemptionTokensWithMerchantIdQueryHandler(RewardsDBContext rewardsDBContext)
        {
            this.rewardsDBContext = rewardsDBContext;
        }

        public async Task<ApiResponseViewModel> Handle(DeliveryRedemptionTokensWithMerchantIdQuery request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();
            try
            {
                List<DeliveryRedemptionTokensViewModel> list = new List<DeliveryRedemptionTokensViewModel>();
                var items = await rewardsDBContext.DeliveryRedemptionTokens.Include(x => x.OrderItem.Order).Where(x => x.MerchantId == request.MerchantId && x.OrderItem.Status != 3).ToListAsync();
                if (items != null)
                {
                    foreach (var item in items)
                    {
                        DeliveryRedemptionTokensViewModel vm = new DeliveryRedemptionTokensViewModel();
                        vm.CreatedAt = item.CreatedAt;
                        vm.Email = item.Email;
                        vm.ExpiredDate = item.ExpiredDate;
                        vm.Id = item.Id;
                        vm.IsActivated = item.IsActivated;
                        vm.IsRedeemed = item.IsRedeemed;
                        vm.MasterMemberProfileId = item.MasterMemberProfileId;
                        vm.MerchantId = item.MerchantId;
                        vm.OrderItemId = item.OrderItemId;
                        vm.ProductId = item.ProductId;
                        vm.ProductTitle = item.ProductTitle;
                        vm.RedeemedAt = item.RedeemedAt;
                        vm.RedemptionInfo = item.RedemptionInfo;
                        vm.Revenue = item.Revenue;
                        vm.ShortOrderItemId = item.OrderItem.ShortId;
                        vm.ShortOrderId = item.OrderItem.Order.ShortId;
                        vm.StartDate = item.StartDate;
                        vm.Token = item.Token;
                        vm.CourierProvider = item.CourierProvider;
                        vm.UpdateTokenAt = item.UpdateTokenAt;
                        list.Add(vm);
                    }
                    response.Successful = true;
                    response.Message = "Get Delivery Redemption Tokens Successfully";
                    response.Data = list;
                }
                else
                {
                    response.Successful = false;
                    response.Message = "No Delivery Redemption Tokens";
                    response.Data = null;
                }
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }

            return response;
        }
    }
}
