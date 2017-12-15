using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using System.Xml;
using Newtonsoft.Json.Linq;
using Gbook.Base.Diagnostics;
using Toyota.Gbook.WebSite.Common.Log;
using Gbook.Base.Configuration;

namespace TConnectApi.Controllers
{
    [RoutePrefix("contact")]
    public class ContactController : ApiController
    {
        public async Task<object> Post(HttpRequestMessage requestMessage, [FromBody]JObject request)
        {
            var messageId = requestMessage.GetCorrelationId().ToString();
            TraceSources.AddAdditionalLogItem("RequestMessageId", messageId);
            try
            {
                CheckRequest(request);
                var requestXml = JsonConvert.DeserializeXmlNode(request.ToString(), "contact");
                var xmlDeclaration = requestXml.CreateXmlDeclaration("1.0", "UTF-8", null);
                using (var stringwriter = new StringWriter())
                using (var xmltextWriter = new XmlTextWriter(stringwriter))
                {
                    xmlDeclaration.WriteTo(xmltextWriter);
                    requestXml.WriteTo(xmltextWriter);
                    var requestBodyXml = stringwriter.ToString();
                    var url = new Uri(Config.Get<string>("Toyota.Gbook.WebSite.Contact.MemberUtilityApi.Url"));
                    using (var client = new HttpClient())
                    using (var m = new HttpRequestMessage
                    {
                        Method = HttpMethod.Post,
                        RequestUri = url,
                        Content = new StringContent(requestBodyXml)
                    })
                    {
                        m.Headers.Add("MessageId", messageId);

                        var xid = Config.Get<string>("Toyota.Gbook.WebSite.TConnect.XId");
                        var xpass = Config.Get<string>("Toyota.Gbook.WebSite.TConnect.XPassword");
                        m.Headers.Add("X-ID", xid);
                        m.Headers.Add("X-Password", xpass);
                        PCSiteTraceSource.ApiExecute_Api("問合せAPI(オンプレ)", url.ToString(), requestBodyXml);
                        var response = await client.SendAsync(m);
                        var responseXml = await response.Content.ReadAsStringAsync();
                        PCSiteTraceSource.ApiResponsed_Api("問合せAPI(オンプレ)", response.StatusCode, responseXml);

                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            var x = new XmlDocument();
                            x.LoadXml(responseXml);
                            var code = x.SelectSingleNode("//contact/result_code").InnerText;
                            var result_message = x.SelectSingleNode("//contact/result_code_message").InnerText;
                            if (code == "000000")
                            {
                                if (request["registered"]["user_id"] == null)
                                {
                                    return new SuccessResponse
                                    {
                                        UserId = "",
                                        Datetime = request["inquiry"]["input_date"].ToString() + request["inquiry"]["input_time"]
                                    };

                                }
                                else
                                {
                                    return new SuccessResponse
                                    {
                                        UserId = request["registered"]["user_id"].ToString(),
                                        Datetime = request["inquiry"]["input_date"].ToString() + request["inquiry"]["input_time"]
                                    };
                                }

                            }
                            if (code == "100002")
                            {
                                //入力チェックエラー
                                var trace = PCSiteTraceSource.UnexpectedResultCode_Api(url.ToString(), code);
                                var errorCode = 0;
                                trace.TryGetId(out errorCode);
                                return requestMessage.CreateResponse(HttpStatusCode.BadRequest);
                            }
                            if (code == "300000")
                            {
                                //DBアクセスエラー
                                var trace = PCSiteTraceSource.UnexpectedResultCode_Api(url.ToString(), code);
                                var errorCode = 0;
                                trace.TryGetId(out errorCode);
                                return requestMessage.CreateResponse(HttpStatusCode.InternalServerError,
                                    new ErrorResponse { Errors = new Dictionary<string, string> { { "code", errorCode.ToString() } } });
                            }

                            if (code == "500000")
                            {
                                var trace = PCSiteTraceSource.InvalidRequest_Api(url.ToString(), requestBodyXml, null);
                                var errorCode = 0;
                                trace.TryGetId(out errorCode);
                                return requestMessage.CreateResponse(HttpStatusCode.InternalServerError,
                                    new ErrorResponse { Errors = new Dictionary<string, string> { { "code", errorCode.ToString() } } });
                            }

                            if (code == "600000")
                            {
                                var trace = PCSiteTraceSource.ValueMissing_Api(url.ToString(), "処理結果コード");
                                var errorCode = 0;
                                trace.TryGetId(out errorCode);
                                return requestMessage.CreateResponse(HttpStatusCode.InternalServerError,
                                    new ErrorResponse { Errors = new Dictionary<string, string> { { "code", errorCode.ToString() } } });
                            }

                            if (code == "900000")
                            {
                                var trace = PCSiteTraceSource.SystemError_Api(string.Format("問合せAPI(オンプレ)で想定外のエラーが発生。メッセージ：", result_message), null);
                                var errorCode = 0;
                                trace.TryGetId(out errorCode);
                                return requestMessage.CreateResponse(HttpStatusCode.InternalServerError,
                                    new ErrorResponse { Errors = new Dictionary<string, string> { { "code", errorCode.ToString() } } });
                            }

                            var errorTrace = PCSiteTraceSource.UnexpectedResultCode(url.ToString(), code);
                            var unexpcetErrorCode = 0;
                            errorTrace.TryGetId(out unexpcetErrorCode);
                            return requestMessage.CreateResponse(HttpStatusCode.InternalServerError,
                                new ErrorResponse { Errors = new Dictionary<string, string> { { "code", unexpcetErrorCode.ToString() } } });
                        }
                        else
                        {
                            var trace = PCSiteTraceSource.UnexpectedHttpStatus_Api(url.ToString(), response.StatusCode, "");
                            var errorCode = 0;
                            trace.TryGetId(out errorCode);
                            return requestMessage.CreateResponse(HttpStatusCode.InternalServerError,
                                new ErrorResponse { Errors = new Dictionary<string, string> { { "code", errorCode.ToString() } } });
                        }
                    }
                }
            }
            catch (InvalidRequestForContactException ie)
            {
                PCSiteTraceSource.InvalidRequest_Api("tcmypage/api/contact", request.ToString(), ie);
                return requestMessage.CreateResponse(HttpStatusCode.BadRequest);
            }
            catch (Exception e)
            {
                var trace = PCSiteTraceSource.SystemError_Api("問合せAPIで想定外のエラーが発生", e);
                var code = 0;
                trace.TryGetId(out code);
                return requestMessage.CreateResponse(HttpStatusCode.InternalServerError,
                    new ErrorResponse { Errors = new Dictionary<string, string> { { "code", code.ToString() } } });
            }
        }

        private void CheckRequest(JObject request)
        {
            var onpreRequest = GetRequestJsonForContact(request.ToString());

            if (onpreRequest.registered.user_id == null || onpreRequest.registered.user_id.Length > 256)
            {
                throw new InvalidRequestForContactException("T-ConnectId");
            }

            if (string.IsNullOrEmpty(onpreRequest.registered.full_name) || onpreRequest.registered.full_name.Length > 25)
            {
                throw new InvalidRequestForContactException("氏名");
            }

            try
            {
                DateTime.ParseExact(onpreRequest.registered.birthday, "yyyyMMdd", null);
            }
            catch (Exception)
            {
                throw new InvalidRequestForContactException("生年月日");
            }

            if (string.IsNullOrEmpty(onpreRequest.registered.postal_code) || onpreRequest.registered.postal_code.Length != 7)
            {
                throw new InvalidRequestForContactException("郵便番号");
            }
            else
            {
                try
                {
                    Int32.Parse(onpreRequest.registered.postal_code);
                }
                catch (Exception)
                {
                    throw new InvalidRequestForContactException("郵便番号");
                }
            }

            if (string.IsNullOrEmpty(onpreRequest.registered.city))
            {
                throw new InvalidRequestForContactException("住所(都市区町)");
            }

            if (string.IsNullOrEmpty(onpreRequest.registered.address))
            {
                throw new InvalidRequestForContactException("住所(地区番地ビル名)");
            }

            if (string.IsNullOrEmpty(onpreRequest.registered.phone_number) || onpreRequest.registered.phone_number.Length > 13)
            {
                throw new InvalidRequestForContactException("電話番号");
            }

            if (onpreRequest.registered.email == null || onpreRequest.registered.email.Length > 256)
            {
                throw new InvalidRequestForContactException("メールアドレス");
            }

            if (onpreRequest.current == null || (onpreRequest.current.postal_code == "" && onpreRequest.current.city == "" &&
                onpreRequest.current.address== "" && onpreRequest.current.phone_number == ""))
            {
                //全てが空の場合はOKなので何もしない。
            }
            else
            {
                if (string.IsNullOrEmpty(onpreRequest.current.postal_code) || onpreRequest.current.postal_code.Length != 7)
                {
                    throw new InvalidRequestForContactException("(変更後)郵便番号");
                }
                else
                {
                    try
                    {
                        Int32.Parse(onpreRequest.current.postal_code);
                    }
                    catch (Exception)
                    {
                        throw new InvalidRequestForContactException("(変更後)郵便番号");
                    }
                }

                if (string.IsNullOrEmpty(onpreRequest.current.city))
                {
                    throw new InvalidRequestForContactException("(変更後)住所(都市区町)");
                }

                if (string.IsNullOrEmpty(onpreRequest.current.address))
                {
                    throw new InvalidRequestForContactException("(変更後)住所(地区番地ビル名)");
                }

                if (string.IsNullOrEmpty(onpreRequest.current.phone_number) || onpreRequest.current.phone_number.Length > 13)
                {
                    throw new InvalidRequestForContactException("(変更後)電話番号");
                }
            }

            try
            {
                DateTime.ParseExact(onpreRequest.inquiry.input_date, "yyyyMMdd", null);
            }
            catch (Exception)
            {
                throw new InvalidRequestForContactException("問い合わせ日付");
            }

            try
            {
                DateTime.ParseExact(onpreRequest.inquiry.input_time, "HHmmss", null);
            }
            catch (Exception)
            {
                throw new InvalidRequestForContactException("問い合わせ時間");
            }

            if (onpreRequest.inquiry.from != "1" && onpreRequest.inquiry.from != "2")
            {
                throw new InvalidRequestForContactException("サイト区分");
            }
        }

        public static Input_Onpre_Contact GetRequestJsonForContact(string request)
        {
            try
            {
                var onpreRequest = JsonConvert.DeserializeObject<Input_Onpre_Contact>(request);
                return onpreRequest;
            }
            catch(JsonReaderException jex)
            {
                PCSiteTraceSource.InvalidRequest_Api("api/contact",request, jex);
                throw new InvalidRequestForContactException("Jsonとして不正");
            }
            catch(JsonSerializationException jex)
            {
                PCSiteTraceSource.InvalidRequest_Api("api/contact", request, jex);
                throw new InvalidRequestForContactException("Jsonとして不正");
            }
        }
    }


    public sealed class Input_Onpre_Contact
    {
        [JsonProperty("registered")]
        public Registered registered { get; set; }

        [JsonProperty("current")]
        public Current current { get; set; }

        [JsonProperty("inquiry")]
        public Inquiry inquiry { get; set; }
    }

    public class SuccessResponse
    {
        [JsonProperty("user_id")]
        public string UserId { get; set; }
        [JsonProperty("datetime")]
        public string Datetime { get; set; }
    }

    public sealed class Registered
    {
        public string user_id { get; set; }
        public string full_name { get; set; }
        public string birthday { get; set; }
        public string postal_code { get; set; }
        public string city { get; set; }
        public string address { get; set; }
        public string phone_number { get; set; }
        public string email { get; set; }
    }

    public sealed class Current
    {
        public string postal_code { get; set; }
        public string city { get; set; }
        public string address { get; set; }
        public string phone_number { get; set; }
    }

    public sealed class Inquiry
    {
        public string input_date { get; set; }
        public string input_time { get; set; }
        public string from { get; set; }
    }
}
