using Microsoft.Azure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Voupon.Common.Azure.Blob
{
    public interface IAzureBlobStorage
    {
        void DeleteContainerFiles(string containerName, string prefix);
        void DeleteFiles(string containerName, string prefix, string[] removeFileList);
        Task<CloudBlob> UploadBlobAsync(Stream stream, string filename, string contentType, string containerName, bool isPublic);
        Task<MemoryStream> DownloadAsync(string containerName, string blobName);
        Task DeleteAsync(string containerName, string blobName);
        Task<bool> ExistsAsync(string containerName, string blobName);

        Task<List<IListBlobItem>> ListBlobsAsync(string containerName, string prefix = "");
        Task<bool> CopyAsync(string containerName, string blobName, string copyTo);
    }
}
