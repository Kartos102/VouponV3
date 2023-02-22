using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Processing;
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
    public class CreateImagesCommand : IRequest<ApiResponseViewModel>
    {
        public int Id { get; set; }
        public IFormFileCollection Files { get; set; }
        public string FilePath { get; set; }
        public string ContainerName { get; set; }
    }

    public class CreateImagesCommandHandler : IRequestHandler<CreateImagesCommand, ApiResponseViewModel>
    {
        private readonly IAzureBlobStorage azureBlobStorage;
        //private readonly IOptions<AppSettings> appSettings;


        public CreateImagesCommandHandler(IAzureBlobStorage azureBlobStorage/*, IOptions<AppSettings> appSettings*/)
        {
            this.azureBlobStorage = azureBlobStorage;
            //this.appSettings = appSettings;

        }

        public async Task<ApiResponseViewModel> Handle(CreateImagesCommand request, CancellationToken cancellationToken)
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
                        var filename = "";
                        if (i < 10)
                        {
                             filename = "0" + i + "_" + Guid.NewGuid().ToString() + "." + contentType;

                        }
                        else
                        {
                             filename = i + "_" + Guid.NewGuid().ToString() + "." + contentType;

                        }
                        if (blobName.Contains("big"))
                        {
                            var name = blobName.Replace("big_", "");

                            await azureBlobStorage.CopyAsync(request.ContainerName, blobName, request.Id + "/" + request.FilePath + "/" + "big_" + filename);

                            if (await azureBlobStorage.ExistsAsync(request.ContainerName, blobName.Replace("big", "normal")))
                                await azureBlobStorage.CopyAsync(request.ContainerName, blobName.Replace("big", "normal"), request.Id + "/" + request.FilePath + "/" + "normal_" + filename);
                            if (await azureBlobStorage.ExistsAsync(request.ContainerName, blobName.Replace("big", "small")))
                                await azureBlobStorage.CopyAsync(request.ContainerName, blobName.Replace("big", "small"), request.Id + "/" + request.FilePath + "/" + "small_" + filename);
                            if (await azureBlobStorage.ExistsAsync(request.ContainerName, blobName.Replace("big", "org")))
                                await azureBlobStorage.CopyAsync(request.ContainerName, blobName.Replace("big", "org"), request.Id + "/" + request.FilePath + "/" + "org_" + filename);
                        }
                        else if (blobName.Contains("normal"))
                        {
                            var name = blobName.Replace("normal_", "");

                            await azureBlobStorage.CopyAsync(request.ContainerName, blobName, request.Id + "/" + request.FilePath + "/" + "normal_" + filename);

                            if(await azureBlobStorage.ExistsAsync(request.ContainerName, blobName.Replace("normal", "big")))
                            await azureBlobStorage.CopyAsync(request.ContainerName, blobName.Replace("normal", "big"), request.Id + "/" + request.FilePath + "/" + "big_" + filename);
                            if (await azureBlobStorage.ExistsAsync(request.ContainerName, blobName.Replace("normal", "small")))
                                await azureBlobStorage.CopyAsync(request.ContainerName, blobName.Replace("normal", "small"), request.Id + "/" + request.FilePath + "/" + "small_" + filename);
                            if (await azureBlobStorage.ExistsAsync(request.ContainerName, blobName.Replace("normal", "org")))
                                await azureBlobStorage.CopyAsync(request.ContainerName, blobName.Replace("normal", "org"), request.Id + "/" + request.FilePath + "/" + "org_" + filename);
                        }
                        else if (blobName.Contains("small"))
                        {
                            var name = blobName.Replace("small_", "");
                            await azureBlobStorage.CopyAsync(request.ContainerName, blobName, request.Id + "/" + request.FilePath + "/" + "small_" + filename);

                            if (await azureBlobStorage.ExistsAsync(request.ContainerName, blobName.Replace("small", "big")))
                                await azureBlobStorage.CopyAsync(request.ContainerName, blobName.Replace("small", "big"), request.Id + "/" + request.FilePath + "/" + "big_" + filename);
                            if (await azureBlobStorage.ExistsAsync(request.ContainerName, blobName.Replace("small", "normal")))
                                await azureBlobStorage.CopyAsync(request.ContainerName, blobName.Replace("small", "normal"), request.Id + "/" + request.FilePath + "/" + "normal_" + filename);
                            if (await azureBlobStorage.ExistsAsync(request.ContainerName, blobName.Replace("small", "org")))
                                await azureBlobStorage.CopyAsync(request.ContainerName, blobName.Replace("small", "org"), request.Id + "/" + request.FilePath + "/" + "org_" + filename);
                        }
                        else
                        {
                            await azureBlobStorage.CopyAsync(request.ContainerName, blobName, request.Id + "/" + request.FilePath + "/" + filename);

                        }

                            if (blobName.Contains("big"))
                            {
                                await azureBlobStorage.DeleteAsync(request.ContainerName, blobName.Replace("big", "normal"));
                                await azureBlobStorage.DeleteAsync(request.ContainerName, blobName.Replace("big", "small"));
                                await azureBlobStorage.DeleteAsync(request.ContainerName, blobName.Replace("big", "org"));
                            }
                            else if (blobName.Contains("normal"))
                            {
                                await azureBlobStorage.DeleteAsync(request.ContainerName, blobName.Replace("normal", "big"));
                                await azureBlobStorage.DeleteAsync(request.ContainerName, blobName.Replace("normal", "small"));
                                await azureBlobStorage.DeleteAsync(request.ContainerName, blobName.Replace("normal", "org"));
                            }
                            else if (blobName.Contains("small"))
                            {
                                await azureBlobStorage.DeleteAsync(request.ContainerName, blobName.Replace("small", "big"));
                                await azureBlobStorage.DeleteAsync(request.ContainerName, blobName.Replace("small", "normal"));
                                await azureBlobStorage.DeleteAsync(request.ContainerName, blobName.Replace("small", "org"));
                            }
                        await azureBlobStorage.DeleteAsync(request.ContainerName, blobName);



                    }

                }
                else
                {

                    int smallImageHeight = 180;
                    int normalImageHeight = 400;
                    int bigImageHeight = 600;

                    int smallImageWidth = 180;
                    int normalImageWidth = 600;
                    int bigImageWidth = 900;

                    var contentType = file.ContentType.Split('/')[1];
                    var name = "";
                    if (i < 10)
                    {
                         name = "0" + i + "_" + Guid.NewGuid().ToString() + "." + contentType;

                    }
                    else
                    {
                         name = i + "_" + Guid.NewGuid().ToString() + "." + contentType;

                    }

                    var result = AddImageWithSizeWithRatio(file, name, request.Id, request.FilePath, contentType, request.ContainerName, "small_", smallImageWidth, smallImageHeight).Result;
                    if(!result)
                    {
                        apiResponseViewModel.Message = "Fail to process request. Please try again later";
                        apiResponseViewModel.Successful = false;
                        return apiResponseViewModel;
                    }

                     result = AddImageWithSizeWithRatio(file, name, request.Id, request.FilePath, contentType, request.ContainerName, "normal_", normalImageWidth, normalImageHeight).Result;
                    if (!result)
                    {
                        apiResponseViewModel.Message = "Fail to process request. Please try again later";
                        apiResponseViewModel.Successful = false;
                        return apiResponseViewModel;
                    }

                     result = AddImageWithSizeWithRatio(file, name, request.Id, request.FilePath, contentType, request.ContainerName, "big_", bigImageWidth, bigImageHeight).Result;
                    if (!result)
                    {
                        apiResponseViewModel.Message = "Fail to process request. Please try again later";
                        apiResponseViewModel.Successful = false;
                        return apiResponseViewModel;
                    }

                    using (MemoryStream ms = new MemoryStream())
                    {
                        byte[] fileData;
                        file.CopyTo(ms);
                        fileData = ms.ToArray();
                        var contentTypeOriginalImage = file.ContentType.Split('/')[1];
                        var filename = "";
                        if (i < 10)
                        {
                            filename = "0" + i + "_" + Guid.NewGuid().ToString() + "." + contentTypeOriginalImage;

                        }
                        else
                        {
                            filename = i + "_" + Guid.NewGuid().ToString() + "." + contentTypeOriginalImage;

                        }
                        ms.Position = 0;
                        var blob = await azureBlobStorage.UploadBlobAsync(ms, request.Id + "/" +
                            request.FilePath + "/"  + "org_" + name, contentTypeOriginalImage,
                          request.ContainerName, true);
                        if (blob == null)
                        {
                            apiResponseViewModel.Message = "Fail to process request. Please try again later";
                            apiResponseViewModel.Successful = false;
                            return apiResponseViewModel;
                        }
                        folderUrl = blob.StorageUri.PrimaryUri.OriginalString.Replace(filename, "");
                    }                  


                }
                i++;
            }
            apiResponseViewModel.Data = folderUrl;
            apiResponseViewModel.Message = "Files Uploaded";
            apiResponseViewModel.Successful = true;
            return apiResponseViewModel;
        }

        private async Task<bool> AddImageWithSizeWithRatio(IFormFile file, string name, int id, string filePath, string contentType, string containerName,string imageSizeName, int width, int height )
        {
            string folderUrl = "";
            using (MemoryStream ms = new MemoryStream())
            {

                try
                {
                    using (var image = Image.Load(file.OpenReadStream(), out IImageFormat format))
                    {

                        var imageRatio = (float)image.Width / image.Height;
                        int newHeight = (int)(width / imageRatio);
                        if (newHeight > height)
                        {
                            var newWidth = (int)(height * imageRatio);
                            image.Mutate(x => x.Resize(newWidth, height));

                        }
                        else
                        {
                            image.Mutate(x => x.Resize(width, newHeight));
                        }
                        byte[] fileData;

                        image.Save(ms, format);
                        fileData = ms.ToArray();

                        var filename = imageSizeName + name;
                        ms.Position = 0;
                        var blob = await azureBlobStorage.UploadBlobAsync(ms, id + "/" +
                            filePath + "/" + filename, contentType,
                          containerName, true);
                        if (blob == null)
                        {
                            return false;
                        }
                        folderUrl = blob.StorageUri.PrimaryUri.OriginalString.Replace(filename, "");
                    }

                }
                catch (Exception ex)
                {
                    return false;
                }
                return true;
            }
            
        }

        private async Task<bool> AddImageWithSize(IFormFile file, string name, int id, string filePath, string contentType, string containerName, string imageSizeName, int width, int height)
        {
            string folderUrl = "";
            using (MemoryStream ms = new MemoryStream())
            {

                try
                {
                    using (var image = Image.Load(file.OpenReadStream(), out IImageFormat format))
                    {

                      
                        image.Mutate(x => x.Resize(width, height));
                        byte[] fileData;

                        image.Save(ms, format);
                        fileData = ms.ToArray();

                        var filename = imageSizeName + name;
                        ms.Position = 0;
                        var blob = await azureBlobStorage.UploadBlobAsync(ms, id + "/" +
                            filePath + "/" + filename, contentType,
                          containerName, true);
                        if (blob == null)
                        {
                            return false;
                        }
                        folderUrl = blob.StorageUri.PrimaryUri.OriginalString.Replace(filename, "");
                    }

                }
                catch (Exception ex)
                {
                    return false;
                }
                return true;
            }

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
