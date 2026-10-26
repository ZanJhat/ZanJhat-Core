using System;
using System.Linq;
using System.Collections.Generic;
using Engine;
using Game;

namespace ZanJhat.Core
{
    public struct Result<T>
    {
        public bool Success;
        public T Value;
        public string Error;

        public static Result<T> Ok(T value)
        {
            return new Result<T>
            {
                Success = true,
                Value = value,
                Error = null
            };
        }

        public static Result<T> Fail(string error)
        {
            return new Result<T>
            {
                Success = false,
                Value = default,
                Error = error
            };
        }
    }
}
