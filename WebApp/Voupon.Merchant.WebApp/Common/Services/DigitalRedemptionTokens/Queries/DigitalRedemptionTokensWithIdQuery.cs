using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Merchant.WebApp.ViewModels;

namespace Voupon.Merchant.WebApp.Common.Services.DigitalRedemptionTokens.Queries
{
    public class DigitalRedemptionTokensWithIdQuery : IRequest<ApiResponseViewModel>
    {
        public int Id { get; set; }
    }
    public class DigitalRedemptionTokensWithIdQueryHandler : IRequestHandler<DigitalRedemptionTokensWithIdQuery, ApiResponseViewModel>
    {
        RewardsDBContext rewardsDBContext;
        public DigitalRedemptionTokensWithIdQueryHandler(RewardsDBContext rewardsDBContext)
        {
            this.rewardsDBContext = rewardsDBContext;
        }

        public async Task<ApiResponseViewModel> Handle(DigitalRedemptionTokensWithIdQuery request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();
            try
            {

                var item = await rewardsDBContext.DigitalRedemptionTokens.FirstOrDefaultAsync(x => x.Id == request.Id);
                if (item != null)
                {

                    DigitalRedemptionTokensViewModel vm = new DigitalRedemptionTokensViewModel();
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
                    response.Message = "Get Digital Redemption Tokens Successfully";
                    response.Data = vm;
                }
                else
                {
                    response.Successful = false;
                    response.Message = "No Digital Redemption Tokens";
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
