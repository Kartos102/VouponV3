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
using Voupon.Rewards.WebApp.Infrastructures.Helpers;

namespace Voupon.Merchant.WebApp.Common.Services.Merchants.Commands
{
    public class SendMessageToAdminCommand : IRequest<ApiResponseViewModel>
    {
        public int BankAccountId { get; set; }
        public int PersonInChargeId { get; set; }

        public int MerchantId { get; set; }
        public string Remarks { get; set; }
        public string PersonInChargeName { get; set; }
        public int StatusTypeId { get; set; }
        public DateTime LastUpdatedAt { get; set; }
        public Guid LastUpdatedByUserId { get; set; }
    }

    public class SendMessageToAdminCommandHandler : IRequestHandler<SendMessageToAdminCommand, ApiResponseViewModel>
    {
        RewardsDBContext rewardsDBContext;
        private readonly IOptions<AppSettings> appSettings;
        public SendMessageToAdminCommandHandler(RewardsDBContext rewardsDBContext, IOptions<AppSettings> appSettings)
        {
            this.rewardsDBContext = rewardsDBContext;
            this.appSettings = appSettings;
        }

        public async Task<ApiResponseViewModel> Handle(SendMessageToAdminCommand request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();
            string mercahntNewMessage = request.Remarks;
            if (!string.IsNullOrEmpty(request.Remarks))
                request.Remarks = "Merchant Message : " + request.Remarks + "<i class='meesage-time'>" + request.LastUpdatedAt.ToString("d-M-yyyy h:mm tt") + "</i>";
            rewardsDBContext.Database.BeginTransaction();
            var chatHistory = "";
            string merchantName = "";
            try
            {
                var merchant = await rewardsDBContext.Merchants.FirstOrDefaultAsync(x => x.Id == request.MerchantId);
                if (merchant != null)
                {
                    var jsonString = "";
                    if (String.IsNullOrEmpty(merchant.PendingChanges))
                    {
                        merchant.Remarks = request.Remarks;
                        jsonString = JsonConvert.SerializeObject(merchant);
                    }
                    else
                        jsonString = merchant.PendingChanges;
                    var newItem = JsonConvert.DeserializeObject<Voupon.Database.Postgres.RewardsEntities.Merchants>(jsonString);
                    newItem.LastUpdatedAt = request.LastUpdatedAt;
                    newItem.LastUpdatedByUserId = request.LastUpdatedByUserId;
                    newItem.PendingChanges = "";
                    //if(newItem.StatusTypeId == 1)
                    //{
                    //    newItem.Remarks = "";
                    //}
                    if (!string.IsNullOrEmpty(newItem.Remarks) && !string.IsNullOrEmpty(request.Remarks))
                        newItem.Remarks = newItem.Remarks + "<br>" + request.Remarks;
                    else if (!string.IsNullOrEmpty(request.Remarks))
                        newItem.Remarks = request.Remarks;
                    newItem.StatusTypeId = request.StatusTypeId;
                    merchant.PendingChanges = "";
                    merchant.PendingChanges = JsonConvert.SerializeObject(newItem);
                    rewardsDBContext.SaveChanges();
                    response.Successful = true;
                    response.Message = "Send Message to Admin Successfully";
                    chatHistory = newItem.Remarks;
                    merchantName = merchant.DisplayName;
                }
                else
                {
                    response.Message = "Fail to Send Message to Admin";
                }

                var bankAccount = await rewardsDBContext.BankAccounts.FirstOrDefaultAsync(x => x.Id == request.BankAccountId);
                if (bankAccount != null)
                {
                    var jsonString = "";
                    if (String.IsNullOrEmpty(bankAccount.PendingChanges))
                    {
                        jsonString = JsonConvert.SerializeObject(bankAccount);
                    }
                    else
                        jsonString = bankAccount.PendingChanges;
                    var newItem = JsonConvert.DeserializeObject<Voupon.Database.Postgres.RewardsEntities.BankAccounts>(jsonString);
                    newItem.LastUpdatedAt = request.LastUpdatedAt;
                    newItem.LastUpdatedByUserId = request.LastUpdatedByUserId;
                    newItem.PendingChanges = "";
                    if (!string.IsNullOrEmpty(newItem.Remarks) && !string.IsNullOrEmpty(request.Remarks))
                        newItem.Remarks = newItem.Remarks + "<br>" + request.Remarks;
                    else if (!string.IsNullOrEmpty(request.Remarks))
                        newItem.Remarks = request.Remarks;
                    newItem.StatusTypeId = request.StatusTypeId;
                    bankAccount.PendingChanges = "";
                    bankAccount.PendingChanges = JsonConvert.SerializeObject(newItem);
                    rewardsDBContext.SaveChanges();
                    response.Successful = true;
                    response.Message = "Send Message to Admin Successfully";
                }
                else
                {
                    response.Message = "Fail to Send Message to Admin";
                }
                var personInCharge = await rewardsDBContext.PersonInCharges.FirstOrDefaultAsync(x => x.Id == request.PersonInChargeId);
                if (personInCharge != null)
                {
                    var jsonString = "";
                    if (String.IsNullOrEmpty(personInCharge.PendingChanges))
                    {
                        jsonString = JsonConvert.SerializeObject(personInCharge);
                    }
                    else
                        jsonString = personInCharge.PendingChanges;
                    var newItem = JsonConvert.DeserializeObject<Voupon.Database.Postgres.RewardsEntities.PersonInCharges>(jsonString);
                    newItem.LastUpdatedAt = request.LastUpdatedAt;
                    newItem.LastUpdatedByUser = request.LastUpdatedByUserId;
                    newItem.PendingChanges = "";
                    if (!string.IsNullOrEmpty(newItem.Remarks) && !string.IsNullOrEmpty(request.Remarks))
                        newItem.Remarks = newItem.Remarks + "<br>" + request.Remarks;
                    else if (!string.IsNullOrEmpty(request.Remarks))
                        newItem.Remarks = request.Remarks;
                    newItem.StatusTypeId = request.StatusTypeId;
                    personInCharge.PendingChanges = "";
                    personInCharge.PendingChanges = JsonConvert.SerializeObject(newItem);
                    rewardsDBContext.SaveChanges();
                    rewardsDBContext.Database.CommitTransaction();
                    response.Successful = true;
                    response.Message = "Send Message to Admin Successfully";
                }
                else
                {
                    response.Message = "Fail to Send Message to Admin";
                }
            }
            catch (Exception ex)
            {
                rewardsDBContext.Database.RollbackTransaction();
                response.Successful = false;
                response.Message = "Fail to Send Message to Admin";
            }
            try
            {
                SendEmail sendEmail = new SendEmail();
                sendEmail.MerchantId = request.MerchantId;
                sendEmail.MerchantName = merchantName;
                sendEmail.AdminEmailType = 1;
                sendEmail.SendgridAPIKey = appSettings.Value.Mailer.Sendgrid.APIKey;
                sendEmail.Url = appSettings.Value.App.BaseUrl;
                //  Send email notuification to Admin
                await sendEmail.SendMerchantNotificationEmailToAdmin(mercahntNewMessage);
            }
            catch(Exception ex)
            {
                response.Data = chatHistory;
                return response;
            }
            response.Data = chatHistory;
            return response;
        }
    }
}
