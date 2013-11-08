using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace OptionType
{
    public struct Option<T> : IEnumerable<T>, IEquatable<Option<T>>
    {
        private readonly bool hasValue;
        private readonly T value;

        public static Option<T> Empty() { return new Option<T>(false, default(T)); }
        public static Option<T> Full(T value) { return new Option<T>(true, value); }

        private Option(bool hasValue, T value)
        {
            this.hasValue = hasValue;
            this.value = value;
        }

        public bool HasValue() { return hasValue; }
        public bool IsEmpty() { return !hasValue; }
        public T ValueOr(T other) { return Fold(x => x, other); }
        public T ValueOrDefault() { return ValueOr(default(T)); }

        public TResult Fold<TResult>(Func<T, TResult> ifValue, TResult elseValue)
        {
            return FoldLazy(ifValue, () => elseValue);
        }

        public TResult FoldLazy<TResult>(Func<T, TResult> ifValue, Func<TResult> elseValue)
        {
            return hasValue ? ifValue(value) : elseValue();
        }

        public Option<TResult> Select<TResult>(Func<T, TResult> f)
        {
            return Fold(x => Option.Full(f(x)), Option.Empty());
        }

        public Option<T> Where(Func<T, bool> pred)
        {
            return hasValue && pred(value) ? this : Option.Empty();
        }

        public Option<TResult> SelectMany<TResult>(Func<T, Option<TResult>> f)
        {
            return SelectMany(f, (x, y) => y);
        }

        public Option<TResult> SelectMany<TK, TResult>(Func<T, Option<TK>> f, Func<T, TK, TResult> selector)
        {
            return Fold(val => f(val).Fold(next => Option.Full(selector(val, next)), Option.Empty()), Option.Empty());
        }

        public void Do(Action<T> ifValue) { DoElse(ifValue, () => { }); }
        public void DoElse(Action<T> ifValue, Action elseValue) {
            if (hasValue) { ifValue(value); }
            else { elseValue(); }
        }

        public Option<T> OrElse(Option<T> other)
        {
            return hasValue ? this : other;
        }

        public static implicit operator Option<T>(Option option) { return Empty(); }

        public IEnumerator<T> GetEnumerator() { if (hasValue) yield return value; }
        IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

        public bool Equals(Option<T> other)
        {
            return hasValue.Equals(other.hasValue) && EqualityComparer<T>.Default.Equals(value, other.value);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (obj is Option && !hasValue) return true;
            return obj is Option<T> && Equals((Option<T>) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (hasValue.GetHashCode()*397) ^ EqualityComparer<T>.Default.GetHashCode(value);
            }
        }

        public static bool operator ==(Option<T> left, Option<T> right) { return left.Equals(right); }
        public static bool operator !=(Option<T> left, Option<T> right) { return !left.Equals(right); }
    }

    public class Option : IEquatable<Option>
    {
        private static readonly Option empty = new Option();
        private Option() { }
        public static Option<T> Full<T>(T value) { return Option<T>.Full(value); }
        public static Option Empty() { return empty; }

        public bool Equals(Option other) { return true; }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj is Option) return true;
            if (obj.GetType().IsGenericType && obj.GetType().GetGenericTypeDefinition() == typeof (Option<>))
            {
                return obj.Equals(this);
            }
            return false;
        }

        public override int GetHashCode() { return 1234; }
        public static bool operator ==(Option left, Option right) { return Equals(left, right); }
        public static bool operator !=(Option left, Option right) { return !Equals(left, right); }
    }

    public static class OptionExtensions
    {
        public static Option<T> ToOption<T>(this T instance)
        {
            return ReferenceEquals(null, instance) ? Option.Empty() : Option.Full(instance);
        }

        public static Option<T> FirstOrEmpty<T>(this IEnumerable<T> items)
        {
            return FirstOrEmpty(items, x => true);
        }

        public static Option<T> FirstOrEmpty<T>(this IEnumerable<T> items, Func<T,bool> pred)
        {
            var filtered = items.Where(pred);
            return filtered.Any() ?  Option.Full(filtered.First()) : Option.Empty();
        }

        public static Option<T> Flatten<T>(this Option<Option<T>> option)
        {
            return option.SelectMany(x => x);
        }
    }
}
