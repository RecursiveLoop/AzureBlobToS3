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
                              var lstBlobItems = AzureManager.ListBlobContainer(storageAccount,ContainerName, null);

                              if (lstBlobItems.Count > 0)
                              {

                                  StringBuilder sb = new StringBuilder();

                                  lstBlobItems.ForEach((a) =>
                                  {
                                     


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
