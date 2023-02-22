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
using Voupon.Rewards.WebApp.ViewModels;

namespace Voupon.Rewards.WebApp.Services.Order.Commands
{
    public class CheckPendingOrderCommand : IRequest<ApiResponseViewModel>
    {

        public class CheckPendingOrderCommandHandler : IRequestHandler<CheckPendingOrderCommand, ApiResponseViewModel>
        {
            private readonly RewardsDBContext rewardsDBContext;
            private readonly IOptions<AppSettings> appSettings;

            public CheckPendingOrderCommandHandler(RewardsDBContext rewardsDBContext, IOptions<AppSettings> appSettings)
            {
                this.rewardsDBContext = rewardsDBContext;
                this.appSettings = appSettings;
            }

            public async Task<ApiResponseViewModel> Handle(CheckPendingOrderCommand request, CancellationToken cancellationToken)
            {
                var apiResponseViewModel = new ApiResponseViewModel();
                try
                {
                    var date = DateTime.Now.AddDays(1);
                    var orders = await rewardsDBContext.Orders.Where(x => x.OrderStatus == 1 && x.CreatedAt.AddDays(1) <= date).Take(10).ToListAsync();

                    if (orders != null && orders.Any())
                    {
                        var sendgridMessageList = new List<SendGridMessage>();
                        foreach (var item in orders)
                        {
                            item.OrderStatus = 3;
                            var sendGridClient2 = new SendGridClient(appSettings.Value.Mailer.Sendgrid.APIKey);
                            var msg2 = new SendGridMessage();
                            msg2.SetTemplateId(appSettings.Value.Mailer.Sendgrid.Templates.CancelPendingOrderByScheduler);
                            msg2.SetFrom(new EmailAddress(appSettings.Value.Emails.Noreply, "Vodus No-Reply"));
                            msg2.SetSubject("Vodus Automated Order Cancellation");
                            msg2.AddTo(new EmailAddress(item.Email));
                            msg2.AddSubstitution("-orderId-", item.ShortId);
                            msg2.AddSubstitution("-message-", "Your order was left unattended for more than 24 hours and have been cancelled. If you want to purchase the deal, please re-add the items again. The order consists of the the following items: " + string.Join(" , ", item.OrderItems.SelectMany(x => x.ProductTitle)));
                            var response2 = sendGridClient2.SendEmailAsync(msg2).Result;
                        }
                        rewardsDBContext.Orders.UpdateRange(orders);
                        await rewardsDBContext.SaveChangesAsync();
                    }
                    apiResponseViewModel.Successful = true;
                    return apiResponseViewModel;
                }
                catch (Exception ex)
                {
                    apiResponseViewModel.Successful = false;
                    apiResponseViewModel.Message = "Fail to check and update pending orders";
                    return apiResponseViewModel;
                }
            }

        }
    }
}
