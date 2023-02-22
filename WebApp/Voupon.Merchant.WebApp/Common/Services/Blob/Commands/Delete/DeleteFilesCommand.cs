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

namespace Voupon.Merchant.WebApp.Common.Services.Blob.Commands.Delete
{
    public class DeleteFilesCommand : IRequest<ApiResponseViewModel>
    {
        public int Id { get; set; }
        public string[] Files { get; set; }
        public string FilePath { get; set; }
        public string ContainerName { get; set; }
    }

    public class DeleteFilesCommandHandler : IRequestHandler<DeleteFilesCommand, ApiResponseViewModel>
    {
        private readonly IAzureBlobStorage azureBlobStorage;

        public DeleteFilesCommandHandler(IAzureBlobStorage azureBlobStorage)
        {
            this.azureBlobStorage = azureBlobStorage;
        }

        public async Task<ApiResponseViewModel> Handle(DeleteFilesCommand request, CancellationToken cancellationToken)
        {
            var apiResponseViewModel = new ApiResponseViewModel();
            List <string> FilesWithOtherSiezesImages = new List<string>();
            foreach(var file in request.Files)
            {
                if (file.Contains("big"))
                {
                    FilesWithOtherSiezesImages.Add(file.Replace("big", "normal"));
                    FilesWithOtherSiezesImages.Add(file.Replace("big", "small"));
                    FilesWithOtherSiezesImages.Add(file.Replace("big", "org"));
                }
                else if (file.Contains("normal"))
                {
                    FilesWithOtherSiezesImages.Add(file.Replace("normal", "big"));
                    FilesWithOtherSiezesImages.Add(file.Replace("normal", "small"));
                    FilesWithOtherSiezesImages.Add(file.Replace("normal", "org"));
                }
                else if (file.Contains("small"))
                {
                    FilesWithOtherSiezesImages.Add(file.Replace("small", "big"));
                    FilesWithOtherSiezesImages.Add(file.Replace("small", "normal"));
                    FilesWithOtherSiezesImages.Add(file.Replace("small", "org"));
                }
                FilesWithOtherSiezesImages.Add(file);
            }
            azureBlobStorage.DeleteFiles(request.ContainerName, request.Id + "/"+request.FilePath, FilesWithOtherSiezesImages.ToArray());     
            apiResponseViewModel.Message = "Files Deleted";
            apiResponseViewModel.Successful = true;
            return apiResponseViewModel;
        }
    }
}
