using MediatR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Merchant.WebApp.ViewModels;

namespace Voupon.Merchant.WebApp.Areas.Admin.Services.Config.Commands
{
    public class UpdateMerchantExcludeFilterCommand : IRequest<ApiResponseViewModel>
    {
        public int Id { get; set; }
        public string MerchantId { get; set; }
        public string MerchantUsername { get; set; }

        public class UpdateMerchantExcludeFilterCommandHandler : IRequestHandler<UpdateMerchantExcludeFilterCommand, ApiResponseViewModel>
        {
            private readonly RewardsDBContext rewardsDBContext;

            public UpdateMerchantExcludeFilterCommandHandler(RewardsDBContext rewardsDBContext)
            {
                this.rewardsDBContext = rewardsDBContext;
            }

            public async Task<ApiResponseViewModel> Handle(UpdateMerchantExcludeFilterCommand request, CancellationToken cancellationToken)
            {
                var apiResponseViewModel = new ApiResponseViewModel();
                try
                {
                    // var result = await UserManager.UpdateSecurityStampAsync(user.Id);

                    var aggregatorExcludeMerchants = await rewardsDBContext.AggregatorExcludeMerchants.Where(x => x.Id == request.Id).FirstOrDefaultAsync();

                    if (aggregatorExcludeMerchants == null)
                    {
                        apiResponseViewModel.Message = "Fail to update merchant filter [0001]";
                        apiResponseViewModel.Successful = false;
                        return apiResponseViewModel;
                    }

                    var shopApiCall = await new HttpClient().GetAsync($"https://shopee.com.my/api/v4/shop/get_shop_detail?sort_sold_out=0&username={request.MerchantUsername}");
                    if (shopApiCall.StatusCode != System.Net.HttpStatusCode.OK)
                    {
                        apiResponseViewModel.Message = "Fail to create merchant filter. Invalid username";
                        apiResponseViewModel.Successful = false;
                        return apiResponseViewModel;
                    }

                    var shopResult = await shopApiCall.Content.ReadAsStringAsync();
                    var shop = JsonConvert.DeserializeObject<dynamic>(shopResult);

                    aggregatorExcludeMerchants.MerchantId = shop.data.shopid;
                    aggregatorExcludeMerchants.MerchantUsername = request.MerchantUsername;

                    rewardsDBContext.AggregatorExcludeMerchants.Update(aggregatorExcludeMerchants);
                    await rewardsDBContext.SaveChangesAsync();

                    apiResponseViewModel.Message = "Successfully updated merchant filter";
                    apiResponseViewModel.Successful = true;

                }
                catch (Exception ex)
                {
                    apiResponseViewModel.Message = "Fail to update merchant filter [0003]";
                    apiResponseViewModel.Successful = false;
                }

                return apiResponseViewModel;
            }
        }

    }
}
