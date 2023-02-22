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

namespace Voupon.Rewards.WebApp.Services.PromoCode.Commands
{
    public class SubscriberPromoCodeCommand : IRequest<ApiResponseViewModel>
    {
        public string Email { get; set; }

        public class SubscriberPromoCodeCommandHandler : IRequestHandler<SubscriberPromoCodeCommand, ApiResponseViewModel>
        {
            private readonly RewardsDBContext rewardsDBContext;
            private readonly IOptions<AppSettings> appSettings;


            private string GetRootDomain(string host)
            {
                var filterHost = host.Replace("http://", "").Replace("https://", "");
                return filterHost.Split('/')[0];
            }

            public SubscriberPromoCodeCommandHandler(RewardsDBContext rewardsDBContext, IOptions<AppSettings> appSettings)
            {
                this.rewardsDBContext = rewardsDBContext;
                this.appSettings = appSettings;
            }

            public async Task<ApiResponseViewModel> Handle(SubscriberPromoCodeCommand request, CancellationToken cancellationToken)
            {

                var apiResponseViewModel = new ApiResponseViewModel();

                try
                {
                    //   Add email to mailing list if its a new email
                    var mailingList = await rewardsDBContext.MailingLists.Where(x => x.Email == request.Email).FirstOrDefaultAsync();
                    
                    if(mailingList == null)
                    {
                        await rewardsDBContext.MailingLists.AddAsync(new MailingLists
                        {
                            Email = request.Email,
                            CreatedAt = DateTime.Now,
                            IsSubscribe = true
                        });
                        await rewardsDBContext.SaveChangesAsync();
                    }

                    var firstTimePromoCode = await rewardsDBContext.PromoCodes.Where(x => x.IsFirstTimeUserOnly == true && x.IsNewSignupUserOnly == true).FirstOrDefaultAsync();
                    if (firstTimePromoCode == null)
                    {
                        apiResponseViewModel.Successful = false;
                        apiResponseViewModel.Message = "Invalid promo code";
                        return apiResponseViewModel;
                    }

                    string apiKey = appSettings.Value.Mailer.Sendgrid.APIKey;

                    var sendGridClient = new SendGridClient(apiKey);
                    var msg = new SendGridMessage();
                    msg.SetTemplateId(appSettings.Value.Mailer.Sendgrid.Templates.FirstTimeUserPromoEmail);
                    msg.SetFrom(new EmailAddress(appSettings.Value.Emails.Noreply, "Vodus No-Reply"));
                    msg.SetSubject("Vodus Promo Code");
                    msg.AddTo(new EmailAddress(request.Email));
                    msg.AddSubstitution("-promocode-", firstTimePromoCode.PromoCode);
                    var response = sendGridClient.SendEmailAsync(msg).Result;
                    apiResponseViewModel.Message = "Promo code have been sent to your email";
                    apiResponseViewModel.Successful = true;
                    return apiResponseViewModel;
                }
                catch (Exception ex)
                {
                    apiResponseViewModel.Successful = false;
                    apiResponseViewModel.Message = "We are busy at the moment. Please try again later";
                    return apiResponseViewModel;
                }
            }

        }
    }

}
