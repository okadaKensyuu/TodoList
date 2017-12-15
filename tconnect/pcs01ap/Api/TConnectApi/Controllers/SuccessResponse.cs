using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TConnectApi.Controllers
{
    public class SuccessResponse_MessageId
    {
        [JsonProperty("message_id")]
        public string message_id { get; set; }
    }

}