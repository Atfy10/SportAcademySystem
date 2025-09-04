using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public static Result<EType> Success(EType data, string operation, string message = "Operation done successfully")
            => new(true, data, operation, message);

        public static Result<EType> Failure(string operation, string message, int statusCode = 500)
            => new(false, default!, operation, message)
            {
                StatusCode = statusCode
            };
    }
}
