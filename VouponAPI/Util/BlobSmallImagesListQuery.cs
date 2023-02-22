using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Voupon.Common.Azure.Blob;

namespace Voupon.API.Util
{
    public class AzureBlob
    {
        public class BlobSmallImagesListModel
        {
            public IAzureBlobStorage AzureBlobStorage { get; set; }
            public bool GetIListBlobItem { get; set; }
            public string ContainerName { get; set; }
            public string FilePath { get; set; }
            public int Id { get; set; }

        }
        public async Task<List<string>> BlobSmallImagesListQuery(BlobSmallImagesListModel request)
        {
            var filename = await request.AzureBlobStorage.ListBlobsAsync(request.ContainerName, request.Id + "/" + request.FilePath);
            if (!request.GetIListBlobItem)
            {
                if (filename != null) 
                {
                    var fileList = new List<string>();
                    foreach (var file in filename)
                    {
                        if (file.StorageUri.PrimaryUri.OriginalString.Contains("small"))
                        {
                            fileList.Add(file.StorageUri.PrimaryUri.OriginalString.Replace("http://", "https://"));
                        }
                        else if (!file.StorageUri.PrimaryUri.OriginalString.Contains("big") && !file.StorageUri.PrimaryUri.OriginalString.Contains("normal") && !file.StorageUri.PrimaryUri.OriginalString.Contains("org"))
                        {
                            fileList.Add(file.StorageUri.PrimaryUri.OriginalString.Replace("http://", "https://"));
                        }

                    }
                    return fileList;
                }
                return null;
            }
            else
            {
                return null;
            }
        }
    }
}
