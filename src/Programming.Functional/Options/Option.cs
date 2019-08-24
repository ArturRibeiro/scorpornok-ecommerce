using System;

namespace Programming.Functional.Options
{
    public struct Option<T>
    {

        public T Value { get; }
        public bool IsSome { get; }
        public bool IsNone => !IsSome;

        internal Option(T value, bool isSome)
        {
            Value = value;
            IsSome = isSome;
        }

        //private readonly T _value;

        //public bool IsNone => _value == null || _value.Equals(default(T));

        //public bool IsSome => !IsNone;

        //public T Value => _value;

        //public Option(T value) => _value = value;

        public static implicit operator Option<T>(T value) => Some(value);

        public static Option<T> Some(T value) => new Option<T>(value, true);

        public static Option<T> None() => new Option<T>(default(T), false);

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

        ///// <summary>
        ///// Isso permite uma maneira sofisticada de aplicar alguma ação para valores <see cref="Option {T}" /> sem precisar verificar a existência de um valor.
        ///// </summary>
        ///// <param name="actionWhenSome">Ação que será invocada quando estiver no estado <see cref="IsSome" />.</param>
        ///// <param name="actionWhenNone">Ação que será invocada quando estiver no estado <see cref="IsNone" />.</param>
        //public void Match(
        //    Action<T> actionWhenSome,
        //    Action actionWhenNone)
        //    => Match(
        //        actionWhenSome(_value),
        //        actionWhenNone(_value)
        //    );
    }
}
