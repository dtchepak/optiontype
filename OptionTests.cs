using System;
using NUnit.Framework;

namespace OptionType
{
    public class OptionTests
    {
        [Test]
        public void HasValue()
        {
            Assert.True(Option.Full(2).HasValue());
            Assert.False(Option<int>.Empty().HasValue());
        }

        [Test]
        public void IsEmpty()
        {
            Assert.False(Option.Full(2).IsEmpty());
            Assert.True(Option<int>.Empty().IsEmpty());
        }

        [Test]
        public void ValueOr()
        {
            Assert.AreEqual(42, Option.Full(42).ValueOr(0));
            Assert.AreEqual(0, Option<int>.Empty().ValueOr(0));
        }

        [Test]
        public void Fold()
        {
            Assert.AreEqual("hi!", Option.Full("hi").Fold(x => x + "!", "world"));
            Assert.AreEqual("world", Option<string>.Empty().Fold(x => x + "!", "world"));
        }

        [Test]
        public void FoldLazy()
        {
            Assert.AreEqual("hi!", Option.Full("hi").FoldLazy(x => x + "!", ThrowIfCalled<string>));
            Assert.AreEqual("world", Option<string>.Empty().Fold(x => ThrowIfCalled<string>(), "world"));
        }

        [Test]
        public void OrElse()
        {
            var bothFull = Option.Full(1).OrElse(Option.Full(2));
            Assert.AreEqual(1, bothFull.ValueOr(0));

            var firstFull = Option.Full(1).OrElse(Option.Empty());
            Assert.AreEqual(1, firstFull.ValueOr(0));

            var secondFull = Option<int>.Empty().OrElse(Option.Full(2));
            Assert.AreEqual(2, secondFull.ValueOr(0));

            var bothEmpty = Option<int>.Empty().OrElse(Option.Empty());
            Assert.AreEqual(0, bothEmpty.ValueOr(0));
        }

        [Test]
        public void Do()
        {
            var value = 0;
            Option.Full(1).Do(x => value = x);
            Assert.AreEqual(1, value);
        }

        [Test]
        public void Equality()
        {
            Assert.True(Option.Empty() == Option<int>.Empty());
            Assert.True(Option<int>.Empty() == Option<int>.Empty());
            Assert.True(Option.Full(2) == Option.Full(2));
            Assert.True(Option.Full(2) != Option.Empty());
            Assert.True(Option.Full(2) != Option.Full(3));
            Assert.True(Option.Full(2) != Option<int>.Empty());
            Assert.False(Option<int>.Empty().Equals(Option<string>.Empty()));
            Assert.False(Option.Full(2).Equals(Option.Full("hi")));
        }

        [Test]
        public void Linq()
        {
            var result =
                from i in ParseInt("42")
                from j in ParseInt("100")
                select i + j;

            Assert.AreEqual(142, result.ValueOr(-1));
        }

        [Test]
        public void Linq2()
        {
            var result =
                from i in ParseInt("42")
                from j in ParseInt("not a number")
                select i + j;

            Assert.AreEqual(-1, result.ValueOr(-1));
        }

        [Test]
        public void Linq3()
        {
            var result =
                from i in 42.ToOption()
                from j in ParseInt("100")
                where j < 10
                select i + j;

            Assert.AreEqual(-1, result.ValueOr(-1));
        }

        [Test]
        public void Select()
        {
            Assert.AreEqual("10", 10.ToOption().Select(x => x.ToString()).ValueOr(""));
            string nullString = null;
            Assert.AreEqual("", nullString.ToOption().Select(x => x.ToString()).ValueOr(""));
        }

        [Test]
        public void Flatten()
        {
            var nested = Option.Full(2).Select(Option.Full);
            var option = nested.Flatten();
            Assert.AreEqual(2, option.ValueOr(0));
        }

        private T ThrowIfCalled<T>()
        {
            throw new Exception("Should not be called");
        }

        private Option<int> ParseInt(string s)
        {
            int value;
            return int.TryParse(s, out value) ? Option.Full(value) : Option.Empty();
        }
    }
}