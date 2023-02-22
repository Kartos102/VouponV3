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
    public class ProductAdSubgroupsListQuery : IRequest<ApiResponseViewModel>
    {
       public int Id { get; set; }
    }
    public class ProductAdSubgroupsListQueryHandler : IRequestHandler<ProductAdSubgroupsListQuery, ApiResponseViewModel>
    {
        VodusV2Context vodusV2;
        RewardsDBContext rewardsDBContext;
        public ProductAdSubgroupsListQueryHandler(VodusV2Context vodusV2, RewardsDBContext rewardsDBContext)
        {
            this.vodusV2 = vodusV2;
            this.rewardsDBContext = rewardsDBContext;
        }
        public async Task<ApiResponseViewModel> Handle(ProductAdSubgroupsListQuery request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();
            try
            {
                var items =  vodusV2.ProductAdSubgroups.Include(x => x.ProductAd).Include(x => x.Subgroup).Where(x=> x.ProductAdId == request.Id).ToList();

                foreach(var item in items)
                {
                    item.Subgroup.ProductAdSubgroups = null;
                    item.ProductAd.ProductAdSubgroups = null;
                }
                response.Successful = true;
                response.Message = "Get Product Ad Subgroups List Successfully";
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
