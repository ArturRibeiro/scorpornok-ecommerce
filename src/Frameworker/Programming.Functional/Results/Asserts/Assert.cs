using System;

namespace Programming.Functional.Results.Asserts
{
    public static class Assert2
    {
        public static Result IsNotNull(bool conditional, string message)
        {
            if (!conditional) return Result.Fail(message);
            
            return Result.Ok();
        }

        public static Result IsGuidEmpty(Guid conditional, string message)
        {
            if (conditional == Guid.Empty)
                return Result.Fail(message);
            
            return Result.Ok();
        }

        public static Result IsNullOrEmpty(string conditional, string message)
        {
            if (string.IsNullOrEmpty(conditional)) return Result.Fail(message);
            
            return Result.Ok();
        }
    }
}