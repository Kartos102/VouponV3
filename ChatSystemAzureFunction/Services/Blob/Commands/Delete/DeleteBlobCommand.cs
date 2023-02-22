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

namespace ChatSystemAzureFunctionServices.Blob.Commands.Delete
{
    public class DeleteBlobCommand : IRequest<ApiResponseViewModel>
    {
        public int Id { get; set; }
        public string FilePath { get; set; }
        public string ContainerName { get; set; }
    }

    public class DeleteBlobCommandHandler : IRequestHandler<DeleteBlobCommand, ApiResponseViewModel>
    {
        private readonly IAzureBlobStorage azureBlobStorage;

        public DeleteBlobCommandHandler(IAzureBlobStorage azureBlobStorage)
        {
            this.azureBlobStorage = azureBlobStorage;
        }

        public async Task<ApiResponseViewModel> Handle(DeleteBlobCommand request, CancellationToken cancellationToken)
        {
            var apiResponseViewModel = new ApiResponseViewModel();
            await azureBlobStorage.DeleteAsync(request.ContainerName, request.Id + "/" + request.FilePath);
            apiResponseViewModel.Message = "Files Deleted";
            apiResponseViewModel.Successful = true;
            return apiResponseViewModel;
        }
    }
}
