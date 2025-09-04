using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Services
{
    public abstract class ResultBase
    {
        public bool IsSuccess { get; protected set; }
        public string OperationType { get; protected set; }
        public string Message { get; protected set; }
        public int StatusCode { get; set; }

        protected ResultBase(bool isSuccess, string operationType, string message)
        {
            IsSuccess = isSuccess;
            OperationType = operationType;
            Message = message;
        }
    }
}
