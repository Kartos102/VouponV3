using MediatR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Merchant.WebApp.Common.Services.PersonInCharges.Models;
using Voupon.Merchant.WebApp.ViewModels;
using Microsoft.Extensions.Options;
using Microsoft.Azure.Storage;

namespace Voupon.Merchant.WebApp.Common.Services.PersonInCharges.Queries
{
    public class MerchantPersonInChargePendingChangesQuery : IRequest<ApiResponseViewModel>
    {
        public int MerchantId { get; set; }
    }

    public class MerchantPersonInChargePendingChangesQueryHandler : IRequestHandler<MerchantPersonInChargePendingChangesQuery, ApiResponseViewModel>
    {
        RewardsDBContext rewardsDBContext;
        private readonly IOptions<AppSettings> appSettings;
        public MerchantPersonInChargePendingChangesQueryHandler(RewardsDBContext rewardsDBContext, IOptions<AppSettings> appSettings)
        {
            this.rewardsDBContext = rewardsDBContext;
            this.appSettings = appSettings;
        }

        public async Task<ApiResponseViewModel> Handle(MerchantPersonInChargePendingChangesQuery request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();
            try
            {
                PersonInChargeModel personInChargeModel = new PersonInChargeModel();
                var personInCharge = await rewardsDBContext.PersonInCharges.AsNoTracking().FirstOrDefaultAsync(x => x.MerchantId == request.MerchantId);
                if (personInCharge != null)
                {
                    string json = "";
                    if (String.IsNullOrEmpty(personInCharge.PendingChanges))
                    {
                        json = JsonConvert.SerializeObject(personInCharge);
                        var temp = await rewardsDBContext.PersonInCharges.FirstOrDefaultAsync(x => x.MerchantId == request.MerchantId);
                        temp.PendingChanges = json;
                        rewardsDBContext.SaveChanges();
                    }
                    else
                    {
                        json = personInCharge.PendingChanges;
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

                    var item = JsonConvert.DeserializeObject<Voupon.Database.Postgres.RewardsEntities.PersonInCharges>(json);
                    personInChargeModel.Contact = item.Contact;
                    personInChargeModel.DocumentUrl = (item.DocumentUrl != null ? item.DocumentUrl.Replace("http://", "https://") + sasToken : item.DocumentUrl);
                    personInChargeModel.CreatedAt = item.CreatedAt;
                    personInChargeModel.CreatedByUserId = item.CreatedByUserId;
                    personInChargeModel.Id = item.Id;
                    personInChargeModel.IdentityNumber = item.IdentityNumber;
                    personInChargeModel.LastUpdatedAt = item.LastUpdatedAt;
                    personInChargeModel.LastUpdatedByUser = item.LastUpdatedByUser;
                    personInChargeModel.MerchantId = item.MerchantId;
                    personInChargeModel.Name = item.Name;
                    personInChargeModel.Position = item.Position;
                    personInChargeModel.StatusTypeId = item.StatusTypeId;
                    personInChargeModel.Status = rewardsDBContext.StatusTypes.First(x => x.Id == item.StatusTypeId).Name;
                    personInChargeModel.Remarks = item.Remarks;
                    response.Successful = true;
                    response.Message = "Get Person In Charge Successfully";
                    response.Data = personInChargeModel;
                }
                else
                {
                    response.Successful = false;
                    response.Message = "Person In Charge not found";
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
