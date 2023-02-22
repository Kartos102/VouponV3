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

    public class DeliveryRedemptionTokensWithIdQuery : IRequest<ApiResponseViewModel>
    {
        public int Id { get; set; }
    }
    public class DeliveryRedemptionTokensWithIdQueryHandler : IRequestHandler<DeliveryRedemptionTokensWithIdQuery, ApiResponseViewModel>
    {
        RewardsDBContext rewardsDBContext;
        public DeliveryRedemptionTokensWithIdQueryHandler(RewardsDBContext rewardsDBContext)
        {
            this.rewardsDBContext = rewardsDBContext;
        }

        public async Task<ApiResponseViewModel> Handle(DeliveryRedemptionTokensWithIdQuery request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();
            try
            {

                var item = await rewardsDBContext.DeliveryRedemptionTokens.FirstOrDefaultAsync(x => x.Id == request.Id);
                if (item != null)
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
                    vm.ShortOrderItemId = item.CreatedAt.ToString("yyMMdd") + item.OrderItemId.ToString().Split("-").First().ToUpper();
                    vm.StartDate = item.StartDate;
                    vm.Token = item.Token;
                    vm.UpdateTokenAt = item.UpdateTokenAt;
                    response.Successful = true;
                    response.Message = "Get Delivery Redemption Tokens Successfully";
                    response.Data = vm;
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
