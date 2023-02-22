using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Merchant.WebApp.ViewModels;

namespace Voupon.Merchant.WebApp.Common.Services.DeliveryRedemptionTokens.Commands
{
    public class CreateDeliveryRedemptionTokensCommand : IRequest<ApiResponseViewModel>
    {
        public string ProductTitle { get; set; }
        public int MasterMemberProfileId { get; set; }
        public int MerchantId { get; set; }
        public Guid OrderItemId { get; set; }
        public int ProductId { get; set; }
        public string RedemptionInfo { get; set; }
        public string Email { get; set; }
        public decimal Revenue { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime ExpiredDate { get; set; }
    }
    public class CreateDeliveryRedemptionTokensCommandHandler : IRequestHandler<CreateDeliveryRedemptionTokensCommand, ApiResponseViewModel>
    {
        RewardsDBContext rewardsDBContext;
        public CreateDeliveryRedemptionTokensCommandHandler(RewardsDBContext rewardsDBContext)
        {
            this.rewardsDBContext = rewardsDBContext;
        }

        public async Task<ApiResponseViewModel> Handle(CreateDeliveryRedemptionTokensCommand request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();
            try
            {
                Voupon.Database.Postgres.RewardsEntities.DeliveryRedemptionTokens item = new Voupon.Database.Postgres.RewardsEntities.DeliveryRedemptionTokens();
                item.CreatedAt = DateTime.Now;
                item.ProductId = request.ProductId;
                item.ExpiredDate = request.ExpiredDate;
                item.IsActivated = true;
                item.IsRedeemed = false;
                item.MasterMemberProfileId = request.MasterMemberProfileId;
                item.MerchantId = request.MerchantId;
                item.ProductTitle = request.ProductTitle;
                item.OrderItemId = request.OrderItemId;
                item.RedeemedAt = null;
                item.RedemptionInfo = request.RedemptionInfo;
                item.Revenue = request.Revenue;
                item.StartDate = request.StartDate;
                item.CourierProvider = "";
                item.Token = "";
                item.Email = request.Email;
                rewardsDBContext.DeliveryRedemptionTokens.Add(item);
                rewardsDBContext.SaveChanges();
                if (item!=null)
                {            
                    rewardsDBContext.SaveChanges();
                    response.Successful = true;
                    response.Message = "Create Delivery Redemption Tokens Successfully";
                    response.Data = item;
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
