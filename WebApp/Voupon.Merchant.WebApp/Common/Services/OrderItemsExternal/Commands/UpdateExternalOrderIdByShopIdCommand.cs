using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Database.Postgres.VodusEntities;
using Voupon.Merchant.WebApp.Common.Services.Utility.Queries;
using Voupon.Merchant.WebApp.ViewModels;

namespace Voupon.Merchant.WebApp.Common.Services.OrderItemsExternal.Commands
{
    public class UpdateExternalOrderIdByShopIdCommand : IRequest<ApiResponseViewModel>
    {
        public Guid ExternalOrderShopId { get; set; }
        public string LastUpdatedByUserName { get; set; }
        public string ExternalOrderId { get; set; }


        public class UpdateExternalOrderIdByShopIdCommandHandler : IRequestHandler<UpdateExternalOrderIdByShopIdCommand, ApiResponseViewModel>
        {
            RewardsDBContext rewardsDBContext;
            VodusV2Context vodusV2Context;
            IOptions<AppSettings> appSettings;
            public UpdateExternalOrderIdByShopIdCommandHandler(RewardsDBContext rewardsDBContext, VodusV2Context vodusV2Context, IOptions<AppSettings> appSettings)
            {
                this.rewardsDBContext = rewardsDBContext;
                this.vodusV2Context = vodusV2Context;
                this.appSettings = appSettings;
            }

            public async Task<ApiResponseViewModel> Handle(UpdateExternalOrderIdByShopIdCommand request, CancellationToken cancellationToken)
            {
                ApiResponseViewModel apiResponseViewModel = new ApiResponseViewModel();
                var orderShopItem = await rewardsDBContext.OrderShopExternal.Include(x => x.OrderItemExternal).Where(x => x.Id == request.ExternalOrderShopId).FirstOrDefaultAsync();

                try
                {
                    if (orderShopItem != null && orderShopItem.OrderItemExternal.Count > 0)
                    {
                        foreach (var orderItem in orderShopItem.OrderItemExternal)
                        {
                            orderItem.LastUpdatedAt = DateTime.Now;
                            orderItem.LastUpdatedByUser = request.LastUpdatedByUserName;
                        }

                        if (request.ExternalOrderId != orderShopItem.ExternalOrderId)
                        {
                            orderShopItem.ShippingDetailsJson = null;
                        }
                        orderShopItem.ExternalOrderId = request.ExternalOrderId;
                        
                        rewardsDBContext.SaveChanges();


                        //To Add Send Email to user
                        //if (request.StatusId == 3)
                        //{
                        //    //  Send email to users
                        //    var sendGridClient = new SendGridClient(appSettings.Value.Mailer.Sendgrid.APIKey);
                        //    var msg = new SendGridMessage();
                        //    msg.SetTemplateId(appSettings.Value.Mailer.Sendgrid.Templates.DeliveryTrackingConfirmation);
                        //    msg.SetFrom(new EmailAddress(appSettings.Value.Emails.Noreply, "Vodus No-Reply"));
                        //    msg.SetSubject("Vodus item delivery details for " + orderItem.ProductTitle);
                        //    msg.AddTo(new EmailAddress(orderItem.Order.Email));
                        //    msg.AddSubstitution("-productTitle-", orderItem.ProductTitle);
                        //    msg.AddSubstitution("-courierName-", request.CourierProvider);
                        //    msg.AddSubstitution("-courierUrl-", request.CourierProviderUrl + "/" + request.Token);
                        //    msg.AddSubstitution("-courierTrackingNumber-", request.Token);
                        //    var responsed = sendGridClient.SendEmailAsync(msg).Result;
                        //}


                        apiResponseViewModel.Message = "Succesfully Updated status";
                        apiResponseViewModel.Successful = true;
                        return apiResponseViewModel;
                    }
                    else
                    {
                        apiResponseViewModel.Message = "Fail to get order external and Updated status";
                        apiResponseViewModel.Successful = false;
                        return apiResponseViewModel;
                    }


                }
                catch (Exception ex)
                {
                    apiResponseViewModel.Successful = false;
                    apiResponseViewModel.Message = "Fail to Updated status";
                    return apiResponseViewModel;
                }

            }
        }

    }

}
