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
using ChatSystemAzureFunction.ViewModels;

namespace ChatSystemAzureFunction.Services.Blob.Commands.Create
{
    public class DeleteResizedImagesAndKeepOriginal : IRequest<ApiResponseViewModel>
    {
        public int Id { get; set; }
        public IFormFileCollection Files { get; set; }
        public string FilePath { get; set; }
        public string ContainerName { get; set; }
    }

    public class DeleteResizedImagesAndKeepOriginalHandler : IRequestHandler<DeleteResizedImagesAndKeepOriginal, ApiResponseViewModel>
    {
        private readonly IAzureBlobStorage azureBlobStorage;
        //private readonly IOptions<AppSettings> appSettings;


        public DeleteResizedImagesAndKeepOriginalHandler(IAzureBlobStorage azureBlobStorage/*, IOptions<AppSettings> appSettings*/)
        {
            this.azureBlobStorage = azureBlobStorage;
            //this.appSettings = appSettings;

        }
        public async Task<ApiResponseViewModel> Handle(DeleteResizedImagesAndKeepOriginal request, CancellationToken cancellationToken)
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
                    if (existingFilename[k].StorageUri.PrimaryUri.OriginalString.Contains("org"))
                    {
                        //ListOfExistingImages.Add(existingFilename[k].StorageUri.PrimaryUri.OriginalString);
                        //fileList.Add(existingFilename[k]);
                        var temp = existingFilename.FirstOrDefault(x => x.StorageUri.PrimaryUri.ToString() == existingFilename[k].StorageUri.PrimaryUri.OriginalString);

                        var blobName = existingFilename[k].StorageUri.PrimaryUri.OriginalString.Replace(temp.Container.Uri.ToString() + "/", "");
                        //            var contentType = GetContentType(file.FileName).Split('/')[1];
                        //            var filename = i + "_" + Guid.NewGuid().ToString() + "." + contentType;
                        await azureBlobStorage.CopyAsync(request.ContainerName, blobName, request.Id + "/" + request.FilePath + "/" + blobName.Split('/').Last().Replace("org", ""));

                        await azureBlobStorage.DeleteAsync(request.ContainerName, blobName);
                    }
                    else
                    if (existingFilename[k].StorageUri.PrimaryUri.OriginalString.Contains("normal") || existingFilename[k].StorageUri.PrimaryUri.OriginalString.Contains("small") || existingFilename[k].StorageUri.PrimaryUri.OriginalString.Contains("big"))
                    {
                        //ListOfExistingImages.Add(existingFilename[k].StorageUri.PrimaryUri.OriginalString);
                        //fileList.Add(existingFilename[k]);

                        var temp = existingFilename.FirstOrDefault(x => x.StorageUri.PrimaryUri.ToString() == existingFilename[k].StorageUri.PrimaryUri.OriginalString);
                        var blobName = existingFilename[k].StorageUri.PrimaryUri.OriginalString.Replace(temp.Container.Uri.ToString() + "/", "");
                        await azureBlobStorage.DeleteAsync(request.ContainerName, blobName);
                    }

                }
            }
            //int i = 1;

            //IFormFile file;

            //System.Net.WebClient wc = new System.Net.WebClient();
            //byte[] fileData;
            ////Uri uri = new Uri(ListOfExistingImages[0]);
            //foreach (string ExistedImage in ListOfExistingImages)
            //{

            //    using (MemoryStream ms = new MemoryStream(wc.DownloadData(ExistedImage)))
            //    {
            //        fileData = ms.ToArray();
            //        //ReturnFormFile(FileStreamResult result)
            //        var stream = new MemoryStream(fileData);
            //        file = new FormFile(stream, 0, fileData.Length, "name", ExistedImage.Split('/').Last());
            //        var test = existingFilename.FirstOrDefault().StorageUri.PrimaryUri.ToString();
            //        var temp = existingFilename.FirstOrDefault(x => x.StorageUri.PrimaryUri.ToString() == ExistedImage);
            //        if (temp != null)
            //        {
            //            var blobName = ExistedImage.Replace(temp.Container.Uri.ToString() + "/", "");

            //            var contentType = ListOfExistingImages[0].Split('.').Last();
            //            var name = i + "_" + Guid.NewGuid().ToString() + "." + contentType;

            //            //var contentType = file.ContentType.Split('/')[1];
            //            //    var name = i + "_" + Guid.NewGuid().ToString() + "." + contentType;

            //            int smallImageHeight = Int16.Parse(appSettings.Value.ProductImage.Small.Height);
            //            int normalImageHeight = Int16.Parse(appSettings.Value.ProductImage.Normal.Height);
            //            int bigImageHeight = Int16.Parse(appSettings.Value.ProductImage.Big.Height);

            //            int smallImageWidth = Int16.Parse(appSettings.Value.ProductImage.Small.Width);
            //            int normalImageWidth = Int16.Parse(appSettings.Value.ProductImage.Normal.Width);
            //            int bigImageWidth = Int16.Parse(appSettings.Value.ProductImage.Big.Width);

            //            var result = AddImageWithSizeWithRatio(file, name, request.Id, request.FilePath, contentType, request.ContainerName, "small_", smallImageWidth, smallImageHeight).Result;
            //            if (!result)
            //            {
            //                apiResponseViewModel.Message = "Fail to process request. Please try again later";
            //                apiResponseViewModel.Successful = false;
            //                return apiResponseViewModel;
            //            }

            //            result = AddImageWithSizeWithRatio(file, name, request.Id, request.FilePath, contentType, request.ContainerName, "normal_", normalImageWidth, normalImageHeight).Result;
            //            if (!result)
            //            {
            //                apiResponseViewModel.Message = "Fail to process request. Please try again later";
            //                apiResponseViewModel.Successful = false;
            //                return apiResponseViewModel;
            //            }

            //            result = AddImageWithSizeWithRatio(file, name, request.Id, request.FilePath, contentType, request.ContainerName, "big_", bigImageWidth, bigImageHeight).Result;
            //            if (!result)
            //            {
            //                apiResponseViewModel.Message = "Fail to process request. Please try again later";
            //                apiResponseViewModel.Successful = false;
            //                return apiResponseViewModel;
            //            }
            //            await azureBlobStorage.DeleteAsync(request.ContainerName, blobName);
            //        }
            //    }
            //    i++;

            //}

            //using (MemoryStream ms = new MemoryStream(wc.DownloadData(ListOfExistingImages[0])))
            //{
            //    fileData = ms.ToArray();

            //    //ReturnFormFile(FileStreamResult result)
            //    var stream = new MemoryStream(fileData);
            //    newFile = new FormFile(stream, 0, fileData.Length, "name", "fileName");
            //    var contentType = ListOfExistingImages[0].Split('.').Last();
            //    var name = 1 + "_" + Guid.NewGuid().ToString() + "." + contentType;

            //    var result = AddImageWithSize(newFile, name, request.Id, request.FilePath, contentType, request.ContainerName, "big_", 800, 800).Result;
            //    if (!result)
            //    {
            //        apiResponseViewModel.Message = "Fail to process request. Please try again later";
            //        apiResponseViewModel.Successful = false;
            //        return apiResponseViewModel;
            //    }


            //}



            //var client = new HttpClient();
            //var response = await client.GetAsync((Uri)@existingFilename[0]);

            //using (var stream = await response.Content.ReadAsStreamAsync())
            //{
            //    var fileInfo = new FileInfo("myfile.jpg");
            //    using (var fileStream = fileInfo.OpenWrite())
            //    {
            //        await stream.CopyToAsync(fileStream);
            //    }
            //}

            //foreach (IFormFile file in  request.Files)
            //{
            //    if (file.Length == 0)
            //    {
            //        var temp = existingFilename.FirstOrDefault(x => x.StorageUri.PrimaryUri.ToString() == file.FileName);
            //        if (temp != null)
            //        {

            //            var blobName = file.FileName.Replace(temp.Container.Uri.ToString() + "/", "");
            //            var contentType = GetContentType(file.FileName).Split('/')[1];
            //            var filename = i + "_" + Guid.NewGuid().ToString() + "." + contentType;
            //            if (blobName.Contains("big"))
            //            {
            //                var name = blobName.Replace("big_", "");

            //                await azureBlobStorage.CopyAsync(request.ContainerName, blobName, request.Id + "/" + request.FilePath + "/" + "big_" + filename);

            //                if (await azureBlobStorage.ExistsAsync(request.ContainerName, blobName.Replace("big", "normal")))
            //                    await azureBlobStorage.CopyAsync(request.ContainerName, blobName.Replace("big", "normal"), request.Id + "/" + request.FilePath + "/" + "normal_" + filename);
            //                if (await azureBlobStorage.ExistsAsync(request.ContainerName, blobName.Replace("big", "small")))
            //                    await azureBlobStorage.CopyAsync(request.ContainerName, blobName.Replace("big", "small"), request.Id + "/" + request.FilePath + "/" + "small_" + filename);
            //            }
            //            else if (blobName.Contains("normal"))
            //            {
            //                var name = blobName.Replace("normal_", "");

            //                await azureBlobStorage.CopyAsync(request.ContainerName, blobName, request.Id + "/" + request.FilePath + "/" + "normal_" + filename);

            //                if (await azureBlobStorage.ExistsAsync(request.ContainerName, blobName.Replace("normal", "big")))
            //                    await azureBlobStorage.CopyAsync(request.ContainerName, blobName.Replace("normal", "big"), request.Id + "/" + request.FilePath + "/" + "big_" + filename);
            //                if (await azureBlobStorage.ExistsAsync(request.ContainerName, blobName.Replace("normal", "small")))
            //                    await azureBlobStorage.CopyAsync(request.ContainerName, blobName.Replace("normal", "small"), request.Id + "/" + request.FilePath + "/" + "small_" + filename);
            //            }
            //            else if (blobName.Contains("small"))
            //            {
            //                var name = blobName.Replace("small_", "");
            //                await azureBlobStorage.CopyAsync(request.ContainerName, blobName, request.Id + "/" + request.FilePath + "/" + "small_" + filename);

            //                if (await azureBlobStorage.ExistsAsync(request.ContainerName, blobName.Replace("small", "big")))
            //                    await azureBlobStorage.CopyAsync(request.ContainerName, blobName.Replace("small", "big"), request.Id + "/" + request.FilePath + "/" + "big_" + filename);
            //                if (await azureBlobStorage.ExistsAsync(request.ContainerName, blobName.Replace("small", "normal")))
            //                    await azureBlobStorage.CopyAsync(request.ContainerName, blobName.Replace("small", "normal"), request.Id + "/" + request.FilePath + "/" + "normal_" + filename);
            //            }
            //            else
            //            {
            //                await azureBlobStorage.CopyAsync(request.ContainerName, blobName, request.Id + "/" + request.FilePath + "/" + filename);

            //            }

            //            if (blobName.Contains("big"))
            //            {
            //                await azureBlobStorage.DeleteAsync(request.ContainerName, blobName.Replace("big", "normal"));
            //                await azureBlobStorage.DeleteAsync(request.ContainerName, blobName.Replace("big", "small"));
            //            }
            //            else if (blobName.Contains("normal"))
            //            {
            //                await azureBlobStorage.DeleteAsync(request.ContainerName, blobName.Replace("normal", "big"));
            //                await azureBlobStorage.DeleteAsync(request.ContainerName, blobName.Replace("normal", "small"));
            //            }
            //            else if (blobName.Contains("small"))
            //            {
            //                await azureBlobStorage.DeleteAsync(request.ContainerName, blobName.Replace("small", "big"));
            //                await azureBlobStorage.DeleteAsync(request.ContainerName, blobName.Replace("small", "normal"));
            //            }
            //            await azureBlobStorage.DeleteAsync(request.ContainerName, blobName);



            //        }

            //    }
            //    else
            //    {
            //        var contentType = file.ContentType.Split('/')[1];
            //        var name = i + "_" + Guid.NewGuid().ToString() + "." + contentType;
            //        var result = AddImageWithSize(file, name, request.Id, request.FilePath, contentType, request.ContainerName, "small_", 180, 180).Result;
            //        if (!result)
            //        {
            //            apiResponseViewModel.Message = "Fail to process request. Please try again later";
            //            apiResponseViewModel.Successful = false;
            //            return apiResponseViewModel;
            //        }

            //        result = AddImageWithSize(file, name, request.Id, request.FilePath, contentType, request.ContainerName, "normal_", 600, 400).Result;
            //        if (!result)
            //        {
            //            apiResponseViewModel.Message = "Fail to process request. Please try again later";
            //            apiResponseViewModel.Successful = false;
            //            return apiResponseViewModel;
            //        }

            //        result = AddImageWithSize(file, name, request.Id, request.FilePath, contentType, request.ContainerName, "big_", 800, 800).Result;
            //        if (!result)
            //        {
            //            apiResponseViewModel.Message = "Fail to process request. Please try again later";
            //            apiResponseViewModel.Successful = false;
            //            return apiResponseViewModel;
            //        }
            //        //using (MemoryStream ms = new MemoryStream())
            //        //{
            //        //    try
            //        //    {
            //        //        using (var image = Image.Load(file.OpenReadStream(), out IImageFormat format))
            //        //        {
            //        //            image.Mutate(x => x.Resize(180, 180));
            //        //            byte[] fileData;

            //        //            image.Save(ms, format);
            //        //            //file.CopyTo(ms);
            //        //            fileData = ms.ToArray();

            //        //            var filename = "small_" + name;
            //        //            ms.Position = 0;
            //        //            var blob = await azureBlobStorage.UploadBlobAsync(ms, request.Id + "/" +
            //        //                request.FilePath + "/" + filename, contentType,
            //        //              request.ContainerName, true);
            //        //            if (blob == null)
            //        //            {
            //        //                apiResponseViewModel.Message = "Fail to process request. Please try again later";
            //        //                apiResponseViewModel.Successful = false;
            //        //                return apiResponseViewModel;
            //        //            }
            //        //            folderUrl = blob.StorageUri.PrimaryUri.OriginalString.Replace(filename, "");
            //        //        }

            //        //        //    return Json(new { Message = "OK" });
            //        //    }
            //        //    catch (Exception)
            //        //    {
            //        //        return apiResponseViewModel;
            //        //    }

            //        //}

            //    }
            //    i++;
            //}
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
