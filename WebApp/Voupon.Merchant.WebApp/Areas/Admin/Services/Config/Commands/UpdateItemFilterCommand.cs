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
    public class UpdateItemFilterCommand : IRequest<ApiResponseViewModel>
    {
        public int Id { get; set; }
        public string ProductUrl { get; set; }

        public class UpdateItemFilterCommandHandler : IRequestHandler<UpdateItemFilterCommand, ApiResponseViewModel>
        {
            private readonly RewardsDBContext rewardsDBContext;

            public UpdateItemFilterCommandHandler(RewardsDBContext rewardsDBContext)
            {
                this.rewardsDBContext = rewardsDBContext;
            }

            public async Task<ApiResponseViewModel> Handle(UpdateItemFilterCommand request, CancellationToken cancellationToken)
            {
                var apiResponseViewModel = new ApiResponseViewModel();
                try
                {
                    // var result = await UserManager.UpdateSecurityStampAsync(user.Id);

                    var aggregatorExcludeProducts = await rewardsDBContext.AggregatorExcludeProducts.Where(x => x.Id == request.Id).FirstOrDefaultAsync();

                    if (aggregatorExcludeProducts == null)
                    {
                        apiResponseViewModel.Message = "Fail to update item filter [0001]";
                        apiResponseViewModel.Successful = false;
                        return apiResponseViewModel;
                    }

                    aggregatorExcludeProducts.ProductUrl = request.ProductUrl;

                    var productUrlArray = request.ProductUrl.Split(".");
                    if (productUrlArray.Length < 3)
                    {
                        apiResponseViewModel.Message = "Fail to update item filter. Invalid URL";
                        apiResponseViewModel.Successful = false;
                        return apiResponseViewModel;
                    }

                    aggregatorExcludeProducts.ProductId = productUrlArray[productUrlArray.Length - 1];
                    aggregatorExcludeProducts.MerchantId = productUrlArray[productUrlArray.Length - 2];

                    rewardsDBContext.AggregatorExcludeProducts.Update(aggregatorExcludeProducts);
                    await rewardsDBContext.SaveChangesAsync();

                    apiResponseViewModel.Message = "Successfully updated item filter";
                    apiResponseViewModel.Successful = true;

                }
                catch (Exception ex)
                {
                    apiResponseViewModel.Message = "Fail to update item filter [0003]";
                    apiResponseViewModel.Successful = false;
                }

                return apiResponseViewModel;
            }
        }

    }
}
