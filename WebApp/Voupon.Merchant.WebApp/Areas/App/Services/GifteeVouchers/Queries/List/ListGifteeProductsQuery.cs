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
    public class ListGifteeProductsQuery : IRequest<ApiResponseViewModel>
    {
    }

    public class ListGifteeProductsQueryHandler : IRequestHandler<ListGifteeProductsQuery, ApiResponseViewModel>
    {
        RewardsDBContext rewardsDBContext;
        public ListGifteeProductsQueryHandler(RewardsDBContext rewardsDBContext)
        {
            this.rewardsDBContext = rewardsDBContext;
        }

        public async Task<ApiResponseViewModel> Handle(ListGifteeProductsQuery request, CancellationToken cancellationToken)
        {
            var apiResponseViewModel = new ApiResponseViewModel();
            var gifteeThirdPartyType = await rewardsDBContext.ThirdPartyTypes.Where(x => x.Name == "Giftee").FirstOrDefaultAsync();
            if (gifteeThirdPartyType == null)
            {
                apiResponseViewModel.Successful = false;
                apiResponseViewModel.Message = "Could not get Giftee Third party type [001]";
                return apiResponseViewModel;
            }
            var gifteeProducts = await rewardsDBContext.ThirdPartyProducts.Where(x => x.ThirdPartyTypeId == gifteeThirdPartyType.Id).ToListAsync();
            if (gifteeProducts == null)
            {
                apiResponseViewModel.Successful = false;
                apiResponseViewModel.Message = "Could not get Giftee Products [002]";
                return apiResponseViewModel;
            }
            foreach(var product in gifteeProducts)
            {
                product.ThirdPartyType = null;
            }
            apiResponseViewModel.Successful = true;
            apiResponseViewModel.Data = gifteeProducts;
            return apiResponseViewModel;
        }
    }
}
