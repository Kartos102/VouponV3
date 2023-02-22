using MediatR;
using Microsoft.AspNetCore.Http;
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
    public class CreateStatementFileCommand : IRequest<ApiResponseViewModel>
    {
        public int MerchantId { get; set; }
        public string FileName { get; set; }
        public byte[] Data { get; set; }    
        public string ContainerName { get; set; }
    }

    public class CreateStatementFileCommandHandler : IRequestHandler<CreateStatementFileCommand, ApiResponseViewModel>
    {
        private readonly IAzureBlobStorage azureBlobStorage;       

        public CreateStatementFileCommandHandler(IAzureBlobStorage azureBlobStorage)
        {
           
            this.azureBlobStorage = azureBlobStorage;
        }

        public async Task<ApiResponseViewModel> Handle(CreateStatementFileCommand request, CancellationToken cancellationToken)
        {
            var apiResponseViewModel = new ApiResponseViewModel();
            byte[] fileData;
            using (MemoryStream ms = new MemoryStream(request.Data))
            {              
                fileData = ms.ToArray();
                var contentType = "application/pdf";              
                var filename = request.FileName + ".pdf";
                ms.Position = 0;
                var blob = await azureBlobStorage.UploadBlobAsync(ms, request.MerchantId + "/FinanceStatements/" + filename, contentType,
                  request.ContainerName, true);
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
    }
}
