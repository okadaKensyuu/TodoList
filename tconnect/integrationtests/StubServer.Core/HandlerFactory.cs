using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StubServer.Core
{
    public class HandlerFactory : IHttpHandlerFactory
    {
        public IHttpHandler GetHandler(HttpContext context, string requestType, string url, string pathTranslated)
        {
            return new Handler();
        }

        public void ReleaseHandler(IHttpHandler handler)
        {
            
        }
    }
}