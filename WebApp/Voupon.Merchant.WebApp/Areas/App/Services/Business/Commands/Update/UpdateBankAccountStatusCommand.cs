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
    public class UpdateBankAccountStatusCommand : IRequest<ApiResponseViewModel>
    {
        public int Id { get; set; }
        public int StatusTypeId { get; set; }
        public string Remarks { get; set; }
    }

    public class UpdateBankAccountStatusCommandHandler : IRequestHandler<UpdateBankAccountStatusCommand, ApiResponseViewModel>
    {
        RewardsDBContext rewardsDBContext;
        public UpdateBankAccountStatusCommandHandler(RewardsDBContext rewardsDBContext)
        {
            this.rewardsDBContext = rewardsDBContext;
        }

        public async Task<ApiResponseViewModel> Handle(UpdateBankAccountStatusCommand request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();

            var bankAccount = await rewardsDBContext.BankAccounts.FirstAsync(x => x.Id == request.Id);
            if (bankAccount != null)
            {
                if (request.StatusTypeId == 4)
                {
                    if (!String.IsNullOrEmpty(bankAccount.PendingChanges))
                    {
                        var item = JsonConvert.DeserializeObject<BankAccounts>(bankAccount.PendingChanges);
                        var bankItem = await rewardsDBContext.Banks.Where(x => x.Id == item.BankId).FirstOrDefaultAsync();
                        bankAccount.AccountNumber = item.AccountNumber;                       
                        bankAccount.BankId = item.BankId;
                        // bankAccountModel.DocumentUrl = item.DocumentUrl;                      
                        bankAccount.Name = item.Name;                       
                    }
                }
                bankAccount.StatusTypeId = request.StatusTypeId;
                bankAccount.Remarks = request.Remarks;
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
