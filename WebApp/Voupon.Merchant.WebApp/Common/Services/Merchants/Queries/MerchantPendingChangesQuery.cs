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
using Voupon.Merchant.WebApp.Common.Services.Merchants.Models;
using Voupon.Merchant.WebApp.ViewModels;

namespace Voupon.Merchant.WebApp.Common.Services.Merchants.Queries
{
    public class MerchantPendingChangesQuery : IRequest<ApiResponseViewModel>
    {
        public int MerchantId { get; set; }
    }

    public class MerchantPendingChangesQueryHandler : IRequestHandler<MerchantPendingChangesQuery, ApiResponseViewModel>
    {
        RewardsDBContext rewardsDBContext;
        IOptions<AppSettings> appSettings;
        public MerchantPendingChangesQueryHandler(RewardsDBContext rewardsDBContext, IOptions<AppSettings> appSettings)
        {
            this.rewardsDBContext = rewardsDBContext;
            this.appSettings = appSettings;
        }

        public async Task<ApiResponseViewModel> Handle(MerchantPendingChangesQuery request, CancellationToken cancellationToken)
        {

            ApiResponseViewModel response = new ApiResponseViewModel();
            try
            {
                //var merchant= rewardsDBContext.Merchants.FirstOrDefault(x => x.Id == request.MerchantId);
                var merchant = rewardsDBContext.Merchants.AsNoTracking().FirstOrDefault(x => x.Id == request.MerchantId);
                if (merchant != null)
                {
                    string json = "";
                    if (String.IsNullOrEmpty(merchant.PendingChanges))
                    {

                        json = JsonConvert.SerializeObject(merchant);
                        var temp = rewardsDBContext.Merchants.FirstOrDefault(x => x.Id == request.MerchantId);
                        temp.PendingChanges = json;
                        rewardsDBContext.SaveChanges();
                    }
                    else
                    {
                        json = merchant.PendingChanges;
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

                    var item = JsonConvert.DeserializeObject<Voupon.Database.Postgres.RewardsEntities.Merchants>(json);
                    MerchantModel merchantModel = new MerchantModel();
                    merchantModel.Id = item.Id;
                    merchantModel.Code = item.Code;
                    merchantModel.CountryId = item.CountryId;
                    merchantModel.CountryName = item.CountryId!=null && item.CountryId != 0 ? rewardsDBContext.Countries.First(x => x.Id == item.CountryId).Name:"";
                    merchantModel.ProvinceId = item.ProvinceId;
                    merchantModel.ProvinceName = item.ProvinceId != null && item.ProvinceId != 0 ?  rewardsDBContext.Provinces.First(x => x.Id == item.ProvinceId).Name:"";
                    merchantModel.DistritId = item.DistritId;
                    merchantModel.DistritName = item.DistritId != null && item.DistritId != 0 ? rewardsDBContext.Districts.First(x => x.Id == item.DistritId).Name:"";
                    merchantModel.PostCodeId = item.PostCodeId;
                    merchantModel.PostCodeName = item.PostCodeId != null && item.PostCodeId != 0 ? rewardsDBContext.PostCodes.First(x => x.Id == item.PostCodeId).Name:"" ;
                    merchantModel.Address_1 = item.Address_1;
                    merchantModel.Address_2 = item.Address_2;
                    merchantModel.RegistrationNumber = item.RegistrationNumber;
                    merchantModel.BIDDocumentUrl = (item.BIDDocumentUrl != null ? item.BIDDocumentUrl.Replace("http://", "https://") + sasToken : item.BIDDocumentUrl) ;
                    merchantModel.BusinessTypeId = item.BusinessTypeId;
                    merchantModel.BusinessType = item.BusinessTypeId != null && item.BusinessTypeId != 0 ? rewardsDBContext.BusinessTypes.First(x => x.Id == item.BusinessTypeId).Name:"";
                    merchantModel.CompanyContact = item.CompanyContact;
                    merchantModel.CompanyName = item.CompanyName;
                    merchantModel.Description = item.Description;
                    merchantModel.DisplayName = item.DisplayName;
                    merchantModel.LogoUrl = item.LogoUrl;
                    merchantModel.WebsiteSiteUrl = item.WebsiteSiteUrl;
                    merchantModel.CreatedAt = item.CreatedAt;
                    merchantModel.CreatedByUserId = item.CreatedByUserId;
                    merchantModel.LastUpdatedAt = item.LastUpdatedAt;
                    merchantModel.LastUpdatedByUserId = item.LastUpdatedByUserId;
                    merchantModel.StatusTypeId = item.StatusTypeId;
                    merchantModel.Status = rewardsDBContext.StatusTypes.First(x => x.Id == item.StatusTypeId).Name;
                    merchantModel.Remarks = item.Remarks;
                    merchantModel.IsPublished = item.IsPublished;
                    merchantModel.DefaultCommission = item.DefaultCommission;
                    response.Successful = true;
                    response.Message = "Get Merchant Successfully";
                    response.Data = merchantModel;
                }
                else
                {
                    response.Successful = false;
                    response.Message = "Merchant not found";
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
