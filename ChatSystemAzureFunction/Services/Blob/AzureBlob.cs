using System;
using System.Collections.Generic;
using System.Text;
using Voupon.Common.Azure.Blob;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.Storage;



namespace ChatSystemAzureFunction.Services.Blob
{
    public class AzureBlob : IAzureBlob
    {
        private readonly IAzureBlobStorage azureBlobStorage;

        public AzureBlob(IAzureBlobStorage azureBlobStorage)
        {
            this.azureBlobStorage = azureBlobStorage;
        }
        public async Task<string> CreateFilesCommand(long id, IFormFileCollection files, string filePath, string containerName, bool? isPublic)
        {
            try
            {
                string folderUrl = "";
                var existingFilename = await azureBlobStorage.ListBlobsAsync(containerName, id + "/" + filePath);
                int i = 1;
                foreach (IFormFile file in files)
                {
                    if (file.Length == 0)
                    {
                        var temp = existingFilename.FirstOrDefault(x => x.StorageUri.PrimaryUri.ToString() == file.FileName);
                        if (temp != null)
                        {
                            var blobName = file.FileName.Replace(temp.Container.Uri.ToString() + "/", "");
                            var contentType = GetContentType(file.FileName).Split('/')[1];
                            var filename = i + "_" + Guid.NewGuid().ToString() + "." + contentType;
                            await azureBlobStorage.CopyAsync(containerName, blobName, id + "/" +
                                filePath + "/" + filename);
                            await azureBlobStorage.DeleteAsync(containerName, blobName);

                        }

                    }
                    else
                        using (MemoryStream ms = new MemoryStream())
                        {

                            byte[] fileData;
                            file.CopyTo(ms);
                            fileData = ms.ToArray();
                            var contentType = file.ContentType.Split('/')[1];
                            var filename = i + "_" + Guid.NewGuid().ToString() + "." + contentType;
                            ms.Position = 0;
                            var blob = await azureBlobStorage.UploadBlobAsync(ms, id + "/" +
                                filePath + "/" + filename, contentType,
                              containerName, (isPublic.HasValue ? isPublic.Value : true));
                            if (blob == null)
                            {
                                return "";
                            }
                            folderUrl = blob.StorageUri.PrimaryUri.ToString().Replace(filename, "");
                        }
                    i++;
                }
                return folderUrl;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<List<string>> BlobFilesListQuery(long id, string filePath, string containerName, bool? isPublic)
        {
            var filename = await azureBlobStorage.ListBlobsAsync(containerName, id + "/" + filePath);
            //if (!request.GetIListBlobItem)
            //{
                var fileList = new List<string>();
                foreach (var file in filename)
                {
                    fileList.Add(file.StorageUri.PrimaryUri.ToString().Replace("http://", "https://").Replace(":80", ""));

                }
                return fileList;
            //}
            //else
            //{
            //    apiResponseViewModel.Data = filename;
            //}

        }

        public async Task<string> SASTokenQuery()
        {
            
            var storageAccountName = Environment.GetEnvironmentVariable("AzureConfigurationsStorageAccount");
            var accessKey = Environment.GetEnvironmentVariable("AzureConfigurationsStorageKey");
            var connectionString = String.Format("DefaultEndpointsProtocol=https;AccountName={0};AccountKey={1}",
                storageAccountName,
                accessKey);
            var storageAccount = CloudStorageAccount.Parse(connectionString);

            var policy = new SharedAccessAccountPolicy()
            {
                Permissions = SharedAccessAccountPermissions.Read,
                Services = SharedAccessAccountServices.Blob,
                ResourceTypes = SharedAccessAccountResourceTypes.Container | SharedAccessAccountResourceTypes.Object,
                SharedAccessExpiryTime = DateTime.UtcNow.AddMinutes(30),
                Protocols = SharedAccessProtocol.HttpsOnly,
            };

            return storageAccount.GetSharedAccessSignature(policy);
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
