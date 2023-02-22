using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Voupon.Merchant.WebApp.ViewModels;
using Voupon.Database.Postgres.VodusEntities;
using Voupon.Database.Postgres.RewardsEntities;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using Voupon.Merchant.WebApp.Areas.Admin.ViewModels.ProductAds;

namespace Voupon.Merchant.WebApp.Areas.Admin.Services.ProductAds.Queries
{
    public class GetProductAdsConfigImpressionIdentifierQuery : IRequest<ApiResponseViewModel>
    {
        public int Id { get; set; }
    }
    public class GetProductAdsConfigImpressionIdentifierQueryHandler : IRequestHandler<GetProductAdsConfigImpressionIdentifierQuery, ApiResponseViewModel>
    {
        VodusV2Context vodusV2;
        RewardsDBContext rewardsDBContext;
        public GetProductAdsConfigImpressionIdentifierQueryHandler(VodusV2Context vodusV2, RewardsDBContext rewardsDBContext)
        {
            this.vodusV2 = vodusV2;
        }
        public async Task<ApiResponseViewModel> Handle(GetProductAdsConfigImpressionIdentifierQuery request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();
            try
            {
                var productAd = await vodusV2.ProductAdsConfig.FirstOrDefaultAsync();
                if (productAd != null)
                {
                    response.Successful = true;
                    response.Message = "Got Product Ads Impression Identifier Successfully";
                    response.Data = productAd.ImpressionCountIdentifier;
                }
                else
                {
                    response.Message = "Fail to Get Product Ads Impression Identifier";
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
