using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Toyota.Gbook.WebSite.TConnect.Reminder;
using Gbook.Base.Configuration;
using Gbook.Base.Diagnostics;
using Toyota.Gbook.WebSite.Common.Log;
using Toyota.Gbook.WebSite.VerificationHelper;
using Toyota.Gbook.WebSite.Security.Exception;

namespace TConnectApi.Controllers
{
    [RoutePrefix("is-expired-token")]
    public class IsExpiredTokenController : ApiController
    {
       // GET: api/IsExpiredToken/5
        [Route("")]
        [Route("{id}")]
        public object Get(HttpRequestMessage requestMessage, string id)
        {
            var messageId = requestMessage.GetCorrelationId().ToString();
            TraceSources.AddAdditionalLogItem("RequestMessageId", messageId);
            try
            {
                var passwordChange = new PasswordChange();
                var token = passwordChange.ExpirationCheck(id);
                return new IsExpiredTokenSuccessResponse
                {
                    Result = passwordChange.Result != PasswordChange.ValidatedResult.IsExpired && token != null
                };
            }
            catch (InvalidRequestForTConnectApiException e)
            {
                PCSiteTraceSource.InvalidRequest_Api("is-expired-token", "token", e);
                return requestMessage.CreateResponse(HttpStatusCode.BadRequest);
            }
            catch (InvalidJwtException ex)
            {
                var trace = PCSiteTraceSource.InvalidJwt(id, ex);
                var code = 0;
                trace.TryGetId(out code);
                return requestMessage.CreateResponse(HttpStatusCode.BadRequest,
                   new ErrorResponse { Errors = new Dictionary<string, string> { { "code", code.ToString() } } });

            }
            catch(TableNotFoundException tex)
            {
                var trace = PCSiteTraceSource.ConnectionFailToAzureStorageTable_Api("StredTokenテーブルにアクセスできませんでした。", tex);
                var code = 0;
                trace.TryGetId(out code);
                return requestMessage.CreateResponse(HttpStatusCode.InternalServerError,
                    new ErrorResponse { Errors = new Dictionary<string, string> { { "code", code.ToString() } } });
            }
            catch(Exception ex)
            {
                var trace = PCSiteTraceSource.SystemError_Api("有効期限検証APIで想定外のエラーが発生", ex);
                var code = 0;
                trace.TryGetId(out code);
                return requestMessage.CreateResponse(HttpStatusCode.InternalServerError,
                    new ErrorResponse { Errors = new Dictionary<string, string> { { "code", code.ToString() } } });
            }
        }  
    }

    public class IsExpiredTokenSuccessResponse
    {
        [JsonProperty("result")]
        public bool Result { get; set; }
    }
}
