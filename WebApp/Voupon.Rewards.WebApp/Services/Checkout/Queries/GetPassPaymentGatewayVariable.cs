using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Rewards.WebApp.ViewModels;

namespace Voupon.Rewards.WebApp.Services.Checkout.Queries
{
    public class GetPassPaymentGatewayVariable : IRequest<ApiResponseViewModel>
    {
        public class GetPassPaymentGatewayVariableHandler : IRequestHandler<GetPassPaymentGatewayVariable, ApiResponseViewModel>
        {

            RewardsDBContext rewardsDBContext;

            public GetPassPaymentGatewayVariableHandler(RewardsDBContext rewardsDBContext)
            {
                this.rewardsDBContext = rewardsDBContext;

            }


            public async Task<ApiResponseViewModel> Handle(GetPassPaymentGatewayVariable request, CancellationToken cancellationToken)
            {
                var apiResponseViewModel = new ApiResponseViewModel();
                try
                {
                    var appConfig = await rewardsDBContext.AppConfig.FirstOrDefaultAsync();

                    if (appConfig == null)
                    {
                        apiResponseViewModel.Successful = false;
                        apiResponseViewModel.Data = false;
                        return apiResponseViewModel;
                    }

                    apiResponseViewModel.Successful = true;
                    apiResponseViewModel.Data = appConfig.IsPassPaymentGatewayEnabled;

                    return apiResponseViewModel;
                }
                catch (Exception ex)
                {
                    apiResponseViewModel.Message = ex.Message;
                    apiResponseViewModel.Data = false;
                }

                return apiResponseViewModel;
            }
        }
    }

}
