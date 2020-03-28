using System;

namespace Programming.Functional.Results
{
    public class Result
    {
        public bool IsSome { get; }
        public string Message { get; }
        public bool IsNone => !IsSome;

        protected Result(bool isSuccess, string message)
        {
            if (isSuccess && message != string.Empty) throw new InvalidOperationException();
            if (!isSuccess && message == string.Empty) throw new InvalidOperationException();

            this.IsSome = isSuccess;
            this.Message = message;
        }

        public static Result Fail(string message)
            =>  new Result(false, message);

        public static Result<T> Fail<T>(string message)
            =>  new Result<T>(default(T), false, message);

        public static Result Ok()
            => new Result(true, string.Empty);

        public static Result<T> Ok<T>(T value)
            => new Result<T>(value, true, string.Empty);

        /// <summary>
        /// Combina uma matriz de Option
        /// </summary>
        /// <param name="results"></param>
        /// <returns></returns>
        public static Result Combine(params Result[] results)
        {
            foreach (var result in results)
            {
                if (result.IsNone)
                    return result;
            }

            return Ok();
        }
    }


    


    
}