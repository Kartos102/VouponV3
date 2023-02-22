using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Common.BaseTypes;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Merchant.WebApp.Areas.Admin.Services.Dashboard.Models;
using Voupon.Merchant.WebApp.ViewModels;

namespace Voupon.Merchant.WebApp.Areas.Admin.Services.Dashboard.Queries.Single
{
    public class DashboardQuery : IRequest<ApiResponseViewModel>
    {        
    }
    public class DashboardQueryHandler : IRequestHandler<DashboardQuery, ApiResponseViewModel>
    {
        RewardsDBContext rewardsDBContext;
        public DashboardQueryHandler(RewardsDBContext rewardsDBContext)
        {
            this.rewardsDBContext = rewardsDBContext;
        }

        public async Task<ApiResponseViewModel> Handle(DashboardQuery request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();
            try
            {
                var products = await rewardsDBContext.Products.ToListAsync();
                var merchants = await rewardsDBContext.Merchants.ToListAsync();
                var bankAccounts = await rewardsDBContext.BankAccounts.ToListAsync();
                var personInchanges = await rewardsDBContext.PersonInCharges.ToListAsync();
                DashboardModel newItem = new DashboardModel();
                newItem.MerchantDraft = merchants.Where(x => x.StatusTypeId == 1).Count()+ bankAccounts.Where(x => x.StatusTypeId == 1).Count() + personInchanges.Where(x => x.StatusTypeId == 1).Count();
                newItem.MerchantApproved = merchants.Where(x => x.StatusTypeId == 4).Count() + bankAccounts.Where(x => x.StatusTypeId == 4).Count() + personInchanges.Where(x => x.StatusTypeId == 4).Count();
                newItem.MerchantPendingReview = merchants.Where(x => x.StatusTypeId == 2).Count() + bankAccounts.Where(x => x.StatusTypeId == 2).Count() + personInchanges.Where(x => x.StatusTypeId == 2).Count();
                newItem.MerchantPendingRevision = merchants.Where(x => x.StatusTypeId == 3).Count() + bankAccounts.Where(x => x.StatusTypeId == 3).Count() + personInchanges.Where(x => x.StatusTypeId == 3).Count();
                newItem.MerchantPublished = merchants.Where(x => x.IsPublished == true).Count();
                newItem.MerchantTotal = merchants.Count();
                newItem.ProductDraft = products.Where(x => x.StatusTypeId == 1).Count();
                newItem.ProductApproved = products.Where(x => x.StatusTypeId == 4).Count();
                newItem.ProductPendingReview = products.Where(x => x.StatusTypeId == 3).Count();
                newItem.ProductPendingRevision = products.Where(x => x.StatusTypeId == 4).Count();
                newItem.ProductPublished = products.Where(x => x.IsPublished == true).Count();
                newItem.ProductTotal = products.Count();             
                response.Successful = true;
                response.Message = "Get Dashboard Successfully";
                response.Data = newItem;
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }

            return response;
        }
    }
}
