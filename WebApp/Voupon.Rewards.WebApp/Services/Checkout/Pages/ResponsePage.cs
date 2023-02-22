using MediatR;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Rewards.WebApp.ViewModels;
using Voupon.Rewards.WebApp.ViewModels.ThirdParty.IPay88;
using static Voupon.Rewards.WebApp.Infrastructures.Helpers.Crypto;

namespace Voupon.Rewards.WebApp.Services.Checkout.Pages
{
    public class ResponsePage : IRequest<ApiResponseViewModel>
    {
        public IPay88ResponseViewModel IPay88ResponseViewModel { get; set; }
        private class ResponsePageHandler : IRequestHandler<ResponsePage, ApiResponseViewModel>
        {
            RewardsDBContext rewardsDBContext;
            private readonly IOptions<AppSettings> appSettings;
            public ResponsePageHandler(RewardsDBContext rewardsDBContext, IOptions<AppSettings> appSettings)
            {
                this.rewardsDBContext = rewardsDBContext;
                this.appSettings = appSettings;
            }

            public async Task<ApiResponseViewModel> Handle(ResponsePage request, CancellationToken cancellationToken)
            {
                var apiResponseViewModel = new ApiResponseViewModel();

                if (request.IPay88ResponseViewModel.eStatus.Trim() == "0")
                {
                    apiResponseViewModel.Message = request.IPay88ResponseViewModel.ErrDesc;
                    apiResponseViewModel.Successful = false;
                    return apiResponseViewModel;
                }

                var isSignatureValid = IsResponseSignatureValid(appSettings.Value.PaymentGateways.Ipay88.MerchantKey, request.IPay88ResponseViewModel.Signature, 
                    request.IPay88ResponseViewModel.MerchantCode, 
                    request.IPay88ResponseViewModel.PaymentId,
                    request.IPay88ResponseViewModel.RefNo, 
                    request.IPay88ResponseViewModel.Amount, 
                    request.IPay88ResponseViewModel.eCurrency, 
                    request.IPay88ResponseViewModel.eStatus);

                if (!isSignatureValid)
                {
                    apiResponseViewModel.Successful = false;
                    return apiResponseViewModel;
                }

                //  Create transaction record

                //  Generate Receipt

                //  Generate Token

                apiResponseViewModel.Successful = true;
                return apiResponseViewModel;
            }


            private bool IsResponseSignatureValid(string merchantKey, string signature, string merchantCode, int paymentId, string refNo, string amount, string currency, string status)
            {
                amount = "1";
                string inputValue = merchantKey
                                  + merchantCode
                                  + paymentId
                                  + refNo
                                  + decimal.Parse(amount).ToString("F").Replace(".", "").Replace(",", "")
                                  + "MYR"
                                  + status;

                if (signature == SHA_256.GenerateSHA256String(inputValue))
                {
                    return true;
                }
                return false;
            }
        }

        public class OrderViewModel
        {
            public Guid Id { get; set; }
            public int MasterMemberProfileId { get; set; }
            public string Email { get; set; }
            public decimal TotalPrice { get; set; }
            public int TotalPoints { get; set; }
            public int TotalItems { get; set; }
            public byte OrderStatus { get; set; }
            public DateTime CreatedAt { get; set; }
            public string BillingPersonFirstName { get; set; }
            public string BillingPersonLastName { get; set; }
            public string BillingEmail { get; set; }
            public string BillingMobileNumber { get; set; }
            public string BillingMobileCountryCode { get; set; }
            public string BillingAddressLine1 { get; set; }
            public string BillingAddressLine2 { get; set; }
            public string BillingPostcode { get; set; }
            public string BillingCity { get; set; }
            public string BillingState { get; set; }
            public string BillingCountry { get; set; }
            public string ShippingPersonFirstName { get; set; }
            public string ShippingPersonLastName { get; set; }
            public string ShippingEmail { get; set; }
            public string ShippingMobileNumber { get; set; }
            public string ShippingMobileCountryCode { get; set; }
            public string ShippingAddressLine1 { get; set; }
            public string ShippingAddressLine2 { get; set; }
            public string ShippingPostcode { get; set; }
            public string ShippingCity { get; set; }
            public string ShippingState { get; set; }
            public string ShippingCountry { get; set; }

            public IPay88RequestViewModel Ipay88RequestViewModel { get; set; }
        }

    }

}
