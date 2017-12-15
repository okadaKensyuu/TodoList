using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using LangExt;

namespace StubServer.Core.Tests
{
    [TestFixture]
    class FindFilePath
    {
        [TestCase("http://localhost/stub/piyo", "piyo")]
        [TestCase("http://localhost/stub/piyo/hoge", "piyo/hoge")]
        [TestCase("http://localhost/stub/piyo/hoge/foo.aspx", "piyo/hoge/foo.aspx")]
        [TestCase("http://localhost/stub/piyo/hoge/foo.aspx?aaa=bbb", "piyo/hoge/foo.aspx")]
        public void URLからファイルを解決できること(string url, string expected)
        {
            var actual = Handler.ResolveFilePath(new Uri(url));
            Assert.That(actual, Is.EqualTo(expected));
        }       
    }

    [TestFixture]
    class GetResponse
    {
        readonly string xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<ResultAppRelation xmlns:i=""http://www.w3.org/2001/XMLSchema-instance"">
  <ResultCode>000000</ResultCode>
  <Authentication i:nil=""true""/>
  <RelationStatus>1</RelationStatus>
  <InternalMemberId>123456</InternalMemberId>
</ResultAppRelation>";

        [SetUp]
        public void SetUp()
        {

            Handler.ResponseReposiroty = new MemoryReposiroty(@"200

Content-Type : application/xml

" + xml);
        }

        [Test]
        public void リクエストからレスポンスを生成できること()
        {
            Uri uri = new Uri("http://localhost/accountservice/hoge/piyo");

            var headers = new List<Tuple<string, string>>()
            {
                new Tuple<string, string>("Content-Type", "application/xml"),
            };

            var expected = new Response(200, headers, xml);

            var actual = Handler.GetStubResponse(Handler.ResolveFilePath(uri), null);
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void リクエストから一覧を選択して指定のレスポンスを生成できること()
        {
            Uri uri = new Uri("http://localhost/accountservice/hoge/piyo");

            var headers = new List<Tuple<string, string>>()
            {
                new Tuple<string, string>("Content-Type", "application/xml"),
            };

            var expected = new Response(200, headers, xml);

            var actual = Handler.GetStubResponse(Handler.ResolveFilePath(uri), null);
            Assert.That(actual, Is.EqualTo(expected));
        }
    }
}
