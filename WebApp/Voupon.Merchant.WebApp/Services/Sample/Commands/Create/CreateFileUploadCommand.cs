using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Common.Azure.Blob;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Merchant.WebApp.ViewModels;

namespace Voupon.Merchant.WebApp.Services.Sample.Create.Commands
{
    public class CreateFileUploadCommand : IRequest<ApiResponseViewModel>
    {
        public string Id { get; set; }
        public IFormFile File { get; set; }
    }

    public class CreateFileUploadCommandHandler : IRequestHandler<CreateFileUploadCommand, ApiResponseViewModel>
    {
        private readonly IAzureBlobStorage azureBlobStorage;

        public CreateFileUploadCommandHandler(IAzureBlobStorage azureBlobStorage)
        {
            this.azureBlobStorage = azureBlobStorage;
        }

        public async Task<ApiResponseViewModel> Handle(CreateFileUploadCommand request, CancellationToken cancellationToken)
        {
            var apiResponseViewModel = new ApiResponseViewModel();
            byte[] fileData;
            using (MemoryStream ms = new MemoryStream())
            {
                request.File.CopyTo(ms);
                fileData = ms.ToArray();
                var contentType = request.File.ContentType.Split('/')[1];
                var filename = Guid.NewGuid() + "." + contentType;
                ms.Position = 0;              
                 var blob = await azureBlobStorage.UploadBlobAsync(ms, filename, contentType, ContainerNameEnum.TEST, true);
                if (blob == null)
                {
                    apiResponseViewModel.Message = "Fail to process request. Please try again later";
                    apiResponseViewModel.Successful = false;
                    return apiResponseViewModel;
                }
            }


            return apiResponseViewModel;
        }
    }
}
