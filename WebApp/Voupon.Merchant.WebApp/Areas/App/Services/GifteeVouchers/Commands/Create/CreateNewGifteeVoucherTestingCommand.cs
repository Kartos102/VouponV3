using MediatR;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Merchant.WebApp.ViewModels;


namespace Voupon.Merchant.WebApp.Areas.App.Services.GifteeVouchers.Commands.Create
{
    public class CreateNewGifteeVoucherTestingCommand : IRequest<ApiResponseViewModel>
    {
        public Guid Id { get; set; }
    }

    public class CreateNewGifteeVoucherTestingCommandHandler : IRequestHandler<CreateNewGifteeVoucherTestingCommand, ApiResponseViewModel>
    {
        RewardsDBContext rewardsDBContext;
        public CreateNewGifteeVoucherTestingCommandHandler(RewardsDBContext rewardsDBContext)
        {
            this.rewardsDBContext = rewardsDBContext;
        }

        public async Task<ApiResponseViewModel> Handle(CreateNewGifteeVoucherTestingCommand request, CancellationToken cancellationToken)
        {
            var response = new ApiResponseViewModel();
            try
            {
                var requestBody = new GifteeRequestModel
                {
                    giftcard_option = true,
                    egift_item_id = 5879,
                    request_code = DateTime.Now.Ticks.ToString()
                };
                var httpClient = new HttpClient();
                var req = new HttpRequestMessage(HttpMethod.Post, "https://staging-channel-api.giftee-biz.asia/generate/egift");



                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                req.Content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json"); ;
                req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "ITycI99KJUonHKJ5PcMMNQOnIqYuiEwVJBA8YMBF");
                req.Headers.Add("X-Giftee", "1");


                var result = await httpClient.SendAsync(req);
                var resultString = await result.Content.ReadAsStringAsync();

               // dynamic testResult = JsonConvert.DeserializeObject(resultString);
                response.Data = result;
                response.Successful = true;
            }
            catch (Exception ex)
            {
                response.Data = ex.Message;
                return response;
            }

            return response;

        }

        public class GifteeResponseModel
        {
            public bool IsSuccessful { get; set; }
            public string Url { get; set; }
            public string VoucherName { get; set; }

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
