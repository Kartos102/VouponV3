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
    public class GetPayoutQuery : IRequest<ApiResponseViewModel>
    {
        public int MerchantFinanceId { get; set; }
    }
    public class GetPayoutQueryHandler : IRequestHandler<GetPayoutQuery, ApiResponseViewModel>
    {
        RewardsDBContext rewardsDBContext;
        public GetPayoutQueryHandler(RewardsDBContext rewardsDBContext)
        {
            this.rewardsDBContext = rewardsDBContext;
        }

        public async Task<ApiResponseViewModel> Handle(GetPayoutQuery request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();
            try
            {             
                Voupon.Database.Postgres.RewardsEntities.MerchantFinance merchantFinance = await rewardsDBContext.MerchantFinance.Include(x=>x.Merchant).Include(x=>x.FinanceSummary).Include(x=>x.FinanceTransaction).ThenInclude(x=>x.OrderItem).ThenInclude(x => x.Order).FirstOrDefaultAsync(x=>x.Id== request.MerchantFinanceId);
                response.Successful = true;
                response.Message = "Get Merchant Finance Successfully";
                response.Data = merchantFinance;
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
