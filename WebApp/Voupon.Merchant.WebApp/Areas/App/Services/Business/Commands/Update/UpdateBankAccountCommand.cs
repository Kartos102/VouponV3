using MediatR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Merchant.WebApp.Areas.App.Services.Business.ViewModels;
using Voupon.Merchant.WebApp.ViewModels;

namespace Voupon.Merchant.WebApp.Areas.App.Services.Business.Commands.Update
{
    public class UpdateBankAccountCommand : IRequest<ApiResponseViewModel>
    {
        public int Id { get; set; }
        public int BankId { get; set; }
        public string Name { get; set; }
        public string AccountNumber { get; set; }
        public string Document { get; set; }
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
            if(bankAccount != null)
            {
                var bankAccountModel = new BankAccounts();
                bankAccountModel.Name = request.Name;
                bankAccountModel.AccountNumber = request.AccountNumber;
                bankAccountModel.BankId = request.BankId;
                bankAccountModel.LastUpdatedAt = DateTime.Now;
                bankAccountModel.PendingChanges = "";
                var pendingChanges = JsonConvert.SerializeObject(bankAccountModel);
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
