using System;
using System.Collections.Generic;
using System.Text;

namespace Shared
{
    public class Constants
    {
        public static string StorageConnectionStringSSMPath = "/AzureBlobToS3/StorageConnection";

        public static string StorageContainerNamesSSMPath = "/AzureBlobToS3/StorageContainerNames";

        public static string S3SyncBucketNameSSMPath = "/AzureBlobToS3/S3SyncBucketName";

        public static string S3SyncRegionSSMPath = "/AzureBlobToS3/S3SyncRegion";
    }
}
