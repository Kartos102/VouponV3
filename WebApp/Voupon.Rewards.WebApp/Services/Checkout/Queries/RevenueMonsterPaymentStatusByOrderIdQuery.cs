using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Rewards.WebApp.Infrastructures.Helpers;
using Voupon.Rewards.WebApp.ViewModels;
using Voupon.Rewards.WebApp.ViewModels.ThirdParty.RevenueMonster.Callback;

namespace Voupon.Rewards.WebApp.Services.Checkout.Queries
{
    public class RevenueMonsterPaymentStatusByOrderIdQuery : IRequest<ApiResponseViewModel>
    {
        public string ShortId { get; set; }
     
        public class RevenueMonsterPaymentStatusByOrderIdQueryHandler : IRequestHandler<RevenueMonsterPaymentStatusByOrderIdQuery, ApiResponseViewModel>
        {
            RewardsDBContext rewardsDBContext;
            private readonly IOptions<AppSettings> appSettings;
            public RevenueMonsterPaymentStatusByOrderIdQueryHandler(RewardsDBContext rewardsDBContext, IOptions<AppSettings> appSettings)
            {
                this.rewardsDBContext = rewardsDBContext;
                this.appSettings = appSettings;
            }


            public async Task<ApiResponseViewModel> Handle(RevenueMonsterPaymentStatusByOrderIdQuery request, CancellationToken cancellationToken)
            {
                var apiResponseViewModel = new ApiResponseViewModel();
                try
                {
                    var appConfig = await rewardsDBContext.AppConfig.FirstOrDefaultAsync();

                    var order = await rewardsDBContext.Orders.Where(x => x.ShortId == request.ShortId).FirstOrDefaultAsync();

                    if(order == null)
                    {

                    }

                    var grantType = new RevenueMonsterGrantType
                    {
                        GrantType = "client_credentials"
                    };

                    var httpClient = new HttpClient();
                    httpClient.DefaultRequestHeaders.Add("Authorization", $"Basic {Convert.ToBase64String(Encoding.UTF8.GetBytes($"{appSettings.Value.PaymentGateways.RevenueMonster.ClientId}:{appSettings.Value.PaymentGateways.RevenueMonster.ClientSecret}"))}");
                    var tokenData = new StringContent(JsonConvert.SerializeObject(grantType), Encoding.UTF8, "application/json");
                    var tokenResponse = await httpClient.PostAsync($"{appSettings.Value.PaymentGateways.RevenueMonster.AuthUrl}/v1/token", tokenData);
                    string tokenResult = tokenResponse.Content.ReadAsStringAsync().Result;

                    dynamic dynamicTokenResult = JsonConvert.DeserializeObject<ExpandoObject>(tokenResult);
                    var accessToken = dynamicTokenResult.accessToken;

                    var privateKey = appSettings.Value.PaymentGateways.RevenueMonster.PrivateKey;
                    string method = "get";
                    string nonceStr = RandomString.GenerateRandomString(32);
                    var requestURL = $"{appSettings.Value.PaymentGateways.RevenueMonster.ApiUrl}/v3/payment/transaction/order/{order.ShortId}";
                    string signType = "sha256";
                    string timestamp = Convert.ToString((Int32)(DateTime.Now.Subtract(new DateTime(1970, 1, 1))).TotalSeconds);
                    RevenueMonsterSignature signature = new RevenueMonsterSignature();
                    string signatureResult = "";
                    signatureResult = signature.GenerateSignature("", method, nonceStr, privateKey, requestURL, signType, timestamp);
                    signatureResult = "sha256 " + signatureResult;


                    HttpClient client = new HttpClient();
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                    client.DefaultRequestHeaders.Add("X-Nonce-Str", nonceStr);
                    client.DefaultRequestHeaders.Add("X-Signature", signatureResult);
                    client.DefaultRequestHeaders.Add("X-Timestamp", timestamp);

                    var response = await client.GetAsync(requestURL);
                    var jsonAsString = await response.Content.ReadAsStringAsync();
                    dynamic orderResult = JsonConvert.DeserializeObject<ExpandoObject>(jsonAsString);
                    var viewModel = new RevenueMonsterCallback();

                    if (((IDictionary<string, object>)orderResult).ContainsKey("error"))
                    {
                        apiResponseViewModel.Successful = false;
                        return apiResponseViewModel;
                    }

                    var status = orderResult.code;

                    if (string.IsNullOrEmpty(status))
                    {
                        apiResponseViewModel.Successful = false;
                    }

                    if (status == "SUCCESS")
                    {
                        apiResponseViewModel.Successful = true;
                        apiResponseViewModel.Data = orderResult;
                    }
                    else
                    {
                        apiResponseViewModel.Successful = false;
                        apiResponseViewModel.Data = viewModel;
                    }

                    return apiResponseViewModel;
                }
                catch (Exception ex)
                {
                    apiResponseViewModel.Message = ex.Message;
                    apiResponseViewModel.Data = false;
                }

                return apiResponseViewModel;
            }

            private class RevenueMonsterGrantType
            {
                public string GrantType { get; set; }
            }
        }
    }

}
