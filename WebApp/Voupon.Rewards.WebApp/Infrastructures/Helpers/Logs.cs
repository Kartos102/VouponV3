using Microsoft.EntityFrameworkCore;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Voupon.Database.Postgres.RewardsEntities;

namespace Voupon.Rewards.WebApp.Infrastructures.Helpers
{
    public class Logs
    {
        public string ActionName { get; set; }
        public string Description { get; set; }
        public string JsonData { get; set; }
        public string Email { get; set; }
        public byte TypeId { get; set; }
        public string SendgridAPIKey { get; set; }
        public string TechTeamEmail { get; set; }
        public int? MasterProfileId { get; set; }

        public RewardsDBContext RewardsDBContext;

        public async Task<bool> Error()
        {
            await SendEmail(Description);
            RollBack(RewardsDBContext);
            var errorLogs = new ErrorLogs
            {
                ActionName = ActionName,
                ActionRequest = JsonData,
                CreatedAt = DateTime.Now,
                Errors = Description,
                Email = (!string.IsNullOrEmpty(Email) ? Email : null),
                MasterProfileId = (MasterProfileId.HasValue ? MasterProfileId : null),
                TypeId = TypeId
            };

            RewardsDBContext.ErrorLogs.Add(errorLogs);
            await RewardsDBContext.SaveChangesAsync();
            return true;
        }

        private void RollBack(RewardsDBContext rewardsDBContext)
        {
            var context = rewardsDBContext;
            var changedEntries = context.ChangeTracker.Entries()
                .Where(x => x.State != EntityState.Unchanged).ToList();

            foreach (var entry in changedEntries)
            {
                switch (entry.State)
                {
                    case EntityState.Modified:
                        entry.CurrentValues.SetValues(entry.OriginalValues);
                        entry.State = EntityState.Unchanged;
                        break;
                    case EntityState.Added:
                        entry.State = EntityState.Detached;
                        break;
                    case EntityState.Deleted:
                        entry.State = EntityState.Unchanged;
                        break;
                }
            }
        }
        private async Task<bool> SendEmail(string message)
        {
            TechTeamEmail = "kok.hong@vodus.my, osa@vodus.my, kelvin.goh@vodus.com";
            var appConfig = await RewardsDBContext.AppConfig.FirstOrDefaultAsync();
            if (appConfig == null)
            {
                return true;
            }

            if (!appConfig.IsErrorLogEmailEnabled)
            {
                return true;
            }

            if (TechTeamEmail == null)
            {
                return true;
            }

            var sendGridClient = new SendGridClient(SendgridAPIKey);
            var msg = new SendGridMessage();
            msg.SetTemplateId("d3071f81-7e2d-4e0f-9f1d-4e6ebcab4d96");
            msg.SetFrom(new EmailAddress("noreply@vodus.my", "Vodus No-Reply"));
            msg.SetSubject("Error detected at Vodus.my (Voupon)");

            var techEmail = TechTeamEmail.Split(",");
            for (var email = 0; email < techEmail.Length; email++)
            {
                msg.AddTo(new EmailAddress(techEmail[email].Trim()));
            }
            //msg.AddTo(new EmailAddress("kok.hong@vodus.commy
            msg.AddSubstitution("-EmailBody-", message);
            var response = sendGridClient.SendEmailAsync(msg).Result;

            return true;
        }
    }
}
