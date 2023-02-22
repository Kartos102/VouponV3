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
    public class ProductAdsPartnerDomainsListQuery : IRequest<ApiResponseViewModel>
    {
       public int Id { get; set; }
    }
    public class ProductAdsPartnerDomainsListQueryHandler : IRequestHandler<ProductAdsPartnerDomainsListQuery, ApiResponseViewModel>
    {
        VodusV2Context vodusV2;
        RewardsDBContext rewardsDBContext;
        public ProductAdsPartnerDomainsListQueryHandler(VodusV2Context vodusV2, RewardsDBContext rewardsDBContext)
        {
            this.vodusV2 = vodusV2;
            this.rewardsDBContext = rewardsDBContext;
        }
        public async Task<ApiResponseViewModel> Handle(ProductAdsPartnerDomainsListQuery request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();
            try
            {
                var items =  vodusV2.ProductAdPartnersDomain.Include(x=> x.ProductAdPartnersDomainWebsites).ThenInclude(y => y.PartnerWebsite).Include(x => x.ProductAd).Include(x=> x.Partner).Where(x=> x.ProductAdId == request.Id).ToList();

                foreach(var item in items)
                {
                    foreach(var ProductAdPartnersDomainWebsite in item.ProductAdPartnersDomainWebsites)
                    {
                        ProductAdPartnersDomainWebsite.ProductAdPartnersDomain = null;
                        ProductAdPartnersDomainWebsite.PartnerWebsite.ProductAdPartnersDomainWebsites = null;
                    }
                    item.Partner.ProductAdPartnersDomain = null;
                    item.Partner.PartnerWebsites = null;
                    item.ProductAd.ProductAdPartnersDomain = null;
                }
                response.Successful = true;
                response.Message = "Get Product Ad Partner Domains List Successfully";
                response.Data = items;
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }

            return response;
        }
    }
}
