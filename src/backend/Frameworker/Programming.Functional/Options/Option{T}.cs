using System;
using System.Diagnostics;

namespace Programming.Functional.Options
{
    public interface IOption
    {
        bool IsNone { get; }
        bool IsSome { get; }
        string Message { get; }
    }

    public struct Option<T> : IOption
    {
        public bool IsNone => Value == null || Value.Equals(default(T));

        public bool IsSome => !IsNone;
        public T Value { get; }
        public string Message { get; }

        public Option(T value, string message = null)
        {
            Value = value;
            Message = message;
        }

        public static implicit operator Option<T>(T value) => Some(value);

        public static Option<T> Some(T value) => new Option<T>(value);

        public static Option<T> None() => new Option<T>(default(T));

        public static Option<T> None(string message)
            => new Option<T>(default(T), message);

        /// <summary>
        /// Permite uma maneira de aplicar um método à um valor opcional sem necessidade de checar o estado do valor..
        /// </summary>
        /// <typeparam name="TResult">O tipo de valor retornado por funções <paramref name="funcSome"/> e <paramref name="funcNone" />.</typeparam>
        /// <param name="funcSome">Método que será invocado quando estiver no estado <see cref="IsSome" />.</param>
        /// <param name="funcNone">Método que será invocado quando estiver no estado <see cref="IsNone" />.</param>
        /// <returns></returns>
        public TResult Match<TResult>(Func<T, TResult> funcSome, Func<TResult> funcNone)
            => IsSome
                ? funcSome(Value)
                : funcNone();

        /////// <summary>
        /////// Isso permite uma maneira sofisticada de aplicar alguma ação para valores <see cref="Option {T}" /> sem precisar verificar a existência de um valor.
        /////// </summary>
        /////// <param name="actionWhenSome">Ação que será invocada quando estiver no estado <see cref="IsSome" />.</param>
        /////// <param name="actionWhenNone">Ação que será invocada quando estiver no estado <see cref="IsNone" />.</param>
        //public void Match(
        //    Action<T> actionWhenSome,
        //    Action actionWhenNone)
        //    => Match(
        //        actionWhenSome(_value),
        //        actionWhenNone(_value)
        //    );
    }
}
