using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SportAcademy.Application.Services
{
    public class Result<EType> : ResultBase
    {
        public EType? Data { get; set; }

        private Result(bool isSuccess, EType data, string operationType, string message)
                : base(isSuccess, operationType, message)
        {
            Data = data;
        }

        public static Result<EType> Success(EType data, string operation,
            string message = "{Operation} operation done successfully")
            => new(true, data, operation, message.Replace("{Operation}", operation));
    }

    public class Result : ResultBase
    {
        private Result(bool isSuccess, string operationType, string message)
        : base(isSuccess, operationType, message)
        {
        }

        public static Result Failure(string operation, string message, int statusCode = 500)
        => new(false, operation, message)
        {
            StatusCode = statusCode
        };

    }
}
