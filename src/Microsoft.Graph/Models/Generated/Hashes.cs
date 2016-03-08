// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

// **NOTE** This file was generated by a tool and any changes will be overwritten.


namespace Microsoft.Graph
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.Serialization;
    using Newtonsoft.Json;

    /// <summary>
    /// The type Hashes.
    /// </summary>
    [DataContract]
    [JsonConverter(typeof(DerivedTypeConverter))]
    public partial class Hashes
    {
    
        /// <summary>
        /// Gets or sets crc32Hash.
        /// </summary>
        [DataMember(Name = "crc32Hash", EmitDefaultValue = false, IsRequired = false)]
        public string Crc32Hash { get; set; }
    
        /// <summary>
        /// Gets or sets sha1Hash.
        /// </summary>
        [DataMember(Name = "sha1Hash", EmitDefaultValue = false, IsRequired = false)]
        public string Sha1Hash { get; set; }
    
        /// <summary>
        /// Gets or sets additional data.
        /// </summary>
        [JsonExtensionData(ReadData = true)]
        public IDictionary<string, object> AdditionalData { get; set; }
    
    }
}