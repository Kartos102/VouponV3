using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Database.Postgres.VodusEntities;
using Voupon.Merchant.WebApp.ViewModels;

namespace Voupon.Merchant.WebApp.Areas.App.Services.GifteeVouchers.Commands
{
    public class CreateGifteedigitalRedemptionTokensCommand : IRequest<ApiResponseViewModel>
    {
        public int Id { get; set; }
        public int ProductId { get; set; }

        public class CreateGifteedigitalRedemptionTokensCommandHandler : IRequestHandler<CreateGifteedigitalRedemptionTokensCommand, ApiResponseViewModel>
        {
            private VodusV2Context vodusV2Context;
            RewardsDBContext rewardsDBContext;
            private readonly IOptions<AppSettings> appSettings;
            public CreateGifteedigitalRedemptionTokensCommandHandler(RewardsDBContext rewardsDBContext, VodusV2Context vodusV2Context, IOptions<AppSettings> appSettings)
            {
                this.rewardsDBContext = rewardsDBContext;
                this.vodusV2Context = vodusV2Context;
                this.appSettings = appSettings;
            }

            public async Task<ApiResponseViewModel> Handle(CreateGifteedigitalRedemptionTokensCommand request, CancellationToken cancellationToken)
            {
                ApiResponseViewModel response = new ApiResponseViewModel();
                try
                {
                    var product = rewardsDBContext.Products.Where(x => x.Id == request.ProductId).FirstOrDefault();
                    var digitalRedemptionToken = rewardsDBContext.DigitalRedemptionTokens.Where(x => x.Id == request.Id).FirstOrDefault();
                    if (digitalRedemptionToken == null)
                    {
                        response.Successful = false;
                        response.Message = "Fail to Get Digital Redemption Tokens";
                        response.Data = null;
                        return response;
                    }
                    var masterMemberProfile = vodusV2Context.MasterMemberProfiles.Include(x => x.User).Where(x => x.Id == digitalRedemptionToken.MasterMemberProfileId).FirstOrDefault();
                    if (product == null || masterMemberProfile == null || masterMemberProfile.User == null)
                    {
                        response.Successful = false;
                        response.Message = "Fail to Get Product Or master Member Profile for this Redemption";
                        response.Data = null;
                        return response;
                    }


                    var fullName = masterMemberProfile.User.FirstName + " " + masterMemberProfile.User.LastName;
                    var email = masterMemberProfile.User.Email;
                    var result = await ThirdPartyRedemption(product, digitalRedemptionToken.Id);


                    if (string.IsNullOrEmpty(result.Error))
                    {
                        if (result.IsSuccessful)

                        {
                            var date = DateTime.Now;
                            GifteeTokens gifteeToken = new GifteeTokens()
                            {
                                VoucherName = result.VoucherName,
                                Token = result.Url,
                                IssuedDate = date,
                                DigitalRedemptionId = digitalRedemptionToken.Id
                            };
                            rewardsDBContext.GifteeTokens.Add(gifteeToken);
                            rewardsDBContext.SaveChanges();

                            digitalRedemptionToken.Token = result.Url;
                            digitalRedemptionToken.RedeemedAt = date;
                            digitalRedemptionToken.IsRedeemed = true;
                            digitalRedemptionToken.TokenType = 2;
                            digitalRedemptionToken.UpdateTokenAt = DateTime.Now;
                            rewardsDBContext.SaveChanges();

                            var sendGridClient2 = new SendGridClient(appSettings.Value.Mailer.Sendgrid.APIKey);
                            var msg2 = new SendGridMessage();
                            msg2.SetTemplateId(appSettings.Value.Mailer.Sendgrid.Templates.ThirdPartyRedemption);
                            msg2.SetFrom(new EmailAddress(appSettings.Value.Emails.Noreply, "Vodus No-Reply"));
                            msg2.SetSubject("Vodus Redemption Url");
                            msg2.AddTo(new EmailAddress(email));
                            msg2.AddTo(new EmailAddress("merchant@vodus.my"));
                            msg2.AddSubstitution("-customerName-", fullName);
                            msg2.AddSubstitution("-item-", product.Title);
                            msg2.AddSubstitution("-redemptionUrl-", result.Url);
                            msg2.AddSubstitution("-imageUrl-", product.ImageFolderUrl);
                            var response2 = sendGridClient2.SendEmailAsync(msg2).Result;
                            response.Successful = true;
                            response.Message = "Create Giftee Degital Redemption Token Successfully";
                            return response;
                        }
                        else
                        {
                            response.Message = result.Error;
                        }
                    }
                    else
                    {
                        response.Message = result.Error;
                        response.Successful = false;
                    }
                }
                catch (Exception ex)
                {
                    response.Message = ex.Message;
                    return response;
                }

                return response;
            }
            private async Task<GifteeResponseModel> ThirdPartyRedemption(Voupon.Database.Postgres.RewardsEntities.Products product,int requestCode)
            {
                var gifteeResponseModel = new GifteeResponseModel();
                var type = await rewardsDBContext.ThirdPartyTypes.Where(x => x.Id == product.ThirdPartyTypeId).FirstOrDefaultAsync();

                if (type.Name.ToUpper() == "GIFTEE")
                {
                    var thirdPartyProduct = await rewardsDBContext.ThirdPartyProducts.Where(x => x.Id == product.ThirdPartyProductId).FirstOrDefaultAsync();
                    if (thirdPartyProduct != null)
                    {
                        try
                        {
                            var requestBody = new GifteeRequestModel
                            {
                                giftcard_option = appSettings.Value.ThirdPartyRedemptions.Giftee.Option,
                                egift_item_id = int.Parse(thirdPartyProduct.ExternalId),
                                request_code = requestCode.ToString()
                            };

                            //  Override with test item for non production env
                            if(appSettings.Value.ThirdPartyRedemptions.Giftee.Url.Contains("staging"))
                            {
                                requestBody.egift_item_id = 5879;
                            }

                            var httpClient = new HttpClient();
                            var request = new HttpRequestMessage(HttpMethod.Post, appSettings.Value.ThirdPartyRedemptions.Giftee.Url + "/generate/egift");

                            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                            request.Content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json"); ;
                            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", appSettings.Value.ThirdPartyRedemptions.Giftee.Token);
                            request.Headers.Add("X-Giftee", "1");


                            var result = await httpClient.SendAsync(request);
                            var resultString = await result.Content.ReadAsStringAsync();

                            dynamic testResult = JsonConvert.DeserializeObject(resultString);


                            if (testResult.url != null)
                            {
                                gifteeResponseModel.Url = testResult.url;
                                gifteeResponseModel.VoucherName = thirdPartyProduct.Name;
                                gifteeResponseModel.IsSuccessful = true;
                                gifteeResponseModel.RawResponse = Convert.ToString(testResult); ;
                                return gifteeResponseModel;
                            }
                            else
                            {
                                gifteeResponseModel.IsSuccessful = false;
                                gifteeResponseModel.Error = JsonConvert.SerializeObject(testResult);
                                return gifteeResponseModel;
                            }
                        }
                        catch (Exception ex)
                        {
                            gifteeResponseModel.Error = ex.ToString();
                            return gifteeResponseModel;
                        }
                    }
                    else
                    {
                        gifteeResponseModel.Error = "Invalid products";
                        return gifteeResponseModel;
                    }
                }
                gifteeResponseModel.Error = "Invalid type";
                return gifteeResponseModel;
            }
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