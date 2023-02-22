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
    public class CreateMaxOrderQuantityCommand : IRequest<ApiResponseViewModel>
    {
        public byte MaxQuantity { get; set; }
        public string Keyword { get; set; }

        public Guid UpdatedBy { get; set; }

        public class CreateMaxOrderQuantityCommandHandler : IRequestHandler<CreateMaxOrderQuantityCommand, ApiResponseViewModel>
        {
            private readonly RewardsDBContext rewardsDBContext;

            public CreateMaxOrderQuantityCommandHandler(RewardsDBContext rewardsDBContext)
            {
                this.rewardsDBContext = rewardsDBContext;
            }

            public async Task<ApiResponseViewModel> Handle(CreateMaxOrderQuantityCommand request, CancellationToken cancellationToken)
            {
                var apiResponseViewModel = new ApiResponseViewModel();
                try
                {
                    var aggregatorMaxQuantityFilters = new AggregatorMaxQuantityFilters
                    {
                        Keyword = request.Keyword,
                        MaxQuantity = request.MaxQuantity,
                        StatusId = 1,
                        CreatedAt = DateTime.Now
                    };

                    await rewardsDBContext.AggregatorMaxQuantityFilters.AddAsync(aggregatorMaxQuantityFilters);
                    await rewardsDBContext.SaveChangesAsync();

                    apiResponseViewModel.Successful = true;

                }
                catch (Exception ex)
                {
                    //  log
                    apiResponseViewModel.Message = "Fail to create max order quantity";
                    apiResponseViewModel.Successful = false;
                }

                return apiResponseViewModel;
            }
        }

    }
}
