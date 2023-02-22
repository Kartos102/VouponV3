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

namespace Voupon.Merchant.WebApp.Common.Services.Blob.Commands.Create
{
    public class CreateFileCommand : IRequest<ApiResponseViewModel>
    {
        public int Id { get; set; }
        public IFormFile File { get; set; }
        public string FilePath { get; set; }
        public string ContainerName { get; set; }

        public bool? IsPublic { get; set; }
    }

    public class CreateFileCommandHandler : IRequestHandler<CreateFileCommand, ApiResponseViewModel>
    {
        private readonly IAzureBlobStorage azureBlobStorage;

        public CreateFileCommandHandler(IAzureBlobStorage azureBlobStorage)
        {

            this.azureBlobStorage = azureBlobStorage;
        }

        public async Task<ApiResponseViewModel> Handle(CreateFileCommand request, CancellationToken cancellationToken)
        {
            var apiResponseViewModel = new ApiResponseViewModel();
            byte[] fileData;
            using (MemoryStream ms = new MemoryStream())
            {
                request.File.CopyTo(ms);
                fileData = ms.ToArray();
                var contentType = request.File.ContentType.Split('/')[1];
                if (contentType.ToLower() == "pdf")
                {
                    contentType = "application/pdf";
                }
                var filename = Guid.NewGuid() + "." + (contentType == "application/pdf" ? "pdf" : contentType);
                ms.Position = 0;
                var blob = await azureBlobStorage.UploadBlobAsync(ms, request.Id + "/" + request.FilePath + "/" + filename, contentType,
                  request.ContainerName, (request.IsPublic.HasValue ? request.IsPublic.Value : true));
                if (blob == null)
                {
                    apiResponseViewModel.Message = "Fail to process request. Please try again later";
                    apiResponseViewModel.Successful = false;
                    return apiResponseViewModel;
                }
                apiResponseViewModel.Successful = true;
                apiResponseViewModel.Data = blob.StorageUri.PrimaryUri.ToString().Replace("http://", "https://");
            }
            apiResponseViewModel.Message = "File Uploaded";
            apiResponseViewModel.Successful = true;
            return apiResponseViewModel;
        }
    }
}
