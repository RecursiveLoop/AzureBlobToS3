using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.XRay.Recorder.Core;
using Amazon.XRay.Recorder.Handlers.AwsSdk;

using Amazon.Lambda.Core;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Shared;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace AzureBlobToS3
{
    public class Function
    {
        CloudStorageAccount storageAccount = null;

        public Function()
        {
            AWSSDKHandler.RegisterXRayForAllServices();
        }

        /// <summary>
        /// A simple function that takes a string and does a ToUpper
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public void FunctionHandler( ILambdaContext context)
        {
            SSMParameterManager ssmParameterManager = new SSMParameterManager();
            string storageConnectionString;

            if (!ssmParameterManager.TryGetValue(Constants.StorageConnectionStringSSMPath, out storageConnectionString))
            {
                throw new Exception("Storage connection path not defined.");
            }
            else
                Console.WriteLine($"Storage connection path is set at '{storageConnectionString}'");

            // Check whether the connection string can be parsed.
            if (CloudStorageAccount.TryParse(storageConnectionString, out storageAccount))
            {
                string ContainerNames;
                if (ssmParameterManager.TryGetValue(Constants.StorageContainerNamesSSMPath, out ContainerNames))
                {
                   var arrContainerNames= ContainerNames.Split(',', StringSplitOptions.RemoveEmptyEntries);
                    List<Task> lstTasks = new List<Task>();
                    foreach(var ContainerName in arrContainerNames)
                    {
                        var t=Task.Factory.StartNew(() =>
                        {
                        ListBlobContainer(ContainerName, null);
                        });
                        lstTasks.Add(t);
                    }

                    Task.WaitAll(lstTasks.ToArray());
                }


            }
            else
            {
                // Otherwise, let the user know that they need to define the environment variable.
                throw new Exception("Storage connection path not defined.");
            }

        }

        void ListBlobContainer(string ContainerName,string Prefix)
        {
            CloudBlobClient cloudBlobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer cloudBlobContainer = cloudBlobClient.GetContainerReference(ContainerName);

            Console.WriteLine($"Listing container {ContainerName}");
            
            BlobContinuationToken blobContinuationToken = null;
            do
            {
                var results = cloudBlobContainer.ListBlobsSegmentedAsync(Prefix,true, BlobListingDetails.None, null,blobContinuationToken,null,null).GetAwaiter().GetResult();
                // Get the value of the continuation token returned by the listing call.
                blobContinuationToken = results.ContinuationToken;
                foreach (IListBlobItem item in results.Results)
                {
                    if (item.GetType() == typeof(CloudBlockBlob))
                    {
                        CloudBlockBlob blob = (CloudBlockBlob)item;
                        Console.WriteLine("Block blob {2} of length {0}: {1}", blob.Properties.Length, blob.Uri, blob.Name);
                    }
                    else if (item.GetType() == typeof(CloudPageBlob))
                    {
                        CloudPageBlob pageBlob = (CloudPageBlob)item;
                        Console.WriteLine("Page blob {2} of length {0}: {1}", pageBlob.Properties.Length, pageBlob.Uri, pageBlob.Name);
                    }
                    else if (item.GetType() == typeof(CloudBlobDirectory))
                    {
                        CloudBlobDirectory directory = (CloudBlobDirectory)item;
                        Console.WriteLine("Directory: {0}", directory.Uri);
                    }


                }
            } while (blobContinuationToken != null); // Loop while the continuation token is not null. 
        }
    }
}
