using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CloudFlashCard.Web.Services
{
    public class Result
    {
        public bool IsSuccess { get; }
        public string Error { get; }
        public int Id { get; }
        public bool IsFailure => !IsSuccess;

        protected Result(bool issuccess, string error)
        {
            if (issuccess && error != string.Empty)
                throw new InvalidOperationException();
            if (!issuccess && error == string.Empty)
                throw new InvalidOperationException();

            IsSuccess = issuccess;
            Error = error;
        }

        protected Result(bool issuccess, int id)
        {
            if (issuccess && id < 0)
                throw new InvalidOperationException();
            if (!issuccess && id > 0)
                throw new InvalidOperationException();

            IsSuccess = issuccess;
            Id = id;
        }


        public static Result Fail(string message)
        {
            return new Result(false, message);
        }

        public static Result<T> Fail<T>(string message)
        {
            return new Result<T>(default(T), false, message);
        }

        public static Result Ok()
        {
            return new Result(true, string.Empty);
        }

        public static Result OkWithId(int id)
        {
            return new Result(true, id);
        }

        public static Result<T> Ok<T>(T value)
        {
            return new Result<T>(value, true, string.Empty);
        }

    }

    public class Result<T> : Result
    {
        private readonly T _value;
        public T Value
        {
            get
            {
                if (!IsSuccess)
                    throw new InvalidOperationException();

                return _value;
            }
        }

        protected internal Result(T value, bool isSuccess, string error) : base(isSuccess, error)
        {
            _value = value;
        }
    }
}
