using Gbook.Base.Configuration;
using Gbook.Base.IO;
using Gbook.Base.IO.WindowsAzure;
using Gbook.Base.TransientFaultHandling;
using Microsoft.WindowsAzure.Storage;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace StubServer.Core
{
    public static class RequestLogger
    {
        public static string ToString(string httpMethod, Uri url, NameValueCollection headers, string body)
        {
            return string.Format("{0} {1} HTTP /1.1\r\n{2}\r\n{3}", httpMethod, url, ToString(headers), body);
        }

        public static void Write(IRepository repository, string path, HttpRequestBase req, string inputContent)
        {
            var logReq = ToString(req.HttpMethod, req.Url, req.Headers, inputContent);
            repository.WriteFile(string.Format("{0}_{1}.log", path, DateTime.UtcNow.ToString("yyyyMMddHHmmss")),logReq);
        }

        static string ToString(NameValueCollection headers)
        {
            var result = new StringBuilder();
            foreach(var key in headers.AllKeys)
            foreach(var value in headers.GetValues(key))
            {
                result.AppendFormat("{0}: {1}\r\n", key, value);
            }
            return result.ToString();
        }
    }
}
