using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace StubServer.Core.Tests
{
    [TestFixture]
    class EnumerableExtensionsTest
    {
        [Test]
        public void シーケンスを2つに分割できること()
        {
            var input = new[] { "a", "b", "", "c", "d" };
            var actual = input.Split(String.IsNullOrEmpty).Select(x => x.ToArray()).ToArray();

            var expected = new[] { 
                new[] { "a", "b" },
                new[] { "c", "d" },
            };

            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void シーケンスを3つに分割できること()
        {
            var input = new[] { "a", "b", "", "c", "d", "", "e", "f" };
            var actual = input.Split(String.IsNullOrEmpty).Select(x => x.ToArray()).ToArray();

            var expected = new[] { 
                new[] { "a", "b" },
                new[] { "c", "d" },
                new[] { "e", "f" },
            };

            Assert.That(actual, Is.EqualTo(expected));
        }
    }
}
