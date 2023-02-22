using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Merchant.WebApp.Infrastructure.Enums;
using Voupon.Merchant.WebApp.ViewModels;

namespace Voupon.Merchant.WebApp.Common.Services.DigitalRedemptionTokens.Commands
{
    public class UpdateDigitalRedemptionTokensCommand : IRequest<ApiResponseViewModel>
    {
        public int Id { get; set; }
        public string Token { get; set; }
    }
    public class UpdateDigitalRedemptionTokensCommandHandler : IRequestHandler<UpdateDigitalRedemptionTokensCommand, ApiResponseViewModel>
    {
        RewardsDBContext rewardsDBContext;
        public UpdateDigitalRedemptionTokensCommandHandler(RewardsDBContext rewardsDBContext)
        {
            this.rewardsDBContext = rewardsDBContext;
        }

        public async Task<ApiResponseViewModel> Handle(UpdateDigitalRedemptionTokensCommand request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();
            try
            {
                var item = await rewardsDBContext.DigitalRedemptionTokens.Where(x => x.Id == request.Id).FirstOrDefaultAsync();
                if (item != null)
                {
                    var orderItem = await rewardsDBContext.OrderItems.Where(x => x.Id == item.OrderItemId).FirstOrDefaultAsync();
                    if(orderItem != null)
                    {
                        orderItem.Status = (byte)OrderStatus.Completed;
                    }
                
                    item.Token = request.Token;
                    item.UpdateTokenAt = DateTime.Now;
                    item.RedeemedAt = DateTime.Now;
                    item.IsRedeemed = true;

                    rewardsDBContext.OrderItems.Update(orderItem);
                    rewardsDBContext.DigitalRedemptionTokens.Update(item);
                    rewardsDBContext.SaveChanges();
                    
                    response.Successful = true;
                    response.Message = "Update Digital Redemption Tokens Successfully";
                    response.Data = item;
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
