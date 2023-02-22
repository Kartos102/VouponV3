using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Common.BaseTypes;
using Voupon.Database.Postgres.RewardsEntities;
using Microsoft.EntityFrameworkCore;
using Voupon.Merchant.WebApp.ViewModels;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;
using Voupon.Database.Postgres.VodusEntities;

namespace Voupon.Merchant.WebApp.Areas.App.Services.GifteeVouchers.Queries.Single
{
    public class GifteeProductQuery : IRequest<ApiResponseViewModel>
    {
        public int Id { get; set; }

        public class GifteeProductQueryHandler : IRequestHandler<GifteeProductQuery, ApiResponseViewModel>
        {
            private VodusV2Context vodusV2Context;
            RewardsDBContext rewardsDBContext;
            private readonly IOptions<AppSettings> appSettings;
            public GifteeProductQueryHandler(RewardsDBContext rewardsDBContext, VodusV2Context vodusV2Context, IOptions<AppSettings> appSettings)
            {
                this.rewardsDBContext = rewardsDBContext;
                this.vodusV2Context = vodusV2Context;
                this.appSettings = appSettings;
            }

            public async Task<ApiResponseViewModel> Handle(GifteeProductQuery request, CancellationToken cancellationToken)
            {
                ApiResponseViewModel response = new ApiResponseViewModel();
                try
                {
                    var httpClient = new HttpClient();
                    var gifteeRequest = new HttpRequestMessage(HttpMethod.Get, $"{appSettings.Value.ThirdPartyRedemptions.Giftee.Url}/reference/egiftitem?egift_item_id={request.Id}");

                    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    gifteeRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", appSettings.Value.ThirdPartyRedemptions.Giftee.Token);
                    gifteeRequest.Headers.Add("X-Giftee", "1");


                    var result = await httpClient.SendAsync(gifteeRequest);
                    var resultString = await result.Content.ReadAsStringAsync();

                    dynamic testResult = JsonConvert.DeserializeObject(resultString);

                    response.Data = new GifteeItemResponseModel
                    {
                        ItemImageUrl = testResult.items.data[0].item_image_url,
                    };
                    response.Successful = true;
                }
                catch (Exception ex)
                {
                    response.Message = ex.Message;
                    response.Successful = false;
                    return response;
                }

                return response;
            }

        }
        public class GifteeItemResponseModel
        {

            public string ItemImageUrl { get; set; }

            public string RawResponse { get; set; }
            public string Error { get; set; }
        }
        public class GifteeRequestModel
        {
            //public string campaign { get; set; }
            // public string distributor { get; set; }

            public int egift_item_id { get; set; }
            public bool giftcard_option { get; set; }
            public string request_code { get; set; }
        }

    }

}
