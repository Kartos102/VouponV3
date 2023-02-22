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

namespace Voupon.Merchant.WebApp.Common.Services.OrderItems.Commands
{
    public class UpdateMerchantOrderItemStatusCommand : IRequest<ApiResponseViewModel>
    {
        public Guid OrderItemId { get; set; }
        public byte StatusId { get; set; }

        public class UpdateMerchantOrderItemStatusCommandHandler : IRequestHandler<UpdateMerchantOrderItemStatusCommand, ApiResponseViewModel>
        {
            RewardsDBContext rewardsDBContext;
            VodusV2Context vodusV2Context;
            IOptions<AppSettings> appSettings;
            public UpdateMerchantOrderItemStatusCommandHandler(RewardsDBContext rewardsDBContext, VodusV2Context vodusV2Context, IOptions<AppSettings> appSettings)
            {
                this.rewardsDBContext = rewardsDBContext;
                this.vodusV2Context = vodusV2Context;
                this.appSettings = appSettings;
            }

            public async Task<ApiResponseViewModel> Handle(UpdateMerchantOrderItemStatusCommand request, CancellationToken cancellationToken)
            {
                ApiResponseViewModel apiResponseViewModel = new ApiResponseViewModel();
                var orderItem = await rewardsDBContext.OrderItems.Where(x => x.Id == request.OrderItemId).FirstOrDefaultAsync();

                try
                {
                    if (orderItem != null)
                    {
                        orderItem.Status = request.StatusId;

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
