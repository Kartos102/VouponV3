using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Merchant.WebApp.ViewModels;
using Microsoft.EntityFrameworkCore;
using SendGrid;
using SendGrid.Helpers.Mail;
using Microsoft.Extensions.Options;

namespace Voupon.Merchant.WebApp.Areas.App.Services.QR.Commands
{
    public class ConfirmQRRedemptionCommand : IRequest<ApiResponseViewModel>
    {
        public string Token { get; set; }
        public int MerchantId { get; set; }

        public int OutletId { get; set; }

        public Guid UserId { get; set; }

        public class ConfirmQRRedemptionCommandHandler : IRequestHandler<ConfirmQRRedemptionCommand, ApiResponseViewModel>
        {
            RewardsDBContext rewardsDBContext;
            private IOptions<AppSettings> appSettings;
            public ConfirmQRRedemptionCommandHandler(RewardsDBContext rewardsDBContext, IOptions<AppSettings> appSettings)
            {
                this.rewardsDBContext = rewardsDBContext;
                this.appSettings = appSettings;
            }

            public async Task<ApiResponseViewModel> Handle(ConfirmQRRedemptionCommand request, CancellationToken cancellationToken)
            {
                var apiResponseViewModel = new ApiResponseViewModel();

                //apiResponseViewModel.Successful = true;
                //apiResponseViewModel.Message = "Redeemed before";
                //return apiResponseViewModel;

                if (string.IsNullOrEmpty(request.Token))
                {
                    apiResponseViewModel.Successful = false;
                    apiResponseViewModel.Message = "Invalid request[0001]";
                    return apiResponseViewModel;
                }

                var token = await rewardsDBContext.InStoreRedemptionTokens.Include(x => x.OrderItem).ThenInclude(x => x.Product).ThenInclude(x => x.ProductOutlets).ThenInclude(x => x.Outlet).Where(x => x.Token == request.Token).FirstOrDefaultAsync();
                if (token == null)
                {
                    apiResponseViewModel.Successful = false;
                    apiResponseViewModel.Message = "Invalid request[0002]";
                    return apiResponseViewModel;
                }

                if (token.MerchantId != request.MerchantId)
                {
                    //  apiResponseViewModel.Successful = false;
                    //  apiResponseViewModel.Message = "Invalid request[0003]";
                    //  return apiResponseViewModel;
                }

                if (token.IsRedeemed)
                {
                    apiResponseViewModel.Successful = false;
                    apiResponseViewModel.Message = "This item have been redeemed before";
                    return apiResponseViewModel;
                }


                var outlet = token.OrderItem.Product.ProductOutlets.Where(x => x.OutletId == request.OutletId).FirstOrDefault();
                if (outlet == null)
                {
                    apiResponseViewModel.Successful = false;
                    apiResponseViewModel.Message = "Invalid request. Product is not allowed to be redeemed at the selected outlet[0003]";
                    return apiResponseViewModel;
                }

                token.IsRedeemed = true;
                token.RedeemedAt = DateTime.Now;
                token.OutletId = request.OutletId;
               
                rewardsDBContext.InStoreRedemptionTokens.Update(token);
                await rewardsDBContext.SaveChangesAsync();

                //  Send item redemeed email               
                var sendGridClient = new SendGridClient(appSettings.Value.Mailer.Sendgrid.APIKey);
                var msg = new SendGridMessage();
                msg.SetTemplateId(appSettings.Value.Mailer.Sendgrid.Templates.RedemptionConfirmation);
                msg.SetFrom(new EmailAddress(appSettings.Value.Emails.Noreply, "Vodus No-Reply"));
                msg.SetSubject("Redemption Confirmation["+ token.ProductTitle + "]");
                msg.AddTo(new EmailAddress(token.Email));
                msg.AddSubstitution("-productTitle-", token.ProductTitle);
                msg.AddSubstitution("-transactionId-", token.CreatedAt.ToString("yyMMdd") + token.Id.ToString().Split("-").First().ToUpper());
                msg.AddSubstitution("-redeemedAt-", token.RedeemedAt.Value.ToString("dd/MM/yyyy"));
                msg.AddSubstitution("-url-", appSettings.Value.App.VouponUrl+ "/Order/History");
                var response = sendGridClient.SendEmailAsync(msg).Result;               

                //apiResponseViewModel.Data = viewModel;
                apiResponseViewModel.Successful = true;
                return apiResponseViewModel;
            }

        }

        public class QRValidatePageViewModel
        {
            public int Id { get; set; }
            public string Token { get; set; }
            public string ProductTitle { get; set; }

            public string ProductImageUrl { get; set; }

            public List<AvailableOutlets> AvailableOutlets { get; set; }
        }

        public class AvailableOutlets
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }
    }

}
