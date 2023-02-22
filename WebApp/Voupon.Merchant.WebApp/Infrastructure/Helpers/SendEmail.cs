using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Merchant.WebApp;

namespace Voupon.Rewards.WebApp.Infrastructures.Helpers
{
    public class SendEmail
    {
        public string Email { get; set; }
        public string SendgridAPIKey { get; set; }
        public string AdminEmails { get; set; }
        public int AdminEmailType { get; set; }
        public int MerchantId { get; set; }
        public string MerchantName { get; set; }
        public string Url { get; set; }

        public RewardsDBContext RewardsDBContext;


        public async Task<bool> SendMerchantNotificationEmailToAdmin(string message)
        {
            AdminEmails = "shh1612@gmail.com,vodus.voupon@gmail.com";
       
            var sendGridClient = new SendGridClient(SendgridAPIKey);
            var msg = new SendGridMessage();
            msg.SetTemplateId("d3071f81-7e2d-4e0f-9f1d-4e6ebcab4d96");
            msg.SetFrom(new EmailAddress("noreply@vodus.my", "Vodus No-Reply"));
            if(AdminEmailType == 1)
            {
                msg.SetSubject($"You have a message from Merchant {MerchantName}");
                msg.AddSubstitution("-EmailBody-", "You got a message from <b>" + MerchantName + "</b> Merchant<br> Message: " + message + $"<br> Review and reply : <a href='{Url}/Admin/Merchants/Details/{MerchantId}'>Reply to merchant</a>");
            }
            else if (AdminEmailType == 2)
            {
                msg.SetSubject($"Pending Revision for Merchant {MerchantName}");
                msg.AddSubstitution("-EmailBody-", "The merchant <b>" + MerchantName + $"</b> have updated their info and awaiting revision <br> Review : <a href='{Url}/Admin/Merchants/Details/{MerchantId}'>Review changes for merchant</a>");
            }

            var techEmail = AdminEmails.Split(",");
            for (var email = 0; email < techEmail.Length; email++)
            {
                msg.AddTo(new EmailAddress(techEmail[email].Trim()));
            }
            //msg.AddTo(new EmailAddress("kok.hong@vodus.my"));
            var response = sendGridClient.SendEmailAsync(msg).Result;

            return true;
        }

    }
}
