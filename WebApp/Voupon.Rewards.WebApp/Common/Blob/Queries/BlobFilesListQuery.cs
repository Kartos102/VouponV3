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
    public class BlobFilesListQuery : IRequest<ApiResponseViewModel>
    {
        public int Id { get; set; }
        public string FilePath { get; set; }
        public string ContainerName { get; set; }
    }

    public class BlobFilesListQueryHandler : IRequestHandler<BlobFilesListQuery, ApiResponseViewModel>
    {
        private readonly IAzureBlobStorage azureBlobStorage;

        public BlobFilesListQueryHandler(IAzureBlobStorage azureBlobStorage)
        {

            this.azureBlobStorage = azureBlobStorage;
        }

        public async Task<ApiResponseViewModel> Handle(BlobFilesListQuery request, CancellationToken cancellationToken)
        {
            var apiResponseViewModel = new ApiResponseViewModel();
            var filename = await azureBlobStorage.ListBlobsAsync(request.ContainerName, request.Id + "/" + request.FilePath);
            var fileList = new List<string>();
            foreach (var file in filename)
            {
                fileList.Add(file.StorageUri.PrimaryUri.OriginalString.Replace("http://", "https://"));
            }
            apiResponseViewModel.Data = fileList;
            apiResponseViewModel.Message = "Get File List Successfully";
            apiResponseViewModel.Successful = true;
            return apiResponseViewModel;
        }
    }
}
