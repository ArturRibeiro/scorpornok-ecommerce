using System;
using System.Linq;

namespace Programming.Functional.Options.Asserts
{
    public struct Option : IOption
    {
        private static readonly Option _some = new Option(false);
        private static readonly Option _none = new Option(true);

        public bool IsNone { get; }
        public bool IsSome => !IsNone;
        public string Message { get; }

        public Option(bool isNone, string message = null)
        {
            this.IsNone = isNone;
            this.Message = message;
        }

        public static Option<T> Some<T>(T value) 
            => new Option<T>(value);

        public static Option<T> None<T>(T value, string msg) 
            => new Option<T>(value, msg);

        public static Option Some() 
            => _some;

        public static Option None() 
            => new Option(isNone: true);

        public static Option None(string message) 
            => new Option(isNone: true, message: message);

        //public static implicit operator Option(int value)
        //    => Option.Some(value);

        public static IOption Combine(params IOption[] options)
        {
            //var opt = options.Where(x => x.IsNone).Select(x => x).ToArray();

            //if (!opt.Any())
            //    return _some;

            //return Option<string>.None(string.Join(Environment.NewLine, opt.Select(x => x.Message).ToArray()));

            var opt = options.FirstOrDefault(x => x.IsNone);

            if (opt == null) return Option.Some();

            return Option<string>.None(string.Join(Environment.NewLine, opt.Message));
        }
    }
}