using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Gbook.Base.Configuration;
using Gbook.Base.IO.WindowsAzure;
using Gbook.Base.TransientFaultHandling;
using Toyota.Gbook.WebSite.BlobGateway;
using Toyota.Gbook.WebSite.Common.Log;
using Toyota.Gbook.WebSite.Security.Exception;
using Toyota.Gbook.WebSite.VerificationHelper;
using Gbook.Base.Diagnostics;

namespace TConnectApi.Controllers
{
    [Route("remind-id")]
    public class RemindIdController : ApiController
    {
        // POST: api/RemindId
        public async Task<object> Post(HttpRequestMessage requestMessage, [FromBody]JObject request)
        {

            var emailAddress = "";
           try
            {
               var messageId = requestMessage.GetCorrelationId().ToString();
               TraceSources.AddAdditionalLogItem("RequestMessageId", messageId);
               var personReq = GetRequestJson_PersonForRemindId(request.ToString());
               var companyReq = GetRequestJson_CompanyForRemindId(request.ToString());

                var person = personReq.person;
                var company = companyReq.company;

                if (person != null)
                {
                    if(person.birthday == null || person.email == null)
                    {
                        PCSiteTraceSource.InvalidRequest_Api("remind-id", "必須要素が存在しない", null);
                        return requestMessage.CreateResponse(HttpStatusCode.BadRequest);
                    }

                    var strBirth = person.birthday;
                    emailAddress = person.email;

                    //入力チェック
                    if (emailAddress.Equals(string.Empty) || emailAddress.Length > 256)
                    {
                        PCSiteTraceSource.InvalidRequest_Api("remind-id", "メールアドレス", null);
                        return requestMessage.CreateResponse(HttpStatusCode.BadRequest);
                    }

                    try
                    {
                        DateTime.ParseExact(strBirth, "yyyyMMdd", null);
                    }
                    catch (Exception)
                    {
                        PCSiteTraceSource.InvalidRequest_Api("remind-id", "誕生日", null);
                        return requestMessage.CreateResponse(HttpStatusCode.BadRequest);
                    }

                    //個人の場合I003000213 会員ログイン情報取得を呼びだす
                    var reminder = new Toyota.Gbook.WebSite.Authentication.Control.Reminder();
                    var memberInfoList = reminder.CheckPersonalMemberExist("", strBirth, emailAddress);

                    var validMemberList = memberInfoList.Where(member => reminder.IsToyotaTConnectMember(member.InternalMemberId));

                    var encoding = Encoding.GetEncoding("UTF-8");
                    var emailBytes = encoding.GetBytes(emailAddress);
                    var emailBase64 = System.Convert.ToBase64String(emailBytes);

                    if (validMemberList.Count() == 0)
                    {
                        var ex = PCSiteTraceSource.UserNotFound_Api(emailBase64);
                        var id = 0;
                        ex.TryGetId(out id);
                        return requestMessage.CreateResponse(HttpStatusCode.Unauthorized,
                            new ErrorResponse { Errors = new Dictionary<string, string> { { "code", id.ToString() } } });                       
                    }

                   var memberList = new List<string> { };
                    foreach(var m in validMemberList)
                    {
                        memberList.Add(m.LoginId);
                    }

                    await MailSend(new MailAddress(emailAddress), memberList);

                    return requestMessage.CreateResponse(HttpStatusCode.Accepted,
                        new SuccessResponse_MessageId { message_id = messageId });

                }
                else if (company != null)
                {
                    if (company.frame_no == null || company.email == null)
                    {
                        PCSiteTraceSource.InvalidRequest_Api("remind-id", "必須要素が存在しない", null);
                        return requestMessage.CreateResponse(HttpStatusCode.BadRequest);
                    }

                    emailAddress = company.email;
                    var vin = company.frame_no;

                    if (emailAddress.Equals(string.Empty) || emailAddress.Length > 256)
                    {
                        PCSiteTraceSource.InvalidRequest_Api("remind-id", "メールアドレス", null);
                        return requestMessage.CreateResponse(HttpStatusCode.BadRequest);
                    }

                    if (vin.Equals(string.Empty) || vin.Length > 20)
                    {
                        PCSiteTraceSource.InvalidRequest_Api("remind-id", "VIN", null);
                        return requestMessage.CreateResponse(HttpStatusCode.BadRequest);
                    }

                    //法人の場合00901：テレマサービス汎用操作API.テレマサービス契約情報取得を呼びだす
                    var reminder = new Toyota.Gbook.WebSite.Authentication.Control.Reminder();
                    var memberInfo = reminder.CheckCompanyMemberExist(null, emailAddress, vin);

                    var encoding = Encoding.GetEncoding("UTF-8");
                    var emailBytes = encoding.GetBytes(emailAddress);
                    var emailBase64 = System.Convert.ToBase64String(emailBytes);

                    if (!reminder.IsToyotaTConnectMember(memberInfo.InternalMemberId))
                    {
                        var ex = PCSiteTraceSource.UserNotFound_Api(emailBase64);
                        var id = 0;
                        ex.TryGetId(out id);
                        return requestMessage.CreateResponse(HttpStatusCode.Unauthorized,
                            new ErrorResponse { Errors = new Dictionary<string, string> { { "code", id.ToString() } } });  
                    }

                    var internalMemberIdList = new List<string> { memberInfo.LoginId };
                    await MailSend(new MailAddress(emailAddress), internalMemberIdList);

                    return requestMessage.CreateResponse(HttpStatusCode.Accepted,
                        new SuccessResponse_MessageId { message_id = messageId });
                }

                PCSiteTraceSource.InvalidRequest_Api("remind-id", "リクエストJson", null);
                return requestMessage.CreateResponse(HttpStatusCode.BadRequest);
            }
            catch(InvalidJsonException)
           {
               return requestMessage.CreateResponse(HttpStatusCode.BadRequest);
           }
            catch (UserNotFoundException)
            {
                var encoding = Encoding.GetEncoding("UTF-8");
                var emailBytes = encoding.GetBytes(emailAddress);
                var emailBase64 = System.Convert.ToBase64String(emailBytes);
                var ex = PCSiteTraceSource.UserNotFound_Api(emailBase64);
                var id = 0;
                ex.TryGetId(out id);
                return requestMessage.CreateResponse(HttpStatusCode.Unauthorized,
                    new ErrorResponse { Errors = new Dictionary<string, string> { { "code", id.ToString() } } });
            }
            catch (UnexpectedMemberDivisionException)
            {
                var encoding = Encoding.GetEncoding("UTF-8");
                var emailBytes = encoding.GetBytes(emailAddress);
                var emailBase64 = System.Convert.ToBase64String(emailBytes);
                var ex = PCSiteTraceSource.UnexpectedMemberDivision_Api(emailBase64);
                var id = 0;
                ex.TryGetId(out id);
                return requestMessage.CreateResponse(HttpStatusCode.Unauthorized,
                    new ErrorResponse { Errors = new Dictionary<string, string> { { "code", id.ToString() } } });
            }
           catch (MissingFileOnBlobException mex)
           {
               var ex = PCSiteTraceSource.FileMissingOnBlob_Api(mex.FileName, mex);
               var id = 0;
               ex.TryGetId(out id);
               return requestMessage.CreateResponse(HttpStatusCode.InternalServerError,
                   new ErrorResponse { Errors = new Dictionary<string, string> { { "code", id.ToString() } } });
           }
           catch(UnexpectedResultCodeException uex)
           {
               var ex = PCSiteTraceSource.UnexpectedResultCode_Api(uex.api, uex.resultCode);
               var id = 0;
               ex.TryGetId(out id);
               return requestMessage.CreateResponse(HttpStatusCode.InternalServerError,
                   new ErrorResponse { Errors = new Dictionary<string, string> { { "code", id.ToString() } } });
           }
           catch (GetMemberLoginInfoFailException gex)
           {
               var ex = PCSiteTraceSource.NetworkAccessFail_Api("00000_common/Member.svc/rest/GetMemberLoginInformation", gex);
               var id = 0;
               ex.TryGetId(out id);
               return requestMessage.CreateResponse(HttpStatusCode.InternalServerError,
                   new ErrorResponse { Errors = new Dictionary<string, string> { { "code", id.ToString() } } });

           }
           catch (TelemaServiceGeneralOperationFailException tex)
           {
               var ex = PCSiteTraceSource.NetworkAccessFail_Api("00000_common/TelemaServiceGeneralOperation.sv/rest/GetTelemaServiceContract", tex);
               var id = 0;
               ex.TryGetId(out id);
               return requestMessage.CreateResponse(HttpStatusCode.InternalServerError,
                   new ErrorResponse { Errors = new Dictionary<string, string> { { "code", id.ToString() } } });
           }
           catch (WebApplicationFatalException wex)
           {
               var ex = PCSiteTraceSource.MethodFailure(string.Format("パスワードリセットAPIでエラーが発生。URL{0}, レスポンス：{1}", wex.Url, wex.Response));
               var id = 0;
               ex.TryGetId(out id);
               return requestMessage.CreateResponse(HttpStatusCode.InternalServerError,
                   new ErrorResponse { Errors = new Dictionary<string, string> { { "code", id.ToString() } } });
           }
            catch (Exception e)
            {
                var ex = PCSiteTraceSource.SystemError_Api("APIで想定外のエラーが発生", e);
                var id = 0;
                ex.TryGetId(out id);
                return requestMessage.CreateResponse(HttpStatusCode.InternalServerError,
                    new ErrorResponse { Errors = new Dictionary<string, string> { { "code", id.ToString() } } });
            }
        }

        public static CompanyRequest GetRequestJson_CompanyForRemindId(string request)
        {
            try
            {
                var companyReq = JsonConvert.DeserializeObject<CompanyRequest>(request);
                return companyReq;
            }
            catch (JsonReaderException jex)
            {
                PCSiteTraceSource.InvalidRequest_Api("api/remind-id", request, jex);
                throw new InvalidJsonException();
            }
            catch (JsonSerializationException jex)
            {
                PCSiteTraceSource.InvalidRequest_Api("api/remind-id", request, jex);
                throw new InvalidJsonException();
            } 
        }

        public static PersonRequest GetRequestJson_PersonForRemindId(string request)
        {
            try
            {
                var personReq = JsonConvert.DeserializeObject<PersonRequest>(request);
                return personReq;
            }
            catch (JsonReaderException jex)
            {
                PCSiteTraceSource.InvalidRequest_Api("api/remind-id", request, jex);
                throw new InvalidJsonException();
            }
            catch (JsonSerializationException jex)
            {
                PCSiteTraceSource.InvalidRequest_Api("api/remind-id", request, jex);
                throw new InvalidJsonException();
            } 
        }


        internal async Task MailSend(MailAddress mailAddress, List<string> internalMemberId)
        {
            PCSiteTraceSource.MethodStart();
            try
            {
                var url = Config.Get<string>("Toyota.Gbook.WebSite.Mail.MailGW.Url");
                var subject = Config.Get<string>("Toyota.Gbook.WebSite.IdRemind.Subject");
                var mailsender = new VerificationMailSender(new MailTemplateGateway(
                new Blob(
                    Config.Item["Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString"],
                    Config.Item["Toyota.Gbook.WebSite.MailTemplate.ContainerName"],
                    RetryPolicies.NoRetry())));
                await mailsender.SendIdReminderMail(mailAddress, internalMemberId);
            }
            catch (Exception ex)
            {
                PCSiteTraceSource.AppError("メールGWを利用してのメール送信中に例外が発生しました。", ex);
            }
        }

        public class PersonRequest
        {
            public Person person { get; set; }
        }

        public class Person
        {
            public string email { get; set; }
            public string birthday { get; set; }
        }

        public class CompanyRequest
        {
            public Company company { get; set; }
        }
        public class Company
        {
            public string email { get; set; }
            public string frame_no { get; set; }
        }
    }
}
