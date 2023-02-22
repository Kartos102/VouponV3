using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Common.BaseTypes;
using Voupon.Database.Postgres.RewardsEntities;
using Microsoft.EntityFrameworkCore;
using Voupon.Merchant.WebApp.ViewModels;

namespace Voupon.Merchant.WebApp.Areas.App.Services.GifteeVouchers.Queries.List
{
    public class ListGifteeVouchersQuery : IRequest<ApiResponseViewModel>
    {
    }

    public class ListGifteeVouchersQueryyHandler : IRequestHandler<ListGifteeVouchersQuery, ApiResponseViewModel>
    {
        RewardsDBContext rewardsDBContext;
        public ListGifteeVouchersQueryyHandler(RewardsDBContext rewardsDBContext)
        {
            this.rewardsDBContext = rewardsDBContext;
        }

        public async Task<ApiResponseViewModel> Handle(ListGifteeVouchersQuery request, CancellationToken cancellationToken)
        {
            var apiResponseViewModel = new ApiResponseViewModel();
            var gifteeVouchers = await rewardsDBContext.GifteeTokens.OrderByDescending(x => x.IssuedDate).ToListAsync();
            if (gifteeVouchers == null)
            {
                apiResponseViewModel.Successful = false;
                apiResponseViewModel.Message = "Invalid GifteeVouchers [001]";
                return apiResponseViewModel;
            }
            apiResponseViewModel.Successful = true;
            apiResponseViewModel.Data = gifteeVouchers;
            return apiResponseViewModel;
        }
    }
}
