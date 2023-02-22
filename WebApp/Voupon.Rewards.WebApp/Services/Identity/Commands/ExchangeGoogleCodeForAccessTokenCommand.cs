using MediatR;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Rewards.WebApp.ViewModels;

public class ExchangeGoogleCodeForAccessTokenCommand : IRequest<ApiResponseViewModel>
{
    public string token { get; set; }
    public string code { get; set; }
    public string clientId { get; set; }
    public string client_Secret { get; set; }
    public string redirect_uri { get; set; }
    public string grant_type { get; set; }



    private class ExchangeGoogleCodeForAccessTokenCommandHandler : IRequestHandler<ExchangeGoogleCodeForAccessTokenCommand, ApiResponseViewModel>
    {
        private readonly RewardsDBContext rewardsDBContext;
        public ExchangeGoogleCodeForAccessTokenCommandHandler(RewardsDBContext rewardsDBContext)
        {
            this.rewardsDBContext = rewardsDBContext;
        }

        public async Task<ApiResponseViewModel> Handle(ExchangeGoogleCodeForAccessTokenCommand request, CancellationToken cancellationToken)
        {
            var apiResponseViewModel = new ApiResponseViewModel();

            var httpClient = new HttpClient();

            request.clientId = "700069321358-1t3lj52il4lrfcbffo31qbj78b46dind.apps.googleusercontent.com";
            request.client_Secret = "Eci0GWSU7Uqu89UguCQqY1hu";
            request.redirect_uri = Uri.EscapeDataString("https://localhost:44301/identity/google");
            request.grant_type = "authorization_code";
            string json = JsonConvert.SerializeObject(request);

            StringContent httpContent = new StringContent(json, System.Text.Encoding.UTF8, "application/x-www-form-urlencoded");

            var response = await httpClient.PostAsync("https://www.googleapis.com/oauth2/v4/token", httpContent);
            var a = await response.Content.ReadAsStringAsync();
            apiResponseViewModel.Successful = true;
            return apiResponseViewModel;

        }
    }

}