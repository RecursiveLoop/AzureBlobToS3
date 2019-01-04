using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace CopyAzureBlobToS3
{
    public class AzureManager
    {
       

        public static void GetBlobStream(CloudStorageAccount storageAccount, string ContainerName, string BlobName, Stream outputStream)
        {
            CloudBlobClient cloudBlobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer cloudBlobContainer = cloudBlobClient.GetContainerReference(ContainerName);

            var blobRef = cloudBlobContainer.GetBlobReference(BlobName);

            blobRef.DownloadRangeToStream(outputStream, null, null);
        }
    }
}
