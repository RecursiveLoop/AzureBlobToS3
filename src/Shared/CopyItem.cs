using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Shared
{
    [DataContract]
    public class CopyItem
    {
        [DataMember] public AzureBlobItem BlobItem { get; set; }

       
    }
}
