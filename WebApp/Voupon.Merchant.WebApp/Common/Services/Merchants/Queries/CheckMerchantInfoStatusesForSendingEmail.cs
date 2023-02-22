using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
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

namespace Voupon.Merchant.WebApp.Common.Services.Merchants.Queries
{
    public class CheckMerchantInfoStatusesForSendingEmail : IRequest<ApiResponseViewModel>
    {
        public int MerchantId { get; set; }
    }

    public class CheckMerchantInfoStatusesForSendingEmailHandler : IRequestHandler<CheckMerchantInfoStatusesForSendingEmail, ApiResponseViewModel>
    {
        RewardsDBContext rewardsDBContext;
        private readonly IOptions<AppSettings> appSettings;

        public CheckMerchantInfoStatusesForSendingEmailHandler(RewardsDBContext rewardsDBContext, IOptions<AppSettings> appSettings)
        {
            this.rewardsDBContext = rewardsDBContext;
            this.appSettings = appSettings;
        }

        public async Task<ApiResponseViewModel> Handle(CheckMerchantInfoStatusesForSendingEmail request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();
            try
            {
                var merchant = await rewardsDBContext.Merchants.FirstOrDefaultAsync(x => x.Id == request.MerchantId);
                var personInCharge = await rewardsDBContext.PersonInCharges.FirstOrDefaultAsync(x => x.MerchantId == request.MerchantId);
                var bankAcoount = await rewardsDBContext.BankAccounts.FirstOrDefaultAsync(x => x.MerchantId == request.MerchantId);
                if (merchant != null && personInCharge != null && bankAcoount != null)
                {
                    Voupon.Database.Postgres.RewardsEntities.Merchants merchantPendingChanges = new Voupon.Database.Postgres.RewardsEntities.Merchants();
                    Voupon.Database.Postgres.RewardsEntities.BankAccounts bankPendingChanges = new Voupon.Database.Postgres.RewardsEntities.BankAccounts();
                    Voupon.Database.Postgres.RewardsEntities.PersonInCharges personInChargePendingChanges = new Voupon.Database.Postgres.RewardsEntities.PersonInCharges();
                    int bankStatus = 0;
                    int personInChargeStatus = 0;
                    int merchantStatus = 0;
                    if (String.IsNullOrEmpty(bankAcoount.PendingChanges))
                    {
                        bankStatus = bankAcoount.StatusTypeId;
                    }
                    else
                    {
                        string bankJson = bankAcoount.PendingChanges;
                        bankPendingChanges = JsonConvert.DeserializeObject<Voupon.Database.Postgres.RewardsEntities.BankAccounts>(bankJson);
                        bankStatus = bankPendingChanges.StatusTypeId;
                    }

                    if (String.IsNullOrEmpty(merchant.PendingChanges))
                    {
                        merchantStatus = merchant.StatusTypeId;
                    }
                    else
                    {
                        string merchantJson = merchant.PendingChanges;
                        merchantPendingChanges = JsonConvert.DeserializeObject<Voupon.Database.Postgres.RewardsEntities.Merchants>(merchantJson);
                        merchantStatus = merchantPendingChanges.StatusTypeId;
                    }

                    if (String.IsNullOrEmpty(personInCharge.PendingChanges))
                    {
                        personInChargeStatus = personInCharge.StatusTypeId;
                    }
                    else
                    {
                        string personInChargeJson = personInCharge.PendingChanges;
                        personInChargePendingChanges = JsonConvert.DeserializeObject<Voupon.Database.Postgres.RewardsEntities.PersonInCharges>(personInChargeJson);
                        personInChargeStatus = personInChargePendingChanges.StatusTypeId;
                    }


                    int pendingReviewCount = 0;
                    if (merchantStatus == StatusTypeEnum.PENDING_REVIEW)
                        pendingReviewCount++;
                    if (personInChargeStatus == StatusTypeEnum.PENDING_REVIEW)
                        pendingReviewCount++;
                    if (bankStatus == StatusTypeEnum.PENDING_REVIEW)
                        pendingReviewCount++;
                    if (pendingReviewCount != 3)
                    {
                        SendEmail sendEmail = new SendEmail();
                        sendEmail.MerchantId = request.MerchantId;
                        sendEmail.MerchantName = merchant.DisplayName;
                        sendEmail.AdminEmailType = 2;
                        sendEmail.SendgridAPIKey = appSettings.Value.Mailer.Sendgrid.APIKey;
                        sendEmail.Url = appSettings.Value.App.BaseUrl;
                        //  Send email notuification to Admin
                        await sendEmail.SendMerchantNotificationEmailToAdmin("");
                        response.Successful = true;
                        response.Message = "Sent Email Successfully";
                        response.Data = true;

                        if(merchantStatus != StatusTypeEnum.PENDING_REVIEW)
                        {
                            merchant.PendingChanges = "";
                            merchantPendingChanges.StatusTypeId = 2;
                            merchant.PendingChanges = JsonConvert.SerializeObject(merchantPendingChanges);
                        }
                        if(personInChargeStatus != StatusTypeEnum.PENDING_REVIEW)
                        {
                            personInCharge.PendingChanges = "";
                            personInChargePendingChanges.StatusTypeId = 2;
                            personInCharge.PendingChanges = JsonConvert.SerializeObject(personInChargePendingChanges);
                        }
                        if(bankStatus != StatusTypeEnum.PENDING_REVIEW)
                        {
                            bankAcoount.PendingChanges = "";
                            bankPendingChanges.StatusTypeId = 2;
                            bankAcoount.PendingChanges = JsonConvert.SerializeObject(bankPendingChanges);
                        }
                        rewardsDBContext.SaveChanges();
                    }
                    else
                    {
                        response.Successful = true;
                        response.Message = "No need to send email";
                        response.Data = false;
                    }
                }
                else
                {
                    response.Successful = false;
                    response.Message = "Merchant not found";
                }
           
            }
            catch (Exception ex)
            {
                response.Successful = false;
                response.Message = ex.Message;
            }
            return response;
        }
    }
}
