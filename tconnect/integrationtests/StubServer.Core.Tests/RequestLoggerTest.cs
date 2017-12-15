using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace StubServer.Core.Tests
{
    [TestFixture]
    public class RequestLoggerTest
    {
        [Test]
        public void リクエストからログ文字列に変換ができる()
        {
            var expected = @"POST http://hoge/piyo HTTP /1.1
Host: hoge
Connection: Keep-Alive

<request></request>";

            var headers = new NameValueCollection();
            headers.Add("Host", "hoge");
            headers.Add("Connection", "Keep-Alive");
            Assert.That(RequestLogger.ToString("POST", new Uri("http://hoge/piyo"), headers, @"<request></request>"), Is.EqualTo(expected));

        }
    }
}
