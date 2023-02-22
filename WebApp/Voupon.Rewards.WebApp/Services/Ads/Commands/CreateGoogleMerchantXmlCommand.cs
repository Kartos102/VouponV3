using MediatR;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Common.Azure.Blob;
using Voupon.Rewards.WebApp.ViewModels;

namespace Voupon.Rewards.WebApp.Services.Ads.Commands
{
    public class CreateGoogleMerchantXmlCommand : IRequest<ApiResponseViewModel>
    {
        public int Id { get; set; }
        public MemoryStream ms { get; set; }
        public string Filename { get; set; }
        public string ContentType { get; set; }
        public string FilePath { get; set; }
        public string ContainerName { get; set; }

        public bool? IsPublic { get; set; }
    }

    public class CreateGoogleMerchantXmlCommandHandler : IRequestHandler<CreateGoogleMerchantXmlCommand, ApiResponseViewModel>
    {
        private readonly IAzureBlobStorage azureBlobStorage;

        public CreateGoogleMerchantXmlCommandHandler(IAzureBlobStorage azureBlobStorage)
        {
            this.azureBlobStorage = azureBlobStorage;
        }

        public async Task<ApiResponseViewModel> Handle(CreateGoogleMerchantXmlCommand request, CancellationToken cancellationToken)
        {
            var apiResponseViewModel = new ApiResponseViewModel();
            var blob = await azureBlobStorage.UploadBlobAsync(request.ms, request.Filename, "application/xml",
                 "google-merchant", (request.IsPublic.HasValue ? request.IsPublic.Value : true));
            if (blob == null)
            {
                apiResponseViewModel.Message = "Fail to process request. Please try again later";
                apiResponseViewModel.Successful = false;
                return apiResponseViewModel;
            }
            apiResponseViewModel.Successful = true;
            apiResponseViewModel.Data = blob.StorageUri.PrimaryUri.ToString().Replace("http://", "https://");
            apiResponseViewModel.Message = "File Uploaded";
            apiResponseViewModel.Successful = true;
            return apiResponseViewModel;
        }
    }
}
