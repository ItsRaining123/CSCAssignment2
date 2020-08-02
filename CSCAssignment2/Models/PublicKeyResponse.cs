using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace CSCAssignment2.Models
{
    public class PublicKeyResponse
    {
        [JsonProperty("publicKey")]
        public string PublicKey { get; set; }
    }
}
