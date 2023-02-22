using MediatR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Merchant.WebApp.Common.Services.BankAccounts.Models;
using Voupon.Merchant.WebApp.ViewModels;

namespace Voupon.Merchant.WebApp.Common.Services.BankAccounts.Queries
{
    public class BankAccountQuery : IRequest<ApiResponseViewModel>
    {
        public int BankAccountId { get; set; }
    }

    public class BankAccountQueryHandler : IRequestHandler<BankAccountQuery, ApiResponseViewModel>
    {
        RewardsDBContext rewardsDBContext;
        public BankAccountQueryHandler(RewardsDBContext rewardsDBContext)
        {
            this.rewardsDBContext = rewardsDBContext;
        }

        public async Task<ApiResponseViewModel> Handle(BankAccountQuery request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();
            try
            {
                BankAccountModel bankAccountModel = new BankAccountModel();
                var bankAccount = await rewardsDBContext.BankAccounts.Include(x => x.Bank).Include(x => x.StatusType).FirstOrDefaultAsync(x => x.Id == request.BankAccountId);               
                if (bankAccount != null)
                {
                    bankAccountModel.AccountNumber = bankAccount.AccountNumber;
                    bankAccountModel.Bank = bankAccount.Bank != null ? bankAccount.Bank.Name : null;
                    bankAccountModel.BankId = bankAccount.BankId;
                    bankAccountModel.CreatedAt = bankAccount.CreatedAt;
                    bankAccountModel.CreatedByUserId = bankAccount.CreatedByUserId;
                    bankAccountModel.DocumentUrl = bankAccount.DocumentUrl;
                    bankAccountModel.Id = bankAccount.Id;
                    bankAccountModel.LastUpdatedAt = bankAccount.LastUpdatedAt;
                    bankAccountModel.LastUpdatedByUserId = bankAccount.LastUpdatedByUserId;
                    bankAccountModel.MerchantId = bankAccount.MerchantId;
                    bankAccountModel.Name = bankAccount.Name;
                    bankAccountModel.Status = bankAccount.StatusType.Name;
                    bankAccountModel.StatusTypeId = bankAccount.StatusTypeId;
                    bankAccountModel.Remarks = bankAccount.Remarks;
                    response.Successful = true;
                    response.Message = "Get Bank Account Successfully";
                    response.Data = bankAccountModel;
                }
                else
                {
                    response.Successful = false;
                    response.Message ="Bank Account not found";
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
