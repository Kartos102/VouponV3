using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Rewards.WebApp.ViewModels;

namespace Voupon.Rewards.WebApp.Services.Order.Pages
{
    public class InStoreRedemptionPage : IRequest<ApiResponseViewModel>
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public string Token { get; set; }

        public int MasterMemberProfileId { get; set; }
        private class InStoreRedemptionPageHandler : IRequestHandler<InStoreRedemptionPage, ApiResponseViewModel>
        {
            RewardsDBContext rewardsDBContext;
            private readonly IOptions<AppSettings> appSettings;
            public InStoreRedemptionPageHandler(RewardsDBContext rewardsDBContext, IOptions<AppSettings> appSettings)
            {
                this.rewardsDBContext = rewardsDBContext;
                this.appSettings = appSettings;
            }

            public async Task<ApiResponseViewModel> Handle(InStoreRedemptionPage request, CancellationToken cancellationToken)
            {
                var apiResponseViewModel = new ApiResponseViewModel();

                //var token = await rewardsDBContext.InStoreRedemptionTokens.Where(x => x.MasterMemberProfileId == request.MasterMemberProfileId && x.Token == request.Token).FirstOrDefaultAsync();
                var token = await rewardsDBContext.InStoreRedemptionTokens.Where(x => x.OrderItemId == request.Id).FirstOrDefaultAsync();
                if (token == null)
                {
                    apiResponseViewModel.Successful = false;
                    apiResponseViewModel.Message = "Invalid Request [001]";
                    return apiResponseViewModel;
                }

                var viewModel = new InStoreRedemptionPageViewModel
                {
                    Id = token.Id,
                    IsRedeemed = token.IsRedeemed,
                    MasterMemberProfileId = token.MasterMemberProfileId,
                    ProductId = token.ProductId,
                    ProductTitle = token.ProductTitle,
                    Token = token.Token,
                    url = appSettings.Value.App.VouponMerchantAppBaseUrl + "/qr/v/" + token.Token
                };

                apiResponseViewModel.Data = viewModel;
                apiResponseViewModel.Successful = true;
                return apiResponseViewModel;
            }
        }
        public class InStoreRedemptionPageViewModel
        {
            public string url { get; set; }
            public int Id { get; set; }
            public int MasterMemberProfileId { get; set; }
            public int MerchantId { get; set; }
            public int ProductId { get; set; }
            public Guid OrderItemId { get; set; }
            public string ProductTitle { get; set; }
            public string Token { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime ExpiredDate { get; set; }
            public string RedemptionInfo { get; set; }
            public bool IsRedeemed { get; set; }
            public bool IsActivated { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime? RedeemedAt { get; set; }
            public decimal? Revenue { get; set; }
            public int? OutletId { get; set; }
            public string Email { get; set; }
        }

    }

}
