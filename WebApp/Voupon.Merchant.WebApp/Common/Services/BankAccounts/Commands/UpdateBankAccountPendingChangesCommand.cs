using MediatR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Common.Enum;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Merchant.WebApp.ViewModels;

namespace Voupon.Merchant.WebApp.Common.Services.BankAccounts.Commands
{
    public class UpdateBankAccountPendingChangesCommand : IRequest<ApiResponseViewModel>
    {
        public int Id { get; set; }
        public int? BankId { get; set; }
        public string Name { get; set; }
        public string AccountNumber { get; set; }
        public string DocumentUrl { get; set; }
        public int StatusTypeId { get; set; }
        public DateTime LastUpdatedAt { get; set; }
        public Guid LastUpdatedByUserId { get; set; }
    }

    public class UpdateBankAccountPendingChangesCommandHandler : IRequestHandler<UpdateBankAccountPendingChangesCommand, ApiResponseViewModel>
    {
        RewardsDBContext rewardsDBContext;
        public UpdateBankAccountPendingChangesCommandHandler(RewardsDBContext rewardsDBContext)
        {
            this.rewardsDBContext = rewardsDBContext;
        }

        public async Task<ApiResponseViewModel> Handle(UpdateBankAccountPendingChangesCommand request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();

            var bankAccount = await rewardsDBContext.BankAccounts.FirstOrDefaultAsync(x => x.Id == request.Id);
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
                newItem.StatusTypeId = request.StatusTypeId;
                newItem.LastUpdatedByUserId = request.LastUpdatedByUserId;
                newItem.DocumentUrl = request.DocumentUrl;
                newItem.Name = request.Name;
                newItem.AccountNumber = request.AccountNumber;
                newItem.BankId = request.BankId;
                newItem.LastUpdatedAt = request.LastUpdatedAt;
                bankAccount.PendingChanges = "";
                var pendingChanges = JsonConvert.SerializeObject(newItem);
                bankAccount.PendingChanges = pendingChanges;
                rewardsDBContext.SaveChanges();
                response.Successful = true;
                response.Message = "Update Bank Account Successfully";
            }
            else
            {
                response.Message = "Bank Account not found";
            }
            return response;
        }
    }
}
