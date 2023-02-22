using MediatR;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Common.Azure.Blob;
using Voupon.Merchant.WebApp.ViewModels;

namespace Voupon.Merchant.WebApp.Common.Services.Blob.Queries
{
    public class BlobBigImagesListQuery : IRequest<ApiResponseViewModel>
    {
        private bool getIListBlobItem = false;
        public int Id { get; set; }
        public string FilePath { get; set; }
        public string ContainerName { get; set; }
        public bool GetIListBlobItem
        {
            get { return getIListBlobItem; }
            set { getIListBlobItem = value; }
        }
    }

    public class BlobBigImagesListQueryHandler : IRequestHandler<BlobBigImagesListQuery, ApiResponseViewModel>
    {
        private readonly IAzureBlobStorage azureBlobStorage;

        public BlobBigImagesListQueryHandler(IAzureBlobStorage azureBlobStorage)
        {

            this.azureBlobStorage = azureBlobStorage;
        }

        public async Task<ApiResponseViewModel> Handle(BlobBigImagesListQuery request, CancellationToken cancellationToken)
        {
            var apiResponseViewModel = new ApiResponseViewModel();
            var filename = await azureBlobStorage.ListBlobsAsync(request.ContainerName, request.Id + "/" + request.FilePath);
            if (!request.GetIListBlobItem)
            {
                var fileList = new List<string>();
                foreach (var file in filename)
                {
                    if (file.StorageUri.PrimaryUri.OriginalString.Contains("big"))
                    {
                        fileList.Add(file.StorageUri.PrimaryUri.ToString().Replace("http://","https://").Replace(":80",""));
                    }

                }
                apiResponseViewModel.Data = fileList;
            }
            else
            {
                apiResponseViewModel.Data = filename;
            }

            apiResponseViewModel.Message = "Get File List Successfully";
            apiResponseViewModel.Successful = true;
            return apiResponseViewModel;
        }
    }
}
