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
    public class BankAccountPendingChangesQuery : IRequest<ApiResponseViewModel>
    {
        public int Id { get; set; }
    }

    public class BankAccountPendingChangesQueryHandler : IRequestHandler<BankAccountPendingChangesQuery, ApiResponseViewModel>
    {
        RewardsDBContext rewardsDBContext;
        public BankAccountPendingChangesQueryHandler(RewardsDBContext rewardsDBContext)
        {
            this.rewardsDBContext = rewardsDBContext;
        }

        public async Task<ApiResponseViewModel> Handle(BankAccountPendingChangesQuery request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();
            try
            {
                BankAccountModel bankAccountModel = new BankAccountModel();            

                var bank = await rewardsDBContext.BankAccounts.AsNoTracking().FirstOrDefaultAsync(x => x.Id == request.Id);
                if (bank != null)
                {
                    string json = "";
                    if (String.IsNullOrEmpty(bank.PendingChanges))
                    {
                        json = JsonConvert.SerializeObject(bank);
                        var temp = await rewardsDBContext.BankAccounts.FirstOrDefaultAsync(x => x.Id == request.Id);
                        temp.PendingChanges = json;
                        rewardsDBContext.SaveChanges();
                    }
                    else
                    {
                        json = bank.PendingChanges;
                    }
                    var item = JsonConvert.DeserializeObject<Voupon.Database.Postgres.RewardsEntities.BankAccounts>(json);                  
                    bankAccountModel.AccountNumber = item.AccountNumber;
                    bankAccountModel.Bank = (item.BankId !=null )?rewardsDBContext.Banks.First(x => x.Id == item.BankId).Name:"";
                    bankAccountModel.BankId = item.BankId;
                    bankAccountModel.CreatedAt = item.CreatedAt;
                    bankAccountModel.CreatedByUserId = item.CreatedByUserId;
                    bankAccountModel.DocumentUrl = item.DocumentUrl;
                    bankAccountModel.Id = item.Id;
                    bankAccountModel.LastUpdatedAt = item.LastUpdatedAt;
                    bankAccountModel.LastUpdatedByUserId = item.LastUpdatedByUserId;
                    bankAccountModel.MerchantId = item.MerchantId;
                    bankAccountModel.Name = item.Name;
                    bankAccountModel.Status = rewardsDBContext.StatusTypes.First(x => x.Id == item.StatusTypeId).Name;
                    bankAccountModel.StatusTypeId = item.StatusTypeId;
                    bankAccountModel.Remarks = item.Remarks;
                    response.Successful = true;
                    response.Message = "Get Bank Account Successfully";
                    response.Data = bankAccountModel;

                }
                else
                {
                    response.Successful = false;
                    response.Message = "Bank Account not found";
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
