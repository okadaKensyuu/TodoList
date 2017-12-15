using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace StubServer.Core.Tests
{
    [TestFixture]
    class MakeResponse
    {
        [Test]
        public void 読み取ったファイルの内容からレスポンスを生成できること()
        {
            var testText = @"200

Content-Type : text/xml
hoge: hoge

<xml>
    <internalId>10</internalId>
</xml>";
            var headers = new List<Tuple<string, string>>()
            {
                new Tuple<string, string>("Content-Type", "text/xml"),
                new Tuple<string, string>("hoge", "hoge"),
            };

            var expected = new Response(200, headers, @"<xml>
    <internalId>10</internalId>
</xml>");
            var actual = Response.Parse(testText);

            Assert.That(actual, Is.EqualTo(expected));
        }
    }
}
