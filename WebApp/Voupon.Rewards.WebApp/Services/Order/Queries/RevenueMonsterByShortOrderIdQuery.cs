using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Rewards.WebApp.ViewModels;

namespace Voupon.Rewards.WebApp.Services.Order.Queries
{
    public class RevenueMonsterByShortOrderIdQuery : IRequest<ApiResponseViewModel>
    {
        public string ShortOrderId { get; set; }
    }
    public class RevenueMonsterByShortOrderIdQueryHandler : IRequestHandler<RevenueMonsterByShortOrderIdQuery, ApiResponseViewModel>
    {
        private readonly RewardsDBContext rewardsDBContext;


        public RevenueMonsterByShortOrderIdQueryHandler(RewardsDBContext rewardsDBContext, IOptions<AppSettings> appSettings)
        {
            this.rewardsDBContext = rewardsDBContext;

        }
        public async Task<ApiResponseViewModel> Handle(RevenueMonsterByShortOrderIdQuery request, CancellationToken cancellationToken)
        {
            var apiResponseViewModel = new ApiResponseViewModel();
            try
            {
                var order = rewardsDBContext.Orders.Where(x => x.ShortId == request.ShortOrderId).FirstOrDefaultAsync();
                if(order == null)
                {
                    apiResponseViewModel.Successful = false;
                    return apiResponseViewModel;
                }

               


                return apiResponseViewModel;
            }
            catch (Exception ex)
            {
                apiResponseViewModel.Successful = false;
                apiResponseViewModel.Message = "Fail to Get order detail from RM";
                return apiResponseViewModel;

            }
        }
    }
}
