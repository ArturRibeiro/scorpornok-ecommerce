using System;

namespace Programming.Functional.Results
{
    public static class ResultExtensions
    {
        public static Result<T> ToResult<T>(this Maybe<T> maybe, string errorMessage) where T : class
        {
            if (maybe.HasNoValue) return Result.Fail<T>(errorMessage);
            return Result.Ok(maybe.Value);
        }

        public static Result OnSuccess(this Result result, Action action)
        {
            if (result.IsNone) return result;
            action();
            return Result.Ok();
        }

        public static Result OnSuccess(this Result result, Func<Result> func)
        {
            if (result.IsNone) return result;
            return func();
        }

        #region Tap

        /// <summary>
        ///     Executes the given action if the calling result is a success. Returns the calling result.
        /// </summary>
        public static Result With(this Result result, Action action)
        {
            if (result.IsSome)
                action();

            return result;
        }

        /// <summary>
        ///     Executes the given action if the calling result is a success. Returns the calling result.
        /// </summary>
        public static Result<T> With<T>(this Result<T> result, Action action)
        {
            if (result.IsSome)
                action();

            return result;
        }

        /// <summary>
        ///     Executes the given action if the calling result is a success. Returns the calling result.
        /// </summary>
        public static Result<T> With<T>(this Result<T> result, Action<T> action)
        {
            if (result.IsSome)
                action(result.Value);

            return result;
        }

        // /// <summary>
        // ///     Executes the given action if the calling result is a success. Returns the calling result.
        // /// </summary>
        // public static Result<T, E> With<T, E>(this Result<T, E> result, Action action)
        // {
        //     if (result.IsSome)
        //         action();
        //
        //     return result;
        // }
        //
        // /// <summary>
        // ///     Executes the given action if the calling result is a success. Returns the calling result.
        // /// </summary>
        // public static Result<T, E> With<T, E>(this Result<T, E> result, Action<T> action)
        // {
        //     if (result.IsSome)
        //         action(result.Value);
        //
        //     return result;
        // }

        #endregion

        
        public static Result Cath(this Result result, Action action)
        {
            if (result.IsNone) action();
            return result;
        }

        public static Result Finally(this Result result, Action<Result> action)
        {
            action(result);
            return result;
        }

        public static T Finally<T>(this Result result, Func<Result, T> func)
            => func(result);
    }
}
