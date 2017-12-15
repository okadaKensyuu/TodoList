using Gbook.Base.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using Toyota.Gbook.WebSite.Common.Log;

namespace TConnectApi
{
    public class ApiSecurity
    {
        public bool XidXpassFilter(HttpContext context)
        {
            var pathConfig = Config.Item["Toyota.Gbook.WebSite.TConnectApi.XidPassFilter"];
            var headers = context.Request.Headers;
            //Hack: 大文字小文字区別するでいいかどうか確認(x-idやX-idなどを許可しない方針でよいか)
            //現状は13MMのISAPIフィルターの実装と同じになっている。
            var xid = headers.GetValues("X-ID");
            var xpass = headers.GetValues("X-Password");

            try
            {
                var pairOfIdPass = xid[0] + "," + xpass[0];
                var allowList = System.IO.File.ReadAllLines(GetPath(pathConfig))
                    .Select(_ => _.Trim())
                    .Where(_ => !string.IsNullOrWhiteSpace(_));

                if(allowList.Contains(pairOfIdPass))
                {
                    return true;
                }
                else
                {
                    PCSiteTraceSource.InvalidAccess_Api(context.Request.RawUrl, null);
                    return false;
                }
            }
            catch(NullReferenceException)
            {
                PCSiteTraceSource.SettingKeyMissing("Toyota.Gbook.WebSite.TConnectApi.XidPassFilter");
                return false;
            }
        }

        private string GetPath(string pathConfig)
        {
            var path = pathConfig;
            if (VirtualPathUtility.IsAppRelative(path))
                path = HostingEnvironment.MapPath(path);

            return System.IO.Path.GetFullPath(path);
        }
    }
}