using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Database.Postgres.VodusEntities;
using Voupon.Rewards.WebApp.ViewModels;

namespace Voupon.Rewards.WebApp.Common.PartnerWebsites.Queries
{
    public class PartnerWebsiteQuery : IRequest<ApiResponseViewModel>
    {
        public int PartnerWebsiteId { get; set; }
    }
    public class ProductQueryOrderItemRedemptionTokenQueryHandler : IRequestHandler<PartnerWebsiteQuery, ApiResponseViewModel>
    {
        VodusV2Context vodusV2Context;
        private readonly IOptions<AppSettings> appSettings;
        public ProductQueryOrderItemRedemptionTokenQueryHandler(VodusV2Context vodusV2Context, IOptions<AppSettings> appSettings)
        {
            this.vodusV2Context = vodusV2Context;
            this.appSettings = appSettings;
        }

        public async Task<ApiResponseViewModel> Handle(PartnerWebsiteQuery request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel apiResponseViewModel = new ApiResponseViewModel();
            try
            {
                if(request.PartnerWebsiteId == 0)
                {
                    apiResponseViewModel.Successful = false;
                    apiResponseViewModel.Message = "Invalid request [1]";
                    return apiResponseViewModel;
                }

                var partnerWebsite = await vodusV2Context.PartnerWebsites.Where(x => x.Id == request.PartnerWebsiteId).FirstOrDefaultAsync();
                if(partnerWebsite == null)
                {

                    apiResponseViewModel.Successful = false;
                    apiResponseViewModel.Message = "Invalid request [2]";
                    return apiResponseViewModel;
                }

                apiResponseViewModel.Successful = true;
                apiResponseViewModel.Data = partnerWebsite.Name;
            }
            catch (Exception ex)
            {
                apiResponseViewModel.Message = ex.Message;
            }

            return apiResponseViewModel;
        }
    }
}
