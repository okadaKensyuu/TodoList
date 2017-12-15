using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using LangExt;

namespace StubServer.Core.Tests
{
    [TestFixture]
    class GetResponseComplexly
    {
        readonly string fileAXml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<ResultAppRelationA xmlns:i=""http://www.w3.org/2001/XMLSchema-instance"">
  <ResultCode>000000</ResultCode>
  <Authentication i:nil=""true""/>
  <RelationStatus>1</RelationStatus>
  <InternalMemberId>123456</InternalMemberId>
</ResultAppRelationA>";

        readonly string fileBXml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<ResultAppRelationB xmlns:i=""http://www.w3.org/2001/XMLSchema-instance"">
  <ResultCode>000000</ResultCode>
  <Authentication i:nil=""true""/>
  <RelationStatus>1</RelationStatus>
  <InternalMemberId>123456</InternalMemberId>
</ResultAppRelationB>";

        [SetUp]
        public void SetUp()
        {
            var meta = @"200

Content-Type : application/xml

";
            var files = new Dictionary<string, string>()
            {
                { "FileA", meta + fileAXml },
                { "FileB", meta + fileBXml }
            };

            Handler.ResponseReposiroty = new MemoryFolderReposiroty(files);
        }

        [Test]
        public void リクエストから一覧を選択して指定のレスポンスを生成できること()
        {
            Uri uri = new Uri("http://localhost/accountservice/hoge/piyo");

            var headers = new List<Tuple<string, string>>()
            {
                new Tuple<string, string>("Content-Type", "application/xml"),
            };

            var expected = new Response(200, headers, fileAXml);

            var actual = Handler.GetStubResponse(Handler.ResolveFilePath(uri), "input file FileA");
            Assert.That(actual, Is.EqualTo(expected));
        }
    }
}
