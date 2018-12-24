using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace AzureBlobToS3
{
    public class AzureManager
    {
        public class BlobItem
        {
            public string BlobName { get; set; }
            public string MD5 { get; set; }
            public Type ObjectType { get; set; }
        }
        public static List<BlobItem> ListBlobContainer(CloudStorageAccount storageAccount,string ContainerName, string Prefix)
        {
            CloudBlobClient cloudBlobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer cloudBlobContainer = cloudBlobClient.GetContainerReference(ContainerName);

            Console.WriteLine($"Listing container {ContainerName}");

            BlobContinuationToken blobContinuationToken = null;
            List<BlobItem> lstBlobItems = new List<BlobItem>();

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
                        lstBlobItems.Add(new BlobItem { BlobName = blob.Name, MD5 = blob.Properties.ETag, ObjectType = typeof(CloudBlockBlob) });
                        Console.WriteLine("Block blob {2} of length {0}: {1}, MD5 {3}", blob.Properties.Length, blob.Uri, blob.Name, blob.Properties.ETag);
                    }
                    else if (item.GetType() == typeof(CloudPageBlob))
                    {
                        CloudPageBlob pageBlob = (CloudPageBlob)item;
                        lstBlobItems.Add(new BlobItem { BlobName = pageBlob.Name, MD5 = pageBlob.Properties.ETag, ObjectType = typeof(CloudPageBlob) });


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
