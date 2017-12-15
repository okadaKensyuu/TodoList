using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TConnectApi.Controllers
{
    public class ErrorResponse
    {
        [JsonProperty("errors")]
        public IDictionary<string, string> Errors { get; set; }
    }
}