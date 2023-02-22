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
    public class UpdateDeliveryRedemptionTokensCommand : IRequest<ApiResponseViewModel>
    {
        public int Id { get; set; }
        public string CourierProvider { get; set; }

        public string CourierProviderUrl { get; set; }
        public string Token { get; set; }
    }
    public class UpdateDeliveryRedemptionTokensCommandHandler : IRequestHandler<UpdateDeliveryRedemptionTokensCommand, ApiResponseViewModel>
    {
        RewardsDBContext rewardsDBContext;
        private readonly IOptions<AppSettings> appSettings;
        public UpdateDeliveryRedemptionTokensCommandHandler(RewardsDBContext rewardsDBContext, IOptions<AppSettings> appSettings)
        {
            this.rewardsDBContext = rewardsDBContext;
            this.appSettings = appSettings;
        }

        public async Task<ApiResponseViewModel> Handle(UpdateDeliveryRedemptionTokensCommand request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();
            try
            {
                var deliveryRedemptionTokens = await rewardsDBContext.DeliveryRedemptionTokens.Include(x=> x.OrderItem).Where(x => x.Id == request.Id).FirstOrDefaultAsync();

                if (deliveryRedemptionTokens == null)
                {
                    response.Successful = false;
                    response.Message = "No Delivery Redemption Tokens";
                    return response;
                }

                var allOrderItemsForSameProduct = await rewardsDBContext.OrderItems.Include(x=> x.DeliveryRedemptionTokens).Where(x => x.OrderId == deliveryRedemptionTokens.OrderItem.OrderId && x.ProductId == deliveryRedemptionTokens.ProductId).ToListAsync();

                foreach(var orderItem in allOrderItemsForSameProduct)
                {
                    orderItem.Status = 2;
                    rewardsDBContext.OrderItems.Update(orderItem);
                    foreach (var deliveryRedemptionToken in orderItem.DeliveryRedemptionTokens)
                    {
                        deliveryRedemptionToken.UpdateTokenAt = DateTime.Now;
                        deliveryRedemptionToken.IsActivated = true;
                        deliveryRedemptionToken.CourierProvider = request.CourierProvider;
                        deliveryRedemptionToken.Token = request.Token;

                        rewardsDBContext.DeliveryRedemptionTokens.Update(deliveryRedemptionTokens);
                    }
                }
                
                

                //var orderItem = await rewardsDBContext.OrderItems.Where(x => x.Id == deliveryRedemptionTokens.OrderItemId).FirstOrDefaultAsync();
                //if(orderItem != null)
                //{
                //    orderItem.Status = 2;
                //    rewardsDBContext.OrderItems.Update(orderItem);
                //}

                await rewardsDBContext.SaveChangesAsync();

                //  Send email to users
                var sendGridClient = new SendGridClient(appSettings.Value.Mailer.Sendgrid.APIKey);
                var msg = new SendGridMessage();
                msg.SetTemplateId(appSettings.Value.Mailer.Sendgrid.Templates.DeliveryTrackingConfirmation);
                msg.SetFrom(new EmailAddress(appSettings.Value.Emails.Noreply, "Vodus No-Reply"));
                msg.SetSubject("Vodus item delivery details for " + deliveryRedemptionTokens.ProductTitle);
                msg.AddTo(new EmailAddress(deliveryRedemptionTokens.Email));
                msg.AddSubstitution("-productTitle-", deliveryRedemptionTokens.ProductTitle);
                msg.AddSubstitution("-courierName-", request.CourierProvider);
                msg.AddSubstitution("-courierUrl-", request.CourierProviderUrl + "/" + request.Token);
                msg.AddSubstitution("-courierTrackingNumber-", request.Token);
                var responsed = sendGridClient.SendEmailAsync(msg).Result;

                response.Successful = true;
                response.Message = "Delivery redemption token have been successfully updated";
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }

            return response;
        }
    }
}
