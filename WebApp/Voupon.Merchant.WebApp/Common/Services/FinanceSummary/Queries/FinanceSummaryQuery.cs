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


namespace Voupon.Merchant.WebApp.Common.Services.FinanceSummary.Queries
{
    public class FinanceSummaryQuery : IRequest<ApiResponseViewModel>
    {
        public int FinanceSummaryId { get; set; }
    }
    public class FinanceSummaryQueryHandler : IRequestHandler<FinanceSummaryQuery, ApiResponseViewModel>
    {
        RewardsDBContext rewardsDBContext;
        public FinanceSummaryQueryHandler(RewardsDBContext rewardsDBContext)
        {
            this.rewardsDBContext = rewardsDBContext;
        }

        public async Task<ApiResponseViewModel> Handle(FinanceSummaryQuery request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();
            try
            {             
                Voupon.Database.Postgres.RewardsEntities.FinanceSummary financeSummary = await rewardsDBContext.FinanceSummary.Include(x=>x.MerchantFinance).FirstOrDefaultAsync(x=>x.Id== request.FinanceSummaryId);
                response.Successful = true;
                response.Message = "Get Finance Summary Successfully";
                response.Data = financeSummary;
            }
            catch (Exception ex)
            {
                response.Successful = false;
                response.Message = ex.Message;
            }

            return response;
        }
    }
}
