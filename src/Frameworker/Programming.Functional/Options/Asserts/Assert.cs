using System;
using System.Collections;

namespace Programming.Functional.Options.Asserts
{
    public static class Asserts
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static Option<T> IsNotNull<T>(T value, string message)
            where T : class
            => value == null
                ? Option<T>.None(message)
                : new Option<T>(value);


        public static Option<T> IsGreaterThan<T>(int expected, T value, string message = null)
        {
            switch (typeof(T).Name)
            {
                case "IList`1":
                case "List`1":
                case "String[]":
                case "ReadOnlyCollection`1":
                {
                    var collection = (value as IList);
                    if (collection == null) return Option<T>.None(message);
                    if (collection?.Count == 0) return Option<T>.None(message);
                    if (collection?.Count < expected) return Option<T>.None(message);

                    return Option<T>.Some(value);
                }
                case "Double":
                {
                    var @double = (double)Convert.ChangeType(value, typeof(double));
                    return @double <= expected ? Option<T>.None(message) : Option<T>.Some(value);
                }
                case "Int32":
                {
                    var @int32 = (int)Convert.ChangeType(value, typeof(int));
                    return @int32 <= expected ? Option<T>.None(message) : Option<T>.Some(value);
                }
                case "Decimal":
                {
                    var @decimal = (decimal)Convert.ChangeType(value, typeof(decimal));
                    return @decimal <= expected ? Option<T>.None(message) : Option<T>.Some(value);
                }
            }

            throw new NotImplementedException(nameof(T));
        }
    }
}