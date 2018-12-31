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
using System.Text;
using Amazon.SQS.Model;
using Newtonsoft.Json;

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
        public void FunctionHandler(ILambdaContext context)
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
                string ContainerNames;
                if (ssmParameterManager.TryGetValue(Constants.StorageContainerNamesSSMPath, out ContainerNames))
                {
                    var arrContainerNames = ContainerNames.Split(',', StringSplitOptions.RemoveEmptyEntries);
                    List<Task> lstTasks = new List<Task>();
                    foreach (var ContainerName in arrContainerNames)
                    {
                        var t = Task.Factory.StartNew(() =>
                          {
                              var lstBlobItems = AzureManager.ListBlobContainer(storageAccount, ContainerName, null);

                              if (lstBlobItems.Count > 0)
                              {

                                  Console.WriteLine("Listing S3 items.");
                                  var S3Objects = S3Manager.GetS3Items();

                                  Console.WriteLine($"{S3Objects.Count.ToString()} items retrieved from S3.");

                                  foreach (var S3Object in S3Objects)
                                  {
                                      Console.WriteLine($"Item retrieved from S3: {S3Object.BucketName} - {S3Object.Key} ({S3Object.Size.ToString()} bytes) - ETag {S3Object.ETag}");
                                  }

                                  StringBuilder sb = new StringBuilder();

                                  Amazon.SQS.AmazonSQSClient sqsClient = new Amazon.SQS.AmazonSQSClient();


                                  lstBlobItems.ForEach((a) =>
                                  {
                                      if (!S3Objects.Any(s => s.Key == a.BlobName && s.Size == a.Size))
                                      {
                                          CopyItem copyItem = new CopyItem { BlobItem = a };
                                          // No S3 objects that match the Azure blob, copy it over
                                          var strCopyItem = JsonConvert.SerializeObject(copyItem);
                                          SendMessageRequest sendRequest = new SendMessageRequest { MessageBody = strCopyItem, QueueUrl = System.Environment.GetEnvironmentVariable("QueueName") };

                                          var sendMessageResult = sqsClient.SendMessageAsync(sendRequest).GetAwaiter().GetResult();

                                          Console.WriteLine($"Item not found in S3, adding to copy queue - {a.BlobName} - send message result: {sendMessageResult.HttpStatusCode.ToString()}");
                                      }

                                      
                                  });
                              }

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


    }
}
