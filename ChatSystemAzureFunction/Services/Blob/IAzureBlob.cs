using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;


namespace ChatSystemAzureFunction.Services.Blob
{
    public interface IAzureBlob
    {
        Task<string> CreateFilesCommand(long id, IFormFileCollection files, string filePath, string containerName, bool? isPublic);
        Task<List<string>> BlobFilesListQuery(long id, string filePath, string containerName, bool? isPublic);
        Task<string> SASTokenQuery();

    }
}
