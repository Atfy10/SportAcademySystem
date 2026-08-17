using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SportAcademy.Application.Common.Result;

namespace SportAcademy.Web.Filters
{
    // Controllers uniformly return Ok(result) regardless of whether the underlying
    // Result/ResultBase actually succeeded. This filter rewrites the HTTP status code
    // to match ResultBase.StatusCode whenever IsSuccess is false, so application-level
    // failures stop being reported to clients as HTTP 200.
    public class ResultStatusFilter : IResultFilter
    {
        public void OnResultExecuting(ResultExecutingContext context)
        {
            if (context.Result is ObjectResult { Value: ResultBase result } objectResult && !result.IsSuccess)
            {
                objectResult.StatusCode = result.StatusCode is >= 400 and <= 599
                    ? result.StatusCode
                    : StatusCodes.Status400BadRequest;
            }
        }

        public void OnResultExecuted(ResultExecutedContext context)
        {
        }
    }
}
