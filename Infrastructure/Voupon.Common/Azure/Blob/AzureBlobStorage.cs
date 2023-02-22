using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Auth;
using Microsoft.Azure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Voupon.Common.Azure.Blob
{
    public class AzureBlobStorage : IAzureBlobStorage
    {
        private readonly AzureBlobSettings settings;

        public AzureBlobStorage(AzureBlobSettings settings)
        {
            this.settings = settings;
        }

        public async Task<CloudBlob> UploadBlobAsync(Stream stream, string filename, string contentType, string containerName, bool isPublic)
        {

            var container = GetBlobClientAsync().GetContainerReference(containerName);
            try
            {
                var success = container.CreateIfNotExists();
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            if (isPublic)
            {
                var permissions = new BlobContainerPermissions
                {
                    PublicAccess = BlobContainerPublicAccessType.Container
                };
                container.SetPermissions(permissions);
            }
            else
            {
                var permissions = new BlobContainerPermissions
                {
                    PublicAccess = BlobContainerPublicAccessType.Off
                };
                container.SetPermissions(permissions);
            }
            var blob = container.GetBlockBlobReference(filename);
            blob.Properties.ContentType = contentType;

            await blob.UploadFromStreamAsync(stream);
            return blob;
        }

        public List<string> GetBlobUrlList(string containerName)
        {
            var result = new List<string>();
            // Retrieve reference to a previously created container.
            var container = GetBlobClientAsync().GetContainerReference(containerName);

            // Loop over items within the container and output the length and URI.
            foreach (IListBlobItem item in container.ListBlobs())
            {
                if (item.GetType() == typeof(CloudBlockBlob))
                {
                    CloudBlockBlob blob = (CloudBlockBlob)item;
                    result.Add(blob.Uri.ToString());

                }
            }
            return result;
        }


        public async Task<List<IListBlobItem>> ListBlobsAsync(string containerName, string prefix)
        {
            try
            {
                var container = GetBlobClientAsync().GetContainerReference(containerName);
                BlobContinuationToken continuationToken = null;
                List<IListBlobItem> results = new List<IListBlobItem>();
                do
                {
                    bool useFlatBlobListing = true;
                    BlobListingDetails blobListingDetails = BlobListingDetails.None;
                    int maxBlobsPerRequest = 500;
                    var response = await container.ListBlobsSegmentedAsync(prefix, useFlatBlobListing, blobListingDetails, maxBlobsPerRequest, continuationToken, null, null);
                    continuationToken = response.ContinuationToken;
                    results.AddRange(response.Results);
                }
                while (continuationToken != null);
                return results;
            }
            catch (Exception ex)
            {
                return null;
            }
        }


        public async Task<MemoryStream> DownloadAsync(string containerName, string blobName)
        {
            var blobContainer = GetBlockBlobAsync(containerName, blobName);
            using (var stream = new MemoryStream())
            {
                await blobContainer.DownloadToStreamAsync(stream);
                return stream;
            }
        }

        public async Task<bool> CopyAsync(string containerName, string blobName, string copyTo)
        {
            CloudBlockBlob blockBlob = GetBlockBlobAsync(containerName, blobName);
            CloudBlockBlob blockCopyToBlob = GetBlockBlobAsync(containerName, copyTo);
            await blockCopyToBlob.StartCopyAsync(blockBlob);
            return true;
        }

        public void DeleteContainerFiles(string containerName, string prefix)
        {
            CloudBlobContainer container = GetBlobClientAsync().GetContainerReference(containerName);
            CloudBlobDirectory directory = container.GetDirectoryReference(prefix);
            var list = directory.ListBlobs();
            var blobNames = list.OfType<CloudBlockBlob>().ToList();
            foreach (var tempBlob in blobNames)
            {
                tempBlob.Delete(DeleteSnapshotsOption.IncludeSnapshots);
            }
        }

        public void DeleteFiles(string containerName, string prefix, string[] removeFileList)
        {
            if (removeFileList == null || removeFileList.Count() == 0)
            {
                return;
            }
            CloudBlobContainer container = GetBlobClientAsync().GetContainerReference(containerName);
            CloudBlobDirectory directory = container.GetDirectoryReference(prefix);
            var list = directory.ListBlobs();
            var blobNames = list.OfType<CloudBlockBlob>().ToList();
            foreach (var tempBlob in blobNames)
            {
                if (removeFileList.Contains(tempBlob.StorageUri.PrimaryUri.OriginalString) || removeFileList.Contains(tempBlob.StorageUri.PrimaryUri.OriginalString.Replace("http", "https")))
                    tempBlob.Delete(DeleteSnapshotsOption.IncludeSnapshots);
            }
        }

        public async Task DeleteAsync(string containerName, string blobName)
        {
            CloudBlockBlob blockBlob = GetBlockBlobAsync(containerName, blobName);
            if (blockBlob.Exists())
            {
                await blockBlob.DeleteAsync();
            }
        }

        public async Task<bool> ExistsAsync(string containerName, string blobName)
        {
            CloudBlockBlob blockBlob = GetBlockBlobAsync(containerName, blobName);

            //Exists
            return await blockBlob.ExistsAsync();
        }


        private CloudBlobClient GetBlobClientAsync()
        {
            CloudStorageAccount storageAccount = new CloudStorageAccount(
                new StorageCredentials(settings.StorageAccount, settings.StorageKey), false);

            return storageAccount.CreateCloudBlobClient();
        }

        private CloudBlockBlob GetBlockBlobAsync(string containerName, string blobName)
        {
            //Container
            var blobContainer = GetBlobClientAsync().GetContainerReference(containerName);
            if (!blobContainer.Exists())
            {
                return null;
            }
            //Blob
            return blobContainer.GetBlockBlobReference(blobName);
        }

        private async Task<List<AzureBlobItem>> GetBlobListAsync(string containerName, bool useFlatListing = true)
        {
            //Container
            var blobContainer = GetBlobClientAsync().GetContainerReference(containerName);

            //List
            var list = new List<AzureBlobItem>();
            BlobContinuationToken token = null;
            do
            {
                BlobResultSegment resultSegment =
                    await blobContainer.ListBlobsSegmentedAsync("", useFlatListing, new BlobListingDetails(), null, token, null, null);
                token = resultSegment.ContinuationToken;

                foreach (IListBlobItem item in resultSegment.Results)
                {
                    list.Add(new AzureBlobItem(item));
                }
            } while (token != null);

            return list.OrderBy(i => i.Folder).ThenBy(i => i.Name).ToList();
        }
    }
}
