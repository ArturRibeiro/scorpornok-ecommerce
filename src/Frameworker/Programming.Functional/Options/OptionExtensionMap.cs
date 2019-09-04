using System;
using System.Runtime.InteropServices.WindowsRuntime;

namespace Programming.Functional.Options
{
    public static class OptionExtensionMap
    {
        /// <summary>
        /// Esta função é comumente utilizada para alterar um valor opcional através de uma função comum.
        /// Com o Map é possível aplicar uma função que espera um valor do tipo int à um Option<int>, por exemplo.
        /// Esta função utiliza o Match para extrair o valor do tipo Option<T>, aplica a função mapping e encapsula o resultado em um novo valor opcional.
        /// 
        /// Esta função é similar ao Select do namespace System.Linq, mas para valores opcionais.
        ///
        ///     <example>
        ///     <code>
        ///         <see cref="Option<T>"/> optionValue = 4;
        ///         <see cref="Option<TResult>"/> result = optionValue.Map(value => value * 2);
        ///     </code>
        ///     </example>
        /// </summary>
        /// 
        /// <typeparam name="T">Tipo de valor do <paramref name="Option<T>"/></typeparam>
        /// <typeparam name="TResult">O tipo de valor que vai ser retornado pela função <paramref name="mapfunc" />.</typeparam>
        /// <param name="this">A opção de entrada.</param>
        /// <param name="mapfunc">Função que esta mapeando o valor do <paramref name="Option<T>" de entrada./></param>
        /// <returns></returns>
        public static Option<TResult> Map<T, TResult>(this Option<T> @this,
            Func<T, TResult> mapfunc)
            => @this.Match
                (
                    value => mapfunc(value),
                    Option<TResult>.None
                );


        //public static Option<T> Then<T, TResult>(this Option<T> @this,
        //    Func<T, TResult> mapfunc) where TResult : IOption
        //{
        //    var result = @this.Match(
        //        value =>
        //        {
        //            var r = mapfunc(@this.Value);
        //            return r;
        //        }, Option<TResult>.None);
        //    var result2 = mapfunc(@this.Value);
        //    return new Option<T>();
        //}


        public static Option<T> OnSuccess<T, TResult>(this Option<T> @this, 
            Func<T, TResult> mapfunc)
        {
            if (@this.IsNone) return @this; ;

            var result = (IOption)mapfunc(@this.Value);

            return result.IsSome
                ? @this
                : Option<T>.None(result.Message) ;
        }

        public static Option<T> OnSuccess<T, TResult>(this Option<T> @this,
            Action<T, TResult> mapfunc)
        {
            //if (@this.IsNone) return @this; ;

            //var result = (IOption)mapfunc(@this.Value);

            //return result.IsSome
            //    ? @this
            //    : Option<T>.None(result.Message);

            throw  new NotImplementedException();
        }
    }
}