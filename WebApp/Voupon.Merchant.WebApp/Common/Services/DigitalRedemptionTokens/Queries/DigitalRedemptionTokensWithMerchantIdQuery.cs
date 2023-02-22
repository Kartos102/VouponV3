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
    public class DigitalRedemptionTokensViewModel : Voupon.Database.Postgres.RewardsEntities.DigitalRedemptionTokens
    {
        //public int Id { get; set; }
        public string ShortOrderItemId { get; set; }
        public string ShortOrderId { get; set; }
        //public int MasterMemberProfileId { get; set; }
        //public int MerchantId { get; set; }
        //public Guid OrderItemId { get; set; }
        //public int ProductId { get; set; }
        //public string ProductTitle { get; set; }
        //public string Token { get; set; }
        //public DateTime StartDate { get; set; }
        //public DateTime ExpiredDate { get; set; }
        //public string RedemptionInfo { get; set; }
        //public bool IsRedeemed { get; set; }
        //public bool IsActivated { get; set; }
        //public DateTime CreatedAt { get; set; }
        //public DateTime? RedeemedAt { get; set; }
        //public decimal? Revenue { get; set; }
        //public string Email { get; set; }
        //public DateTime? UpdateTokenAt { get; set; }
    }

    public class DigitalRedemptionTokensWithMerchantIdQuery : IRequest<ApiResponseViewModel>
    {
        public int MerchantId { get; set; }
    }
    public class DigitalRedemptionTokensWithMerchantIdQueryHandler : IRequestHandler<DigitalRedemptionTokensWithMerchantIdQuery, ApiResponseViewModel>
    {
        RewardsDBContext rewardsDBContext;
        public DigitalRedemptionTokensWithMerchantIdQueryHandler(RewardsDBContext rewardsDBContext)
        {
            this.rewardsDBContext = rewardsDBContext;
        }

        public async Task<ApiResponseViewModel> Handle(DigitalRedemptionTokensWithMerchantIdQuery request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();
            try
            {
                List<DigitalRedemptionTokensViewModel> list = new List<DigitalRedemptionTokensViewModel>();
                var items = await rewardsDBContext.DigitalRedemptionTokens.Include(x => x.OrderItem.Order).Where(x => x.MerchantId == request.MerchantId && x.OrderItem.Status != 3).ToListAsync();
                if (items != null)
                {
                    foreach (var item in items)
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
                        vm.ShortOrderItemId = item.OrderItem.ShortId;
                        vm.ShortOrderId = item.OrderItem.Order.ShortId;
                        vm.StartDate = item.StartDate;
                        vm.Token = item.Token;
                        vm.UpdateTokenAt = item.UpdateTokenAt;
                        list.Add(vm);
                    }
                    response.Successful = true;
                    response.Message = "Get Digital Redemption Tokens Successfully";
                    response.Data = list;
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
