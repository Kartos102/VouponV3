using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Common.Azure.Blob;
using Voupon.Rewards.WebApp.ViewModels;

namespace Voupon.Rewards.WebApp.Common.Blob.Queries
{
    public class SASTokenQuery : IRequest<ApiResponseViewModel>
    {
        public class SASTokenQueryHandler : IRequestHandler<SASTokenQuery, ApiResponseViewModel>
        {
            private readonly IOptions<AppSettings> appSettings;

            public SASTokenQueryHandler(IOptions<AppSettings> appSettings)
            {
                this.appSettings = appSettings;
            }

            public async Task<ApiResponseViewModel> Handle(SASTokenQuery request, CancellationToken cancellationToken)
            {
                var apiResponseViewModel = new ApiResponseViewModel();
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

                apiResponseViewModel.Data = storageAccount.GetSharedAccessSignature(policy);
                apiResponseViewModel.Successful = true;
                return apiResponseViewModel;
            }
        }
    }    
}
