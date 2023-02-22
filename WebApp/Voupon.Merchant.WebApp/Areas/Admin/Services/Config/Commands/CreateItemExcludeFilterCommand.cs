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
    public class CreateItemExcludeFilterCommand : IRequest<ApiResponseViewModel>
    {
        public byte ExternalTypeId { get; set; }
        public string MerchantId { get; set; }
        public string ProductUrl { get; set; }

        public Guid UpdatedBy { get; set; }

        public class CreateItemExcludeFilterCommandHandler : IRequestHandler<CreateItemExcludeFilterCommand, ApiResponseViewModel>
        {
            private readonly RewardsDBContext rewardsDBContext;

            public CreateItemExcludeFilterCommandHandler(RewardsDBContext rewardsDBContext)
            {
                this.rewardsDBContext = rewardsDBContext;
            }

            public async Task<ApiResponseViewModel> Handle(CreateItemExcludeFilterCommand request, CancellationToken cancellationToken)
            {
                var apiResponseViewModel = new ApiResponseViewModel();
                try
                {
                    var aggregatorExcludeProducts = new AggregatorExcludeProducts
                    {
                        ProductUrl = request.ProductUrl,
                        ExternalTypeId = 1,
                        StatusId = 1,
                        CreatedAt = DateTime.Now
                    };

                    var productUrlArray = request.ProductUrl.Split(".");
                    if(productUrlArray.Length < 3)
                    {
                        apiResponseViewModel.Message = "Fail to create item filter. Invalid URL";
                        apiResponseViewModel.Successful = false;
                        return apiResponseViewModel;
                    }

                    aggregatorExcludeProducts.ProductId = productUrlArray[productUrlArray.Length -1];
                    aggregatorExcludeProducts.MerchantId = productUrlArray[productUrlArray.Length - 2];

                    await rewardsDBContext.AggregatorExcludeProducts.AddAsync(aggregatorExcludeProducts);
                    await rewardsDBContext.SaveChangesAsync();

                    apiResponseViewModel.Successful = true;
                    apiResponseViewModel.Message = "Successfully created item filter";

                }
                catch (Exception ex)
                {
                    //  log
                    apiResponseViewModel.Message = "Fail to create item filter";
                    apiResponseViewModel.Successful = false;
                }

                return apiResponseViewModel;
            }
        }

    }
}
