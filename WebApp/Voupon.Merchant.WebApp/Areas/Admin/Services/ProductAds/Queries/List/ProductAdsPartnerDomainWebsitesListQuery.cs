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
    public class ProductAdsPartnerDomainWebsitesListQuery : IRequest<ApiResponseViewModel>
    {
       public int Id { get; set; }
    }
    public class ProductAdsPartnerDomainWebsitesListQueryHandler : IRequestHandler<ProductAdsPartnerDomainWebsitesListQuery, ApiResponseViewModel>
    {
        VodusV2Context vodusV2;
        RewardsDBContext rewardsDBContext;
        public ProductAdsPartnerDomainWebsitesListQueryHandler(VodusV2Context vodusV2, RewardsDBContext rewardsDBContext)
        {
            this.vodusV2 = vodusV2;
            this.rewardsDBContext = rewardsDBContext;
        }
        public async Task<ApiResponseViewModel> Handle(ProductAdsPartnerDomainWebsitesListQuery request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();
            try
            {
                var items =  vodusV2.ProductAdPartnersDomainWebsites.Include(x => x.ProductAdPartnersDomain).Include(x => x.PartnerWebsite).Where(x=> x.ProductAdPartnersDomainId == request.Id).ToList();

                foreach(var item in items)
                {
                    item.ProductAdPartnersDomain.ProductAdPartnersDomainWebsites= null;
                    item.PartnerWebsite.ProductAdPartnersDomainWebsites = null;
                }
                response.Successful = true;
                response.Message = "Get Product Ad partner Domain Websites List Successfully";
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
