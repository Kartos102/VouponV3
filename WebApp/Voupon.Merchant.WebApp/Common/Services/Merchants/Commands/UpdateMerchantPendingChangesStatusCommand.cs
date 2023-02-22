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
using Voupon.Common.Enum;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Merchant.WebApp.Common.Services.Merchants.Models;
using Voupon.Merchant.WebApp.ViewModels;

namespace Voupon.Merchant.WebApp.Common.Services.Merchants.Commands
{
    public class UpdateMerchantPendingChangesStatusCommand : IRequest<ApiResponseViewModel>
    {
        public int MerchantId { get; set; }
        public string Remarks { get; set; }
        public string PersonInChargeName { get; set; }
        public int StatusTypeId { get; set; }
        public DateTime LastUpdatedAt { get; set; }
        public Guid LastUpdatedByUserId { get; set; }
    }

    public class UpdateMerchantPendingChangesStatusCommandHandler : IRequestHandler<UpdateMerchantPendingChangesStatusCommand, ApiResponseViewModel>
    {
        RewardsDBContext rewardsDBContext;
        private readonly IOptions<AppSettings> appSettings;
        public UpdateMerchantPendingChangesStatusCommandHandler(RewardsDBContext rewardsDBContext, IOptions<AppSettings> appSettings)
        {
            this.rewardsDBContext = rewardsDBContext;
            this.appSettings = appSettings;
        }

        public async Task<ApiResponseViewModel> Handle(UpdateMerchantPendingChangesStatusCommand request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();
            var adminMessage = request.Remarks;
            if (!string.IsNullOrEmpty(request.Remarks))
                request.Remarks = "Vodus Admin : " + request.Remarks + "<i class='meesage-time'>" + request.LastUpdatedAt.ToString("d-M-yyyy h:mm tt") + "</i>";

            var merchant = await rewardsDBContext.Merchants.FirstOrDefaultAsync(x => x.Id == request.MerchantId);
            if (merchant != null)
            {
                var jsonString = "";
                if (String.IsNullOrEmpty(merchant.PendingChanges))
                {
                    merchant.Remarks = /*"Vodus Admin : " +*/ request.Remarks;
                    jsonString = JsonConvert.SerializeObject(merchant);
                }
                else
                    jsonString = merchant.PendingChanges;
                var newItem = JsonConvert.DeserializeObject<Voupon.Database.Postgres.RewardsEntities.Merchants>(jsonString);
                newItem.LastUpdatedAt = request.LastUpdatedAt;
                newItem.LastUpdatedByUserId = request.LastUpdatedByUserId;
                newItem.PendingChanges = "";
                if (!string.IsNullOrEmpty(newItem.Remarks) && !string.IsNullOrEmpty(request.Remarks))
                    newItem.Remarks = newItem.Remarks + "<br>" + request.Remarks;
                else if (!string.IsNullOrEmpty(request.Remarks))
                    newItem.Remarks = request.Remarks;
                newItem.StatusTypeId = request.StatusTypeId;
                merchant.PendingChanges = "";
                merchant.PendingChanges = JsonConvert.SerializeObject(newItem);
                rewardsDBContext.SaveChanges();
                response.Successful = true;
                response.Message = "Update Merchant Status Successfully";
                response.Data = newItem.Remarks;
                if (response.Data == null)
                    response.Data = "";

                if (request.StatusTypeId == 2 || request.StatusTypeId == 3)
                {
                    //  merchant role:  1A436B3D-15A0-4F03-8E4E-0022A5DD5736
                    var userRole = await rewardsDBContext.UserRoles.Where(x => x.MerchantId == merchant.Id && x.RoleId == new Guid("1A436B3D-15A0-4F03-8E4E-0022A5DD5736")).FirstOrDefaultAsync();

                    if(userRole != null)
                    {
                        var user = await rewardsDBContext.Users.Where(x => x.Id == userRole.UserId).FirstOrDefaultAsync();
                        if(user != null)
                        {
                            var url = appSettings.Value.App.BaseUrl + "/App/Business";
                            var sendGridClient = new SendGridClient(appSettings.Value.Mailer.Sendgrid.APIKey);
                            var msg = new SendGridMessage();
                            msg.SetTemplateId(appSettings.Value.Mailer.Sendgrid.Templates.AdminReviewResponse);
                            msg.SetFrom(new EmailAddress(appSettings.Value.Emails.Noreply, "Vodus No-Reply"));
                            msg.SetSubject("Pending Revision");
                            msg.AddTo(new EmailAddress(user.Email));
                            msg.AddSubstitution("-name-", request.PersonInChargeName);//user.Name
                            msg.AddSubstitution("-message-", "Your details is pending revision from your end based on the following comments from our admin:");
                            msg.AddSubstitution("-adminRemark-", adminMessage);
                            msg.AddSubstitution("-url-", url);
                            //var responsess = sendGridClient.SendEmailAsync(msg).Result;
                        }
                    }
                }

            }
            else
            {
                response.Message = "Merchant not found";
            }
            return response;
        }
    }
}
