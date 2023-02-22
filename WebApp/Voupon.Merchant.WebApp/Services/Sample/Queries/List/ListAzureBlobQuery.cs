using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Common.Azure.Blob;
using Voupon.Common.BaseTypes;

namespace Voupon.Merchant.WebApp.Services.Sample.Queries.List
{
    public class ListAzureBlobQuery : ListQueryRequest<ListView<string>>
    {

        public class ListAzureBlobQueryHandler : IRequestHandler<ListAzureBlobQuery, ListView<string>>
        {
            public IAzureBlobStorage azureBlobStorage;
            public ListAzureBlobQueryHandler(IAzureBlobStorage azureBlobStorage)
            {
                this.azureBlobStorage = azureBlobStorage;
            }

            public async Task<ListView<string>> Handle(ListAzureBlobQuery request, CancellationToken cancellationToken)
            {
                var blobItems = await azureBlobStorage.ListBlobsAsync("merchants");

                var result = new ListView<string>(blobItems.Count(), 1, 0)
                {
                    Items = blobItems.Select(x => x.StorageUri.PrimaryUri.ToString()).ToList()
                };
                return result;
            }
        }


    }
}
