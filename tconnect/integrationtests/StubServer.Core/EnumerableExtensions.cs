using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StubServer.Core
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<IEnumerable<T>> Split<T>(this IEnumerable<T> xs, Func<T, bool> pred)
        {
            var first = xs.TakeWhile(x => pred(x) == false);
            if (first.Count() == xs.Count()) return new[] { first };

            var last = xs.SkipWhile(x => pred(x) == false).Skip(1);
            return (new[] { first }).Concat(last.Split(pred));
        }
    }
}
