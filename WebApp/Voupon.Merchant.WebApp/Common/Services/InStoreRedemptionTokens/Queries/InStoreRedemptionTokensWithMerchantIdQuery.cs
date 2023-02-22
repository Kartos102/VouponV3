using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Merchant.WebApp.Common.Services.Products.Models;
using Voupon.Merchant.WebApp.ViewModels;

namespace Voupon.Merchant.WebApp.Common.Services.InStoreRedemptionTokens.Queries
{
    public class InStoreRedemptionTokensViewModel : Voupon.Database.Postgres.RewardsEntities.InStoreRedemptionTokens
    {
        public string ShortOrderItemId { get; set; }
        public string ShortOrderId { get; set; }
    }
    public class InStoreRedemptionTokensWithMerchantIdQuery : IRequest<ApiResponseViewModel>
    {
        public int MerchantId { get; set; }
    }
    public class InStoreRedemptionTokensWithMerchantIdQueryHandler : IRequestHandler<InStoreRedemptionTokensWithMerchantIdQuery, ApiResponseViewModel>
    {
        RewardsDBContext rewardsDBContext;
        public InStoreRedemptionTokensWithMerchantIdQueryHandler(RewardsDBContext rewardsDBContext)
        {
            this.rewardsDBContext = rewardsDBContext;
        }

        public async Task<ApiResponseViewModel> Handle(InStoreRedemptionTokensWithMerchantIdQuery request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();
            try
            {
                List<InStoreRedemptionTokensViewModel> list = new List<InStoreRedemptionTokensViewModel>();
                var items = await rewardsDBContext.InStoreRedemptionTokens.Include(x => x.OrderItem).ThenInclude(x => x.Order).Where(x => x.MerchantId == request.MerchantId && x.OrderItem.Status != 3).ToListAsync();
                if (items != null)
                {
                    foreach (var item in items)
                    {
                        InStoreRedemptionTokensViewModel vm = new InStoreRedemptionTokensViewModel();
                        vm.CreatedAt = item.CreatedAt;
                        vm.Email = item.Email;
                        vm.ExpiredDate = item.ExpiredDate;
                        vm.Id = item.Id;
                        vm.IsActivated = item.IsActivated;
                        vm.IsRedeemed = item.IsRedeemed;
                        vm.MasterMemberProfileId = item.MasterMemberProfileId;
                        vm.Merchant = item.Merchant;
                        vm.MerchantId = item.MerchantId;
                        vm.OrderItem = item.OrderItem;
                        vm.OrderItemId = item.OrderItemId;
                        vm.Outlet = item.Outlet;
                        vm.OutletId = item.OutletId;
                        vm.ProductId = item.ProductId;
                        vm.ProductTitle = item.ProductTitle;
                        vm.RedeemedAt = item.RedeemedAt;
                        vm.RedemptionInfo = item.RedemptionInfo;
                        vm.Revenue = item.Revenue;
                        if (item.OrderItem != null)
                        {
                            vm.ShortOrderItemId = item.OrderItem.ShortId;
                            vm.ShortOrderId = item.OrderItem.Order.ShortId;
                        }
                        vm.StartDate = item.StartDate;
                        vm.Token = item.Token;
                        list.Add(vm);
                    }
                    response.Successful = true;
                    response.Message = "Get InStore Redemption Tokens Successfully";
                    response.Data = list;
                }
                else
                {
                    response.Successful = false;
                    response.Message = "No InStore Redemption Tokens";
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
