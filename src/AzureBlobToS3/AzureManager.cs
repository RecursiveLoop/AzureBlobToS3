using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Shared;

namespace AzureBlobToS3
{
    public class AzureManager
    {
        static CloudBlobClient cloudBlobClient;
        public static List<AzureBlobItem> ListBlobContainer(CloudStorageAccount storageAccount, string ContainerName, string Prefix)
        {
            if (cloudBlobClient == null)
                cloudBlobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer cloudBlobContainer = cloudBlobClient.GetContainerReference(ContainerName);

            Console.WriteLine($"Listing container {ContainerName}");

            BlobContinuationToken blobContinuationToken = null;
            List<AzureBlobItem> lstBlobItems = new List<AzureBlobItem>();

            do
            {
                var results = cloudBlobContainer.ListBlobsSegmentedAsync(Prefix, true, BlobListingDetails.None, null, blobContinuationToken, null, null).GetAwaiter().GetResult();
                // Get the value of the continuation token returned by the listing call.
                blobContinuationToken = results.ContinuationToken;
                foreach (IListBlobItem item in results.Results)
                {
                    if (item.GetType() == typeof(CloudBlockBlob))
                    {
                        CloudBlockBlob blob = (CloudBlockBlob)item;
                        lstBlobItems.Add(new AzureBlobItem { ContainerName = ContainerName, BlobName = blob.Name, MD5 = blob.Properties.ContentMD5, Size = blob.Properties.Length, ObjectTypeName = typeof(CloudBlockBlob).FullName });
                        Console.WriteLine("Block blob {2} of length {0}: {1}, MD5 {3}", blob.Properties.Length, blob.Uri, blob.Name, blob.Properties.ETag);
                    }
                    else if (item.GetType() == typeof(CloudPageBlob))
                    {
                        CloudPageBlob pageBlob = (CloudPageBlob)item;
                        lstBlobItems.Add(new AzureBlobItem { ContainerName = ContainerName, BlobName = pageBlob.Name, Size = pageBlob.Properties.Length, MD5 = pageBlob.Properties.ContentMD5, ObjectTypeName = typeof(CloudPageBlob).FullName });


                        Console.WriteLine("Page blob {2} of length {0}: {1}, MD5 {3}", pageBlob.Properties.Length, pageBlob.Uri, pageBlob.Name, pageBlob.Properties.ETag);
                    }
                    else if (item.GetType() == typeof(CloudBlobDirectory))
                    {
                        CloudBlobDirectory directory = (CloudBlobDirectory)item;

                        Console.WriteLine("Directory: {0}", directory.Uri);
                    }


                }
            } while (blobContinuationToken != null); // Loop while the continuation token is not null. 

            lstBlobItems = lstBlobItems.OrderBy(a => a.BlobName).ToList();

            Console.WriteLine($"{lstBlobItems.Count.ToString()} items retrieved.");

            return lstBlobItems;
        }
    }
}
