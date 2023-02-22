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
    public class UpdateBankAccountCommand : IRequest<ApiResponseViewModel>
    {
        public int Id { get; set; }
        public int BankId { get; set; }
        public string Name { get; set; }
        public string AccountNumber { get; set; }
        public string DocumentUrl { get; set; }
        public DateTime LastUpdatedAt { get; set; }
        public Guid LastUpdatedByUserId { get; set; }
    }

    public class UpdateBankAccountCommandHandler : IRequestHandler<UpdateBankAccountCommand, ApiResponseViewModel>
    {
        RewardsDBContext rewardsDBContext;
        public UpdateBankAccountCommandHandler(RewardsDBContext rewardsDBContext)
        {
            this.rewardsDBContext = rewardsDBContext;
        }

        public async Task<ApiResponseViewModel> Handle(UpdateBankAccountCommand request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();

            var bankAccount = await rewardsDBContext.BankAccounts.FirstAsync(x => x.Id == request.Id);
            if (bankAccount != null)
            {
                var bankAccount2 = await rewardsDBContext.BankAccounts.AsNoTracking().FirstAsync(x => x.Id == request.Id);
                bankAccount2.LastUpdatedByUserId = request.LastUpdatedByUserId;
                bankAccount2.DocumentUrl = request.DocumentUrl;
                bankAccount2.Name = request.Name;
                bankAccount2.AccountNumber = request.AccountNumber;
                bankAccount2.BankId = request.BankId;
                bankAccount2.LastUpdatedAt = request.LastUpdatedAt;
                bankAccount2.PendingChanges = "";

                bankAccount.LastUpdatedByUserId = request.LastUpdatedByUserId;
                bankAccount.DocumentUrl = request.DocumentUrl;
                bankAccount.Name = request.Name;
                bankAccount.AccountNumber = request.AccountNumber;
                bankAccount.BankId = request.BankId;
                bankAccount.LastUpdatedAt = request.LastUpdatedAt;
                bankAccount.PendingChanges = "";
                var pendingChanges = JsonConvert.SerializeObject(bankAccount2);
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
