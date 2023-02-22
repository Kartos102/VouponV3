using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Voupon.Merchant.WebApp.ViewModels;
using System.Threading;
using Voupon.Database.Postgres.RewardsEntities;
using MediatR;
using Microsoft.EntityFrameworkCore;


namespace Voupon.Merchant.WebApp.Common.Services.InStoreRedemptionTokens.Commands
{
    public class CreateInStoreRedemptionTokenCommand : IRequest<ApiResponseViewModel>
    {
        public string ProductTitle { get; set; }
        public int MasterMemberProfileId { get; set; }
        public int MerchantId { get; set; }
        public int ProductId { get; set; }
        public Guid OrderItemId { get; set; }
        public string RedemptionInfo { get; set; }
        public decimal Revenue { get; set; }
        public string Email { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime ExpiredDate { get; set; }
    }

    public class CreateInStoreRedemptionTokenCommandHandler : IRequestHandler<CreateInStoreRedemptionTokenCommand, ApiResponseViewModel>
    {
        RewardsDBContext rewardsDBContext;
        public CreateInStoreRedemptionTokenCommandHandler(RewardsDBContext rewardsDBContext)
        {
            this.rewardsDBContext = rewardsDBContext;
        }

        public async Task<ApiResponseViewModel> Handle(CreateInStoreRedemptionTokenCommand request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();
            try
            {
                Voupon.Database.Postgres.RewardsEntities.InStoreRedemptionTokens item = new Voupon.Database.Postgres.RewardsEntities.InStoreRedemptionTokens();
                item.StartDate = request.StartDate;
                item.ExpiredDate = request.ExpiredDate;
                item.ProductId = request.ProductId;
                item.IsActivated = true;
                item.IsRedeemed = false;
                item.MasterMemberProfileId = request.MasterMemberProfileId;
                item.MerchantId = request.MerchantId;
                item.OutletId = null;
                item.ProductTitle = request.ProductTitle;
                item.OrderItemId = request.OrderItemId;
                item.RedeemedAt = null;
                item.CreatedAt = DateTime.Now;
                item.RedemptionInfo = request.RedemptionInfo;
                item.Revenue = request.Revenue;
                item.Token = "";
                item.Email = request.Email;
                rewardsDBContext.InStoreRedemptionTokens.Add(item);
                rewardsDBContext.SaveChanges();
                item.Token = Voupon.Common.RedemptionTokenGenerator.InStoreRedemptionToken.GenerateToken(item.Id);
                rewardsDBContext.SaveChanges();

                if (item != null)
                {
                    response.Successful = true;
                    response.Message = "Create InStore Redemption Token Successfully";
                    response.Data = item;
                }
                else
                {
                    response.Successful = false;
                    response.Message = "No InStore Redemption Summary";
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
