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
    public class CreateFilesCommand : IRequest<ApiResponseViewModel>
    {
        public int Id { get; set; }
        public IFormFileCollection Files { get; set; }
        public string FilePath { get; set; }
        public string ContainerName { get; set; }
    }

    public class CreateFilesCommandHandler : IRequestHandler<CreateFilesCommand, ApiResponseViewModel>
    {
        private readonly IAzureBlobStorage azureBlobStorage;

        public CreateFilesCommandHandler(IAzureBlobStorage azureBlobStorage)
        {
            this.azureBlobStorage = azureBlobStorage;
        }

        public async Task<ApiResponseViewModel> Handle(CreateFilesCommand request, CancellationToken cancellationToken)
        {
            var apiResponseViewModel = new ApiResponseViewModel();
            string folderUrl = "";
            var existingFilename = await azureBlobStorage.ListBlobsAsync(request.ContainerName, request.Id + "/" + request.FilePath);
            int i = 1;
            foreach (IFormFile file in request.Files)
            {
                if (file.Length == 0)
                {
                    var temp = existingFilename.FirstOrDefault(x => x.StorageUri.PrimaryUri.ToString() == file.FileName);
                    if (temp != null)
                    {
                        var blobName = file.FileName.Replace(temp.Container.Uri.ToString() + "/", "");
                        var contentType = GetContentType(file.FileName).Split('/')[1];
                        var filename = i + "_" + Guid.NewGuid() + "." + contentType;
                        await azureBlobStorage.CopyAsync(request.ContainerName, blobName, request.Id + "/" +
                            request.FilePath + "/" + filename);
                        await azureBlobStorage.DeleteAsync(request.ContainerName, blobName);
                        //var memoryStream = await azureBlobStorage.DownloadAsync(request.ContainerName, blobName);
                        //using (MemoryStream ms = memoryStream)
                        //{                          
                        //    byte[] fileData;
                        //    fileData = ms.ToArray();
                        //    var contentType = GetContentType(file.FileName).Split('/')[1];
                        //    var filename = i + "_" + Guid.NewGuid() + "." + contentType;
                        //    //ms.Position = 0;
                        //    var blob = await azureBlobStorage.UploadBlobAsync(ms, request.Id + "/" +
                        //        request.FilePath + "/" + filename, contentType,
                        //      request.ContainerName, true);
                        //    if (blob == null)
                        //    {
                        //        apiResponseViewModel.Message = "Fail to process request. Please try again later";
                        //        apiResponseViewModel.Successful = false;
                        //        return apiResponseViewModel;
                        //    }
                        //    folderUrl = blob.StorageUri.PrimaryUri.OriginalString.Replace(filename, "");
                        //}
                    }

                }
                else
                    using (MemoryStream ms = new MemoryStream())
                    {

                        byte[] fileData;
                        file.CopyTo(ms);
                        fileData = ms.ToArray();
                        var contentType = file.ContentType.Split('/')[1];
                        var filename = i + "_" + Guid.NewGuid() + "." + contentType;
                        ms.Position = 0;
                        var blob = await azureBlobStorage.UploadBlobAsync(ms, request.Id + "/" +
                            request.FilePath + "/" + filename, contentType,
                          request.ContainerName, true);
                        if (blob == null)
                        {
                            apiResponseViewModel.Message = "Fail to process request. Please try again later";
                            apiResponseViewModel.Successful = false;
                            return apiResponseViewModel;
                        }
                        folderUrl = blob.StorageUri.PrimaryUri.ToString().Replace(filename, "");
                    }
                i++;
            }
            apiResponseViewModel.Data = folderUrl;
            apiResponseViewModel.Message = "Files Uploaded";
            apiResponseViewModel.Successful = true;
            return apiResponseViewModel;
        }

        private string GetContentType(string path)
        {
            var types = GetMimeTypes();
            var ext = Path.GetExtension(path).ToLowerInvariant();
            return types[ext];
        }
        private Dictionary<string, string> GetMimeTypes()
        {
            return new Dictionary<string, string>
            {
                {".txt", "text/plain"},
                {".pdf", "application/pdf"},
                {".doc", "application/vnd.ms-word"},
                {".docx", "application/vnd.ms-word"},
                {".xls", "application/vnd.ms-excel"},
                {".xlsx", "application/vnd.openxmlformats officedocument.spreadsheetml.sheet"},
                {".png", "image/png"},
                {".jpg", "image/jpeg"},
                {".jpeg", "image/jpeg"},
                {".gif", "image/gif"},
                {".csv", "text/csv"}
            };
        }
    }
}
