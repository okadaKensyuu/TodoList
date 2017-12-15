using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;

namespace StubServer.Core
{
    public class Response
    {
        public readonly int StatusCode;
        public readonly IList<Tuple<string, string>> Headers;
        public readonly string Content;

        public Response(int statusCode, IList<Tuple<string, string>>headers, string content)
        {
            StatusCode = statusCode;
            Headers = headers;
            Content = content;
        }

        public static Response NotFound = new Response(404, new List<Tuple<string, string>>(), "");

        public static IEnumerable<string> Lines(string str)
        {
            var reader = new StringReader(str);
            string line = null;
            while ((line = reader.ReadLine()) != null)
            {
                yield return line;
            }
        }

        public static bool IsNotEmpty(string str) { return String.IsNullOrEmpty(str) == false; }

        public override bool Equals(object obj)
        {
            var other = obj as Response;
            if (other == null) return false;

            return this.StatusCode == other.StatusCode &&
                   Enumerable.SequenceEqual(this.Headers, other.Headers) &&
                   this.Content == other.Content;
        }

        public override int GetHashCode()
        {
            return 0;
        }

        public static Response Parse(string text)
        {
            var parts = Lines(text).Split(String.IsNullOrEmpty);

            var statusCode = int.Parse(parts.ElementAt(0).Single());

            var headers = from header in parts.ElementAt(1)
                          let kv = header.Split(':').Select(x => x.Trim())
                          select new Tuple<string, string>(kv.ElementAt(0), kv.ElementAt(1));

            var contentsPart = parts.ElementAt(2);

            var content = string.Join("\r\n", contentsPart);

            foreach(var h in headers)
            {
                if(h.Item1 == "SleepTime")
                {
                    Thread.Sleep(Int32.Parse(h.Item2));
                }
            }

            return new Response(statusCode, headers.ToList(), content);
        }

        public override string ToString()
        {
            var str = new StringBuilder();
            str.AppendLine(StatusCode.ToString());
            str.AppendLine();

            foreach (var header in Headers)
            {
                str.Append(header.Item1).Append(" : ").Append(header.Item2).AppendLine();
            }
            str.AppendLine();

            str.Append(Content);

            return str.ToString();
        }
    }
}
