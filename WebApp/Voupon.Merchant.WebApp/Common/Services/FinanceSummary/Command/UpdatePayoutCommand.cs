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


namespace Voupon.Merchant.WebApp.Common.Services.FinanceSummary.Command
{
    public class UpdatePayoutCommand : IRequest<ApiResponseViewModel>
    {
        public int MerchantFinanceId { get; set; }
        public string Remarks { get; set; }
        public DateTime PaidOutDate { get; set; }
    }
    public class UpdatePayoutCommandHandler : IRequestHandler<UpdatePayoutCommand, ApiResponseViewModel>
    {
        RewardsDBContext rewardsDBContext;
        public UpdatePayoutCommandHandler(RewardsDBContext rewardsDBContext)
        {
            this.rewardsDBContext = rewardsDBContext;
        }

        public async Task<ApiResponseViewModel> Handle(UpdatePayoutCommand request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();
            try
            {
                var merchantFinance = await rewardsDBContext.MerchantFinance.FirstOrDefaultAsync(x => x.Id == request.MerchantFinanceId);
                if (merchantFinance != null)
                {
                    merchantFinance.IsPaid = true;
                    merchantFinance.Remarks = request.Remarks;
                    merchantFinance.PayoutDate = DateOnly.FromDateTime(request.PaidOutDate);
                    var financeSummary = await rewardsDBContext.FinanceSummary.FirstOrDefaultAsync(x => x.Id == merchantFinance.FinanceSummaryId);
                    financeSummary.PayoutDate =DateOnly.FromDateTime(request.PaidOutDate);
                    await rewardsDBContext.SaveChangesAsync();
                    response.Successful = true;
                    response.Data = merchantFinance;
                    response.Message = "Update Merchant Finance Successfully";
                }
                else
                {
                    response.Successful = false;
                    response.Message = "Merchant Finance not found";
                }
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
