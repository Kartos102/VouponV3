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
    public class CreateMerchantExcludeFilterCommand : IRequest<ApiResponseViewModel>
    {
        public byte ExternalTypeId { get; set; }
        public string MerchantId { get; set; }
        public string MerchantUsername { get; set; }

        public Guid UpdatedBy { get; set; }

        public class CreateMerchantExcludeFilterCommandHandler : IRequestHandler<CreateMerchantExcludeFilterCommand, ApiResponseViewModel>
        {
            private readonly RewardsDBContext rewardsDBContext;

            public CreateMerchantExcludeFilterCommandHandler(RewardsDBContext rewardsDBContext)
            {
                this.rewardsDBContext = rewardsDBContext;
            }

            public async Task<ApiResponseViewModel> Handle(CreateMerchantExcludeFilterCommand request, CancellationToken cancellationToken)
            {
                var apiResponseViewModel = new ApiResponseViewModel();
                try
                {
                    var shopApiCall = await new HttpClient().GetAsync($"https://shopee.com.my/api/v4/shop/get_shop_detail?sort_sold_out=0&username={request.MerchantUsername}");
                    if(shopApiCall.StatusCode != System.Net.HttpStatusCode.OK)
                    {
                        apiResponseViewModel.Message = "Fail to create merchant filter. Invalid username";
                        apiResponseViewModel.Successful = false;
                        return apiResponseViewModel;
                    }

                    var shopResult = await shopApiCall.Content.ReadAsStringAsync();
                    var shop = JsonConvert.DeserializeObject<dynamic>(shopResult);

                    var aggregatorExcludeMerchants = new AggregatorExcludeMerchants
                    {
                        MerchantId = shop.data.shopid,
                        MerchantUsername = request.MerchantUsername,
                        ExternalTypeId = 1,
                        StatusId = 1,
                        CreatedAt = DateTime.Now
                    };

                    await rewardsDBContext.AggregatorExcludeMerchants.AddAsync(aggregatorExcludeMerchants);
                    await rewardsDBContext.SaveChangesAsync();

                    apiResponseViewModel.Successful = true;

                }
                catch (Exception ex)
                {
                    //  log
                    apiResponseViewModel.Message = "Fail to create merchant filter";
                    apiResponseViewModel.Successful = false;
                }

                return apiResponseViewModel;
            }
        }

    }
}
