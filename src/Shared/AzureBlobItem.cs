using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Shared
{
    [DataContract]
    public class AzureBlobItem
    {
        [DataMember]
        public string ContainerName { get; set; }
        [DataMember]
        public string BlobName { get; set; }
        [DataMember] public string MD5 { get; set; }
        [DataMember] public string ObjectTypeName { get; set; }
        [DataMember] public long Size { get; set; }
    }
}
