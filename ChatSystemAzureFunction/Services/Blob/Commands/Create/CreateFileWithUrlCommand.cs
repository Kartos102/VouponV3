using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Storage;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Common.Azure.Blob;
using ChatSystemAzureFunction.ViewModels;


namespace ChatSystemAzureFunction.Services.Blob.Commands.Create
{
    public class CreateFileWithUrlCommand : IRequest<ApiResponseViewModel>
    {
        public int Id { get; set; }
        public string Url { get; set; }
        public string FilePath { get; set; }
        public string ContainerName { get; set; }

        public string SASToken { get; set; }
    }

    public class CreateFileWithUrlCommandHandler : IRequestHandler<CreateFileWithUrlCommand, ApiResponseViewModel>
    {
        private readonly IAzureBlobStorage azureBlobStorage;

        public CreateFileWithUrlCommandHandler(IAzureBlobStorage azureBlobStorage)
        {
            this.azureBlobStorage = azureBlobStorage;
        }

        public async Task<ApiResponseViewModel> Handle(CreateFileWithUrlCommand request, CancellationToken cancellationToken)
        {
            System.Net.WebClient wc = new System.Net.WebClient();
            var apiResponseViewModel = new ApiResponseViewModel();
            byte[] fileData;
            try
            {
                using (MemoryStream ms = new MemoryStream(wc.DownloadData(request.Url + (string.IsNullOrEmpty(request.SASToken) ? "" : request.SASToken))))
                {
                    fileData = ms.ToArray();
                    var contentType = request.Url.Split(".").Last();

                    var filename = request.FilePath + "/" + request.Url.Split("/").Last(); //+ Guid.NewGuid().ToString() + "." + contentType;
                    ms.Position = 0;
                    var blob = await azureBlobStorage.UploadBlobAsync(ms, request.Id + "/" + filename, (contentType == "pdf" ? "application/pdf" : contentType),
                      request.ContainerName, (string.IsNullOrEmpty(request.SASToken)));
                    if (blob == null)
                    {
                        apiResponseViewModel.Message = "Fail to process request. Please try again later";
                        apiResponseViewModel.Successful = false;
                        return apiResponseViewModel;
                    }
                    apiResponseViewModel.Data = blob.StorageUri.PrimaryUri.ToString().Replace("http://", "https://");
                }

                apiResponseViewModel.Message = "File Uploaded";
                apiResponseViewModel.Successful = true;
                return apiResponseViewModel;
            }
            catch (Exception ex)
            {
                apiResponseViewModel.Message = ex.Message;
                apiResponseViewModel.Successful = false;
                return apiResponseViewModel;
            }
        }
    }
}
