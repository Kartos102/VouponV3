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
    public class UpdateStatementUrlCommand : IRequest<ApiResponseViewModel>
    {
        public int MerchantFinanceId { get; set; }
        public string Url { get; set; }
     
    }
    public class UpdateStatementUrlCommandHandler : IRequestHandler<UpdateStatementUrlCommand, ApiResponseViewModel>
    {
        RewardsDBContext rewardsDBContext;
        public UpdateStatementUrlCommandHandler(RewardsDBContext rewardsDBContext)
        {
            this.rewardsDBContext = rewardsDBContext;
        }

        public async Task<ApiResponseViewModel> Handle(UpdateStatementUrlCommand request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();
            try
            {
                var merchantFinance = await rewardsDBContext.MerchantFinance.FirstOrDefaultAsync(x => x.Id == request.MerchantFinanceId);
                if (merchantFinance != null)
                {
                    merchantFinance.StatementOfAccountUrl = request.Url;                  
                    await rewardsDBContext.SaveChangesAsync();
                    response.Successful = true;
                    response.Data = merchantFinance;
                    response.Message = "Update Finance Statement Url Successfully";
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
