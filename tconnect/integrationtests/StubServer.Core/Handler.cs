using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LangExt;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Text;
using System.IO;

namespace StubServer.Core
{
    public class Handler : IHttpHandler
    {
        public bool IsReusable
        {
            get { return true; }
        }

        internal readonly Encoding encoding = Encoding.UTF8;

        public static IRepository ResponseReposiroty = new AzureReposiroty();

        internal string ReadRequestContent(HttpContextBase context)
        {
            using (var input = context.Request.InputStream)
            using (var inputReader = new StreamReader(input, encoding))
            {
                return inputReader.ReadToEnd();
            }
        }

        internal void BuildHttpResponse(HttpContextBase context, Response stubResponse)
        {
            var response = context.Response;

            response.StatusCode = stubResponse.StatusCode;

            foreach (Tuple<string, string> header in stubResponse.Headers)
            {
                response.AppendHeader(header.Item1, header.Item2);
            }

            using (var output = context.Response.OutputStream)
            using (var outputWriter = new StreamWriter(output, encoding))
            {
                outputWriter.Write(stubResponse.Content);
            }
        }

        public void ProcessRequest(HttpContextBase context)
        {
            string inputContent = ReadRequestContent(context);
            var filePath = ResolveFilePath(context.Request.Url);
            RequestLogger.Write(ResponseReposiroty, filePath, context.Request, inputContent);
            var stubResponse = GetStubResponse(filePath, inputContent);
            context.Response.TrySkipIisCustomErrors = true;
            BuildHttpResponse(context, stubResponse);
        }

        public void ProcessRequest(HttpContext context)
        {
            ProcessRequest(new HttpContextWrapper(context));
        }

        public static string ResolveFilePath(Uri uri)
        {
            return uri.AbsolutePath.Substring("stub/".Length + 1);
        }

        public static Response GetStubResponse(string filePath, string inputContent)
        {
            var response = from file in ResponseReposiroty.TryReadFile(filePath)
                               .OrElse( () =>
                                   ResponseReposiroty.TryReadFile(filePath + "/" + ResponseReposiroty.ListFiles(filePath).First(inputContent.Contains)))
                           select Response.Parse(file);
            return response.Match<Response>(x => x, () => Response.NotFound);
        }
    }
}