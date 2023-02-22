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
    public class UpdateBankAccountPendingChangesStatusCommand : IRequest<ApiResponseViewModel>
    {
        public int BankAccountId { get; set; }
        public string Remarks { get; set; }
        public int StatusTypeId { get; set; }
        public DateTime LastUpdatedAt { get; set; }
        public Guid LastUpdatedByUserId { get; set; }
    }

    public class UpdateBankAccountPendingChangesStatusCommandHandler : IRequestHandler<UpdateBankAccountPendingChangesStatusCommand, ApiResponseViewModel>
    {
        RewardsDBContext rewardsDBContext;
        public UpdateBankAccountPendingChangesStatusCommandHandler(RewardsDBContext rewardsDBContext)
        {
            this.rewardsDBContext = rewardsDBContext;
        }

        public async Task<ApiResponseViewModel> Handle(UpdateBankAccountPendingChangesStatusCommand request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();
            if (!string.IsNullOrEmpty(request.Remarks))
                request.Remarks = "Vodus Admin : " + request.Remarks + "<i class='meesage-time'>" + request.LastUpdatedAt.ToString("d-M-yyyy h:mm tt") + "</i>";
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
                response.Message = "Update Bank Account Status Successfully";
            }
            else
            {
                response.Message = "Bank Account not found";
            }
            return response;
        }
    }
}
