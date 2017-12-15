using Gbook.Base.Configuration;
using Gbook.Base.Configuration.Internals;
using Gbook.Base.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Routing;
using Toyota.Gbook.WebSite.Common.Log;

namespace TConnectApi
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);
            Gbook.Base.Configuration.Internals.ConfigSetter.SetItem(new Gbook.Base.Configuration.DefaultConfig());
        }

        protected void Application_BeginRequest(Object sender, EventArgs e)
        {
            TraceSources.AddAdditionalLogItem("RequsetId", Guid.NewGuid());
            string AccessURI = HttpContext.Current.Request.Url.PathAndQuery;
            PCSiteTraceSource.CheckPoint("リクエストを受信しました。AccessURI： " + AccessURI);

            var context = HttpContext.Current;
            var security = new ApiSecurity();
            if(!security.XidXpassFilter(context))
            {
                context.Response.StatusCode = 404;
            }

        }

        protected void Application_EndRequest(Object sender, EventArgs e)
        {
            string AccessURI = HttpContext.Current.Request.Url.PathAndQuery;
            PCSiteTraceSource.CheckPoint("レスポンスを送出しました。AccessURI： " + AccessURI);
        }

        protected void Application_Error(Object sender, EventArgs e)
        {
            // 例外オブジェクトを取得
            Exception ex = Server.GetLastError();
            if (ex is System.Web.HttpUnhandledException)
            {
                ex = ex.InnerException;
            }
            // パブリッシャに例外の出力を出力
            PCSiteTraceSource.SystemError("APIで想定外のエラーが発生しました。",ex);
        }
    }
}
