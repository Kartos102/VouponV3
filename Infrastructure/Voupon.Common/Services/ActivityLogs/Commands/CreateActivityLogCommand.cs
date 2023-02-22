using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Voupon.Database.Postgres.RewardsEntities;

namespace Voupon.Common.Services.ActivityLogs.Commands
{
    public class CreateActivityLogCommand
    {
        private RewardsDBContext _rewardsDBContext;
        public CreateActivityLogCommand(RewardsDBContext rewardsDBContext)
        {
            _rewardsDBContext = rewardsDBContext;
        }

        public async Task<bool> Create(CreateActivityRequest request)
        {
            var activityLogs = new Database.Postgres.RewardsEntities.ActivityLogs
            {
                Id = new Guid(),
                ActionData = request.ActionData,
                ActionId = request.ActionId,
                ActionName = request.ActionName,
                ActionFunction = request.ActionFunction,
                IsSuccessful = request.IsSuccessful,
                Message = request.Message,
                TriggerBy = request.TriggerBy,
                TriggerFor = request.TriggerFor,
                CreatedAt = DateTime.Now
            };

            await _rewardsDBContext.ActivityLogs.AddAsync(activityLogs);
            await _rewardsDBContext.SaveChangesAsync();
            return true;
        }

        public class CreateActivityRequest
        {
            public Guid Id { get; set; }
            public string ActionName { get; set; }
            public string ActionFunction { get; set; }
            public string ActionId { get; set; }
            public string ActionData { get; set; }
            public string Message { get; set; }
            public string Email { get; set; }
            public bool IsSuccessful { get; set; }
            public string TriggerFor { get; set; }
            public string TriggerBy { get; set; }
            public DateTime CreatedAt { get; set; }
        }
    }
}

