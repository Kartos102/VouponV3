using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Merchant.WebApp.ViewModels;

namespace Voupon.Merchant.WebApp.Areas.Admin.Services.Config.Commands
{
    public class UpdateMaxOrderCommand : IRequest<ApiResponseViewModel>
    {
        public int Id { get; set; }
        public byte MaxQuantity { get; set; }
        public string Keyword { get; set; }

        public class UpdateMaxOrderCommandHandler : IRequestHandler<UpdateMaxOrderCommand, ApiResponseViewModel>
        {
            private readonly RewardsDBContext rewardsDBContext;

            public UpdateMaxOrderCommandHandler(RewardsDBContext rewardsDBContext)
            {
                this.rewardsDBContext = rewardsDBContext;
            }

            public async Task<ApiResponseViewModel> Handle(UpdateMaxOrderCommand request, CancellationToken cancellationToken)
            {
                var apiResponseViewModel = new ApiResponseViewModel();
                try
                {
                    // var result = await UserManager.UpdateSecurityStampAsync(user.Id);

                    var aggregatorMaxQuantityFilters = await rewardsDBContext.AggregatorMaxQuantityFilters.Where(x => x.Id == request.Id).FirstOrDefaultAsync();

                    if (aggregatorMaxQuantityFilters == null)
                    {
                        apiResponseViewModel.Message = "Fail to update max order quantity [0001]";
                        apiResponseViewModel.Successful = false;
                        return apiResponseViewModel;
                    }

                    aggregatorMaxQuantityFilters.MaxQuantity = request.MaxQuantity;
                    aggregatorMaxQuantityFilters.Keyword = request.Keyword;

                    rewardsDBContext.AggregatorMaxQuantityFilters.Update(aggregatorMaxQuantityFilters);
                    await rewardsDBContext.SaveChangesAsync();

                    apiResponseViewModel.Message = "Successfully updated max order quantity";
                    apiResponseViewModel.Successful = true;

                }
                catch (Exception ex)
                {
                    apiResponseViewModel.Message = "Fail to update max order quantity [0003]";
                    apiResponseViewModel.Successful = false;
                }

                return apiResponseViewModel;
            }
        }

    }
}
