using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
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
using System.Text;

namespace TConnectApi.Controllers
{
    [Route("reset-password")]
    public class ResetPasswordController : ApiController
    {
        // POST: api/ResetPassword
        public async Task<object> Post(HttpRequestMessage requestMessage, [FromBody]JObject request)
        {

            var messageId = requestMessage.GetCorrelationId().ToString();
            TraceSources.AddAdditionalLogItem("RequestMessageId", messageId);
            var emailAddress = "";
            try
            {
                var personReq = GetRequestJsosn_PersonForRemindPass(request.ToString());
                var companyReq = GetRequestJson_CompanyForRemindPass(request.ToString());

                var person = personReq.person;
                var company = companyReq.company;

                if (person != null)
                {
                    if (person.member_id == null || person.birthday == null || person.email == null)
                    {
                        PCSiteTraceSource.InvalidRequest_Api("api/reset-password", "必須要素が存在しない", null);
                        return requestMessage.CreateResponse(HttpStatusCode.BadRequest);
                    }
                    var memberId = person.member_id;
                    var strBirth = person.birthday;
                    emailAddress = person.email;

                    if (memberId.Equals(string.Empty) || memberId.Length > 256)
                    {
                        PCSiteTraceSource.InvalidRequest_Api("api/reset-password", "会員ID", null);
                        return requestMessage.CreateResponse(HttpStatusCode.BadRequest);
                    }

                    //入力チェック
                    if (emailAddress.Equals(string.Empty) || emailAddress.Length > 256)
                    {
                        PCSiteTraceSource.InvalidRequest_Api("api/reset-password", "メールアドレス", null);
                        return requestMessage.CreateResponse(HttpStatusCode.BadRequest);
                    }

                    try
                    {
                        DateTime.ParseExact(strBirth, "yyyyMMdd", null);
                    }
                    catch (Exception)
                    {
                        PCSiteTraceSource.InvalidRequest_Api("api/reset-password", "誕生日", null);
                        return requestMessage.CreateResponse(HttpStatusCode.BadRequest);
                    }

                    //個人の場合I003000213 会員ログイン情報取得を呼びだす
                    var reminder = new Toyota.Gbook.WebSite.Authentication.Control.Reminder();
                    //パスワード変更の場合はT-ConnectIdを指定しているため取得できうる内部会員IDは常に1件のみ。
                    var memberInfoList = reminder.CheckPersonalMemberExist(memberId, strBirth, emailAddress)[0];

                    var token = StoreVerificationData(emailAddress, memberInfoList.InternalMemberId, ReminderConstants.IsPersonalMember);

                    await MailSend(new MailAddress(emailAddress), token);

                    return requestMessage.CreateResponse(HttpStatusCode.Accepted,
                        new SuccessResponse_MessageId { message_id = messageId });

                }
                else if (company != null)
                {
                    if (company.member_id == null || company.frame_no == null || company.email == null)
                    {
                        PCSiteTraceSource.InvalidRequest_Api("api/reset-password", "必須要素が存在しない", null);
                        return requestMessage.CreateResponse(HttpStatusCode.BadRequest);
                    }
                    var memberId = company.member_id;
                    emailAddress = company.email;
                    var vin = company.frame_no;

                    if (memberId.Equals(string.Empty) || memberId.Length > 256)
                    {
                        PCSiteTraceSource.InvalidRequest_Api("api/reset-password", "会員ID", null);
                        return requestMessage.CreateResponse(HttpStatusCode.BadRequest);
                    }

                    if (emailAddress.Equals(string.Empty)  || emailAddress.Length > 256)
                    {
                        PCSiteTraceSource.InvalidRequest_Api("api/reset-password", "メールアドレス", null);
                        return requestMessage.CreateResponse(HttpStatusCode.BadRequest);
                    }

                    if (vin.Equals(string.Empty) || vin.Length > 20)
                    {
                        PCSiteTraceSource.InvalidRequest_Api("api/reset-password", "VIN", null);
                        return requestMessage.CreateResponse(HttpStatusCode.BadRequest);
                    }

                    //法人の場合00901：テレマサービス汎用操作API.テレマサービス契約情報取得を呼びだす
                    var reminder = new Toyota.Gbook.WebSite.Authentication.Control.Reminder();
                    var memberInfo = reminder.CheckCompanyMemberExist(memberId, emailAddress, vin);

                    var token = StoreVerificationData(emailAddress, memberInfo.InternalMemberId, ReminderConstants.IsCompanyMember);

                    await MailSend(new MailAddress(emailAddress), token);

                    return requestMessage.CreateResponse(HttpStatusCode.Accepted,
                    new SuccessResponse_MessageId { message_id = messageId });
                }

                return requestMessage.CreateResponse(HttpStatusCode.BadRequest);
            }
            catch (InvalidJsonException)
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
            catch(MissingFileOnBlobException mex)
            {
                var ex = PCSiteTraceSource.FileMissingOnBlob_Api(mex.FileName, mex);
                var id = 0;
                ex.TryGetId(out id);
                return requestMessage.CreateResponse(HttpStatusCode.InternalServerError,
                    new ErrorResponse { Errors = new Dictionary<string, string> { { "code", id.ToString() } } });
            }
            catch (UnexpectedResultCodeException uex)
            {
                var ex = PCSiteTraceSource.UnexpectedResultCode_Api(uex.api, uex.resultCode);
                var id = 0;
                ex.TryGetId(out id);
                return requestMessage.CreateResponse(HttpStatusCode.InternalServerError,
                    new ErrorResponse { Errors = new Dictionary<string, string> { { "code", id.ToString() } } });
            }
            catch(GetMemberLoginInfoFailException gex)
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
            catch(Exception e)
            {
                var ex = PCSiteTraceSource.SystemError_Api("パスワードリセットAPIで想定外のエラーが発生", e);
                var id = 0;
                ex.TryGetId(out id);
                return requestMessage.CreateResponse(HttpStatusCode.InternalServerError,
                    new ErrorResponse { Errors = new Dictionary<string, string> { { "code", id.ToString() } } });
            }
        }

        public static CompanyRequest GetRequestJson_CompanyForRemindPass(string request)
        {
            try
            {
                var companyReq = JsonConvert.DeserializeObject<CompanyRequest>(request);
                return companyReq;
            }
            catch (JsonReaderException jex)
            {
                PCSiteTraceSource.InvalidRequest_Api("api/reset-password", request, jex);
                throw new InvalidJsonException();
            }
            catch (JsonSerializationException jex)
            {
                PCSiteTraceSource.InvalidRequest_Api("api/reset-password", request, jex);
                throw new InvalidJsonException();
            }         
        }

        public static PersonRequest GetRequestJsosn_PersonForRemindPass(string request)
        {
            try
            {
                var personReq = JsonConvert.DeserializeObject<PersonRequest>(request);
                return personReq;
            }
            catch (JsonReaderException jex)
            {
                PCSiteTraceSource.InvalidRequest_Api("api/reset-password", request, jex);
                throw new InvalidJsonException();
            }
            catch (JsonSerializationException jex)
            {
                PCSiteTraceSource.InvalidRequest_Api("api/reset-password", request, jex);
                throw new InvalidJsonException();
            } 
            
        }

        public string StoreVerificationData(string mailAddress, string internalMemnerId, string memberDivision)
        {
            try
            {
                PCSiteTraceSource.MethodStart();
                var tokenGenerator = new TokenGenetator();
                var token = tokenGenerator.GenerateToken(internalMemnerId, new MailAddress(mailAddress));
                var azureTableOperator = new AzureTableOperation();
                azureTableOperator.StoreEmailAddressIfNeed(internalMemnerId, new MailAddress(mailAddress));
                azureTableOperator.StorePasswordResetRequest(internalMemnerId, memberDivision);
                return token;

            }
            catch(TableNotFoundException tex)
            {
                PCSiteTraceSource.ConnectionFailToAzureStorageTable("VerifiedEmailテーブル、またはStoredTokenテーブル", tex);
                throw;
            }
        }

        internal async Task MailSend(MailAddress mailAddress, string token)
        {
            PCSiteTraceSource.MethodStart();
            try
            {
                var mailsender = new VerificationMailSender(new MailTemplateGateway(
                new Blob(
                    Config.Item["Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString"],
                    Config.Item["Toyota.Gbook.WebSite.MailTemplate.ContainerName"],
                    RetryPolicies.NoRetry())));
                await mailsender.SendPasswordReminderMail(mailAddress, token);
            }
            catch (Exception ex)
            {
                PCSiteTraceSource.AppError("メールGWを利用してのメール送信中に例外が発生しました。", ex);
                throw;
            }
        }

        public class PersonRequest
        {
            public Person person { get; set; }
        }

        public class Person
        {
            public string member_id { get; set; }
            public string email { get; set; }
            public string birthday { get; set; }
        }

        public class CompanyRequest
        {
            public Company company { get; set; }
        }
        public class Company
        {
            public string member_id { get; set; }
            public string email { get; set; }
            public string frame_no { get; set; }
        }
    }
}
