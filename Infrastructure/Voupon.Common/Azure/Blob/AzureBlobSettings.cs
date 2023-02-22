using System;
using System.Collections.Generic;
using System.Text;

namespace Voupon.Common.Azure.Blob
{
    public class AzureBlobSettings
    {
        public AzureBlobSettings(string storageAccount, string storageKey)
        {
            StorageAccount = storageAccount;
            StorageKey = storageKey;
        }

        public string StorageAccount { get; }
        public string StorageKey { get; }
    }
}
