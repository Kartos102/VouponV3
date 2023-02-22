using MediatR;
using Microsoft.AspNetCore.Http;
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
    public class BlobSmallImagesListQuery : IRequest<ApiResponseViewModel>
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

    public class BlobSmallImagesListQueryHandler : IRequestHandler<BlobSmallImagesListQuery, ApiResponseViewModel>
    {
        private readonly IAzureBlobStorage azureBlobStorage;

        public BlobSmallImagesListQueryHandler(IAzureBlobStorage azureBlobStorage)
        {

            this.azureBlobStorage = azureBlobStorage;
        }

        public async Task<ApiResponseViewModel> Handle(BlobSmallImagesListQuery request, CancellationToken cancellationToken)
        {
            var apiResponseViewModel = new ApiResponseViewModel();
            var filename = await azureBlobStorage.ListBlobsAsync(request.ContainerName, request.Id + "/" + request.FilePath);
            if (!request.GetIListBlobItem)
            {
                var fileList = new List<string>();
                foreach (var file in filename)
                {
                    if (file.StorageUri.PrimaryUri.OriginalString.Contains("small"))
                    {
                        fileList.Add(file.StorageUri.PrimaryUri.OriginalString.Replace("http://", "https://"));
                    }
                    else if (!file.StorageUri.PrimaryUri.OriginalString.Contains("big") && !file.StorageUri.PrimaryUri.OriginalString.Contains("normal") && !file.StorageUri.PrimaryUri.OriginalString.Contains("org")) {
                        fileList.Add(file.StorageUri.PrimaryUri.OriginalString.Replace("http://", "https://"));
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
