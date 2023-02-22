using MediatR;
using Microsoft.Azure.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
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
    public class MerchantBankAccountPendingChangesQuery : IRequest<ApiResponseViewModel>
    {
        public int MerchantId { get; set; }
    }

    public class MerchantBankAccountPendingChangesQueryHandler : IRequestHandler<MerchantBankAccountPendingChangesQuery, ApiResponseViewModel>
    {
        RewardsDBContext rewardsDBContext;
        IOptions<AppSettings> appSettings;
        public MerchantBankAccountPendingChangesQueryHandler(RewardsDBContext rewardsDBContext, IOptions<AppSettings> appSettings)
        {
            this.rewardsDBContext = rewardsDBContext;
            this.appSettings = appSettings;
        }

        public async Task<ApiResponseViewModel> Handle(MerchantBankAccountPendingChangesQuery request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();
            try
            {
                BankAccountModel bankAccountModel = new BankAccountModel();

                var bank = await rewardsDBContext.BankAccounts.AsNoTracking().FirstOrDefaultAsync(x => x.MerchantId == request.MerchantId);
                if (bank != null)
                {
                    string json = "";
                    if (String.IsNullOrEmpty(bank.PendingChanges))
                    {
                        json = JsonConvert.SerializeObject(bank);
                        var temp = await rewardsDBContext.BankAccounts.FirstOrDefaultAsync(x => x.MerchantId == request.MerchantId);
                        temp.PendingChanges = json;
                        rewardsDBContext.SaveChanges();
                    }
                    else
                    {
                        json = bank.PendingChanges;
                    }

                    var storageAccountName = appSettings.Value.AzureConfigurations.StorageAccount;
                    var accessKey = appSettings.Value.AzureConfigurations.StorageKey;
                    var connectionString = String.Format("DefaultEndpointsProtocol=https;AccountName={0};AccountKey={1}",
                        storageAccountName,
                        accessKey);
                    var storageAccount = CloudStorageAccount.Parse(connectionString);

                    var policy = new SharedAccessAccountPolicy()
                    {
                        Permissions = SharedAccessAccountPermissions.Read,
                        Services = SharedAccessAccountServices.Blob,
                        ResourceTypes = SharedAccessAccountResourceTypes.Container | SharedAccessAccountResourceTypes.Object,
                        SharedAccessExpiryTime = DateTime.Now.AddMinutes(30),
                        Protocols = SharedAccessProtocol.HttpsOnly,
                    };
                    var sasToken = storageAccount.GetSharedAccessSignature(policy);

                    var item = JsonConvert.DeserializeObject<Voupon.Database.Postgres.RewardsEntities.BankAccounts>(json);
                    bankAccountModel.AccountNumber = item.AccountNumber;
                    bankAccountModel.Bank = (item.BankId != null) && item.BankId != 0 ? rewardsDBContext.Banks.First(x => x.Id == item.BankId).Name : "";
                    bankAccountModel.BankId = item.BankId;
                    bankAccountModel.CreatedAt = item.CreatedAt;
                    bankAccountModel.CreatedByUserId = item.CreatedByUserId;
                    bankAccountModel.DocumentUrl = (item.DocumentUrl != null ? item.DocumentUrl.Replace("http://", "https://") + sasToken : item.DocumentUrl);
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
