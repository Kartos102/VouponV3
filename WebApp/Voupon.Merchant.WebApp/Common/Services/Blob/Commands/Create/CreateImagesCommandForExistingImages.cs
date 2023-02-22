using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Common.Azure.Blob;
using Voupon.Merchant.WebApp.Common.Services.Blob.Queries;
using Voupon.Merchant.WebApp.ViewModels;

namespace Voupon.Merchant.WebApp.Common.Services.Blob.Commands.Create
{
    public class CreateImagesCommandForExistingImages : IRequest<ApiResponseViewModel>
    {
        public int Id { get; set; }
        public IFormFileCollection Files { get; set; }
        public string FilePath { get; set; }
        public string ContainerName { get; set; }
    }

    public class CreateImagesCommandForExistingImagesHandler : IRequestHandler<CreateImagesCommandForExistingImages, ApiResponseViewModel>
    {
        private readonly IAzureBlobStorage azureBlobStorage;
        private readonly IOptions<AppSettings> appSettings;


        public CreateImagesCommandForExistingImagesHandler(IAzureBlobStorage azureBlobStorage, IOptions<AppSettings> appSettings)
        {
            this.azureBlobStorage = azureBlobStorage;
            this.appSettings = appSettings;

        }
        public async Task<ApiResponseViewModel> Handle(CreateImagesCommandForExistingImages request, CancellationToken cancellationToken)
        {

            var apiResponseViewModel = new ApiResponseViewModel();
            string folderUrl = "";
            var existingFilename = await azureBlobStorage.ListBlobsAsync(request.ContainerName, request.Id + "/" + request.FilePath);
            var ListOfExistingImages = new List<string>();
            var fileList = new List<IListBlobItem>();

            if (existingFilename.Count > 0)
            {
                for (int k = 0; k < existingFilename.Count; k++)
                {
                    //if (existingFilename[k].StorageUri.PrimaryUri.OriginalString.Contains("small"))
                    //{
                    //    ListOfExistingImages.Add(existingFilename[k].StorageUri.PrimaryUri.OriginalString);
                    //    fileList.Add(existingFilename[k]);
                    //}
                    //else 
                    if (!existingFilename[k].StorageUri.PrimaryUri.OriginalString.Contains("big") && !existingFilename[k].StorageUri.PrimaryUri.OriginalString.Contains("normal") && !existingFilename[k].StorageUri.PrimaryUri.OriginalString.Contains("small"))
                    {
                        ListOfExistingImages.Add(existingFilename[k].StorageUri.PrimaryUri.ToString());
                        fileList.Add(existingFilename[k]);

                    }

                }
            }
            int i = 1;

            IFormFile file;

            System.Net.WebClient wc = new System.Net.WebClient();
            byte[] fileData;
            //Uri uri = new Uri(ListOfExistingImages[0]);
            foreach (string ExistedImage in ListOfExistingImages)
            {

                using (MemoryStream ms = new MemoryStream(wc.DownloadData(ExistedImage)))
                {
                    fileData = ms.ToArray();
                    //ReturnFormFile(FileStreamResult result)
                    var stream = new MemoryStream(fileData);
                    file = new FormFile(stream, 0, fileData.Length, "name", ExistedImage.Split('/').Last());
                    var test = existingFilename.FirstOrDefault().StorageUri.PrimaryUri.ToString();
                    var temp = existingFilename.FirstOrDefault(x => x.StorageUri.PrimaryUri.ToString() == ExistedImage);
                    if (temp != null)
                    {
                        var blobName = ExistedImage.Replace(temp.Container.Uri.ToString() + "/", "");

                        var contentType = ListOfExistingImages[0].Split('.').Last();
                        var name = i + "_" + Guid.NewGuid() + "." + contentType;

                        //var contentType = file.ContentType.Split('/')[1];
                        //    var name = i + "_" + Guid.NewGuid() + "." + contentType;

                        int smallImageHeight = Int16.Parse(appSettings.Value.ProductImage.Small.Height);
                        int normalImageHeight = Int16.Parse(appSettings.Value.ProductImage.Normal.Height);
                        int bigImageHeight = Int16.Parse(appSettings.Value.ProductImage.Big.Height);

                        int smallImageWidth = Int16.Parse(appSettings.Value.ProductImage.Small.Width);
                        int normalImageWidth = Int16.Parse(appSettings.Value.ProductImage.Normal.Width);
                        int bigImageWidth = Int16.Parse(appSettings.Value.ProductImage.Big.Width);

                        var result = AddImageWithSizeWithRatio(file, name, request.Id, request.FilePath, contentType, request.ContainerName, "small_", smallImageWidth, smallImageHeight).Result;
                        if (!result)
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
                        await azureBlobStorage.CopyAsync(request.ContainerName, blobName, request.Id + "/" + request.FilePath + "/" + "org_" + name);

                        await azureBlobStorage.DeleteAsync(request.ContainerName, blobName);
                        //using (MemoryStream mes = new MemoryStream())
                        //{
                        //    byte[] fileData;
                        //    file.CopyTo(mes);
                        //    fileData = mes.ToArray();
                        //    var contentTypeOriginalImage = file.ContentType.Split('/')[1];
                        //    var filename = i + "_" + Guid.NewGuid() + "." + contentTypeOriginalImage;
                        //    mes.Position = 0;
                        //    var blob = await azureBlobStorage.UploadBlobAsync(mes, request.Id + "/" +
                        //        request.FilePath + "/" + "org_" + name, contentTypeOriginalImage,
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
                i++;

            }

    
            apiResponseViewModel.Data = folderUrl;
            apiResponseViewModel.Message = "Files Uploaded";
            apiResponseViewModel.Successful = true;
            return apiResponseViewModel;
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
                        folderUrl = blob.StorageUri.PrimaryUri.ToString().Replace(filename, "");
                    }

                }
                catch (Exception ex)
                {
                    return false;
                }
                return true;
            }

        }

        private async Task<bool> AddImageWithSizeWithRatio(IFormFile file, string name, int id, string filePath, string contentType, string containerName, string imageSizeName, int width, int height)
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
                        folderUrl = blob.StorageUri.PrimaryUri.ToString().Replace(filename, "");
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

        public IFormFile ReturnFormFile(FileStreamResult result)
        {
            var ms = new MemoryStream();
            try
            {
                result.FileStream.CopyTo(ms);
                return new FormFile(ms, 0, ms.Length, "name", "fileName");
            }
            catch (Exception e)
            {
                ms.Dispose();
                throw;
            }
            finally
            {
                ms.Dispose();
            }
        }
    }
}
