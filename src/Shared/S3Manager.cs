using Amazon.S3.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shared
{
    public class S3Manager
    {

        static SSMParameterManager ssmParameterManager = new SSMParameterManager();

        static string S3BucketName;

        static string S3Region;

        /// <summary>
        /// Returns true if the SSM parameters for S3 have been set.
        /// </summary>
        /// <returns></returns>
        public static bool CheckS3Parameters()
        {

            if (!ssmParameterManager.TryGetValue(Constants.S3SyncBucketNameSSMPath, out S3BucketName))
            {
                Console.WriteLine("S3 bucket name not defined.");
                return false;
            }
            else
                Console.WriteLine($"S3 bucket name is set at '{S3BucketName}'");

            if (!ssmParameterManager.TryGetValue(Constants.S3SyncRegionSSMPath, out S3Region))
            {
                Console.WriteLine("S3 region not defined.");
                return false;
            }
            else
                Console.WriteLine($"S3 region is set at '{S3Region}'");

            return true;
        }

        public static List<S3Object> GetS3Items()
        {
            return GetS3Items(S3BucketName);
        }

        static List<S3Object> GetS3Items(string S3BucketName)
        {

            Amazon.S3.AmazonS3Client s3Client = new Amazon.S3.AmazonS3Client(Amazon.RegionEndpoint.GetBySystemName(S3Region));
            List<S3Object> lstResults = new List<S3Object>();

            string nextMarker = null;
            bool isTruncated = false;
            do
            {


                var result = s3Client.ListObjectsAsync(new ListObjectsRequest { Marker = nextMarker, BucketName = S3BucketName }).GetAwaiter().GetResult();
                lstResults.AddRange(result.S3Objects);

                isTruncated = result.IsTruncated;
                nextMarker = result.NextMarker;

            } while (isTruncated);

            return lstResults;
        }

    }
}
