using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Merchant.WebApp.ViewModels;

namespace Voupon.Merchant.WebApp.Common.Services.DeliveryRedemptionTokens.Commands
{
    public class UpdateDeliveryRedemptionTokensCommandForOrderMerchant : IRequest<ApiResponseViewModel>
    {
        public Guid Id { get; set; }
        public int MerchantId { get; set; }
        public string CourierProvider { get; set; }

        public string CourierProviderUrl { get; set; }
        public string Token { get; set; }
    }
    public class UpdateDeliveryRedemptionTokensCommandForOrderMerchantHandler : IRequestHandler<UpdateDeliveryRedemptionTokensCommandForOrderMerchant, ApiResponseViewModel>
    {
        RewardsDBContext rewardsDBContext;
        private readonly IOptions<AppSettings> appSettings;
        public UpdateDeliveryRedemptionTokensCommandForOrderMerchantHandler(RewardsDBContext rewardsDBContext, IOptions<AppSettings> appSettings)
        {
            this.rewardsDBContext = rewardsDBContext;
            this.appSettings = appSettings;
        }

        public async Task<ApiResponseViewModel> Handle(UpdateDeliveryRedemptionTokensCommandForOrderMerchant request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();
            try
            {
                var order = await rewardsDBContext.Orders.Include(x => x.OrderItems).ThenInclude(x => x.DeliveryRedemptionTokens).Where(x => x.Id == request.Id).FirstOrDefaultAsync();
                if (order != null)
                {
                    string productsTitles = order.OrderItems.First().ProductTitle;
                    var previousOrderid = order.OrderItems.First().OrderId;
                    foreach (var Item in order.OrderItems)
                    {
                        if (Item.MerchantId == request.MerchantId)
                        {
                            Item.DeliveryRedemptionTokens.FirstOrDefault().UpdateTokenAt = DateTime.Now;
                            Item.DeliveryRedemptionTokens.FirstOrDefault().IsActivated = true;
                            Item.DeliveryRedemptionTokens.FirstOrDefault().CourierProvider = request.CourierProvider;
                            Item.DeliveryRedemptionTokens.FirstOrDefault().Token = request.Token;
                            //rewardsDBContext.DeliveryRedemptionTokens.Update(Item.DeliveryRedemptionTokens.FirstOrDefault());
                            Item.Status = 2;
                            rewardsDBContext.OrderItems.Update(Item);

                            if(Item.OrderId != previousOrderid)
                            {
                                productsTitles += ", " + Item.ProductTitle;
                            }
                            previousOrderid = Item.OrderId;
                        }
                    }

                    //if (deliveryRedemptionTokens == null)
                    //{
                    //    response.Successful = false;
                    //    response.Message = "No Delivery Redemption Tokens";
                    //    return response;
                    //}


                    await rewardsDBContext.SaveChangesAsync();
                    //  Send email to users
                    var sendGridClient = new SendGridClient(appSettings.Value.Mailer.Sendgrid.APIKey);
                    var msg = new SendGridMessage();
                    msg.SetTemplateId(appSettings.Value.Mailer.Sendgrid.Templates.DeliveryTrackingConfirmation);
                    msg.SetFrom(new EmailAddress("noreply@vodus.com", "Vodus No-Reply"));
                    //msg.SetSubject("Vodus item delivery details for " + order.OrderItems.FirstOrDefault().DeliveryRedemptionTokens.FirstOrDefault().ProductTitle);
                    msg.SetSubject("Vodus item delivery details");
                    msg.AddTo(new EmailAddress(order.OrderItems.FirstOrDefault().DeliveryRedemptionTokens.FirstOrDefault().Email));
                    //msg.AddSubstitution("-productTitle-", order.OrderItems.FirstOrDefault().DeliveryRedemptionTokens.FirstOrDefault().ProductTitle);
                    msg.AddSubstitution("-productTitle-", productsTitles);
                    msg.AddSubstitution("-courierName-", request.CourierProvider);
                    msg.AddSubstitution("-courierUrl-", request.CourierProviderUrl + "/" + request.Token);
                    msg.AddSubstitution("-courierTrackingNumber-", request.Token);
                    var responsed = sendGridClient.SendEmailAsync(msg).Result;
                    response.Successful = true;
                    response.Message = "Delivery redemption token have been successfully updated";
                }
                response.Successful = true;
            }
            catch (Exception ex)
            {
                response.Message = "Order can't be found ";
                response.Message = ex.Message;
            }

            return response;
        }
    }
}
