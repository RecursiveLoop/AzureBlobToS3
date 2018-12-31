using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Amazon.Lambda.Core;
using Amazon.XRay.Recorder.Handlers.AwsSdk;
using Microsoft.WindowsAzure.Storage;
using Newtonsoft.Json;
using Shared;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace CopyAzureBlobToS3
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
        public void FunctionHandler(Amazon.Lambda.SQSEvents.SQSEvent sqsEvent, ILambdaContext context)
        {
            SSMParameterManager ssmParameterManager = new SSMParameterManager();
            string storageConnectionString;

            if (!ssmParameterManager.TryGetValue(Constants.StorageConnectionStringSSMPath, out storageConnectionString))
            {
                throw new Exception("Storage connection path not defined.");
            }
            else
                Console.WriteLine($"Storage connection path is set at '{storageConnectionString}'");

            if (!S3Manager.CheckS3Parameters())
                return;

            // Check whether the connection string can be parsed.
            if (CloudStorageAccount.TryParse(storageConnectionString, out storageAccount))
            {

                foreach (var record in sqsEvent.Records)
                {
                    if (string.IsNullOrEmpty(record.Body))
                        continue;

                    var copyItem = JsonConvert.DeserializeObject<CopyItem>(record.Body);

                    if (copyItem == null || copyItem.BlobItem == null)
                        continue;

                    Console.WriteLine($"Trying to download item {copyItem.BlobItem.BlobName} from Azure blob storage to S3.");




                }

            }
            else
                throw new Exception("Azure storage account not properly configured.");
        }
    }
}
