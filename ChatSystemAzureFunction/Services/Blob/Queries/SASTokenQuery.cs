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
using ChatSystemAzureFunction.ViewModels;

namespace ChatSystemAzureFunction.Services.Blob.Queries
{
    public class SASTokenQuery : IRequest<ApiResponseViewModel>
    {
        public class SASTokenQueryHandler : IRequestHandler<SASTokenQuery, ApiResponseViewModel>
        {

            public SASTokenQueryHandler()
            {
            }

            public async Task<ApiResponseViewModel> Handle(SASTokenQuery request, CancellationToken cancellationToken)
            {
                var apiResponseViewModel = new ApiResponseViewModel();
                var storageAccountName = Environment.GetEnvironmentVariable("AzureConfigurationsStorageAccount");
                var accessKey = Environment.GetEnvironmentVariable("AzureConfigurationsStorageKey");

                var connectionString = String.Format("DefaultEndpointsProtocol=https;AccountName={0};AccountKey={1}",
                    storageAccountName,
                    accessKey);
                var storageAccount = CloudStorageAccount.Parse(connectionString);

                var policy = new SharedAccessAccountPolicy()
                {
                    Permissions = SharedAccessAccountPermissions.Read,
                    Services = SharedAccessAccountServices.Blob,
                    ResourceTypes = SharedAccessAccountResourceTypes.Container | SharedAccessAccountResourceTypes.Object,
                    SharedAccessExpiryTime = DateTime.UtcNow.AddMinutes(30),
                    Protocols = SharedAccessProtocol.HttpsOnly,
                };

                apiResponseViewModel.Data = storageAccount.GetSharedAccessSignature(policy);
                apiResponseViewModel.Successful = true;
                return apiResponseViewModel;
            }
        }
    }    
}
