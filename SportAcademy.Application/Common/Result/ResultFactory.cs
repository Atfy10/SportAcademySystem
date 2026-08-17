using System.Reflection;

namespace SportAcademy.Application.Common.Result
{
    // Shared generic-failure construction for pipeline behaviors that need to short-circuit
    // with a Result/Result<T> failure without knowing the concrete response type at compile
    // time (mirrors the private CreateFailure helper in ExceptionHandlingBehavior).
    public static class ResultFactory
    {
        public static TResponse CreateFailure<TResponse>(
            string requestName, string message, int statusCode, Dictionary<string, string[]>? errors = null)
            where TResponse : ResultBase
        {
            var responseType = typeof(TResponse);

            if (responseType == typeof(Result))
            {
                return (TResponse)(object)Result.Failure(requestName, message, statusCode, errors);
            }

            if (responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(Result<>))
            {
                var genericArguments = responseType.GetGenericArguments()[0];
                var resultGenericType = typeof(Result<>).MakeGenericType(genericArguments);
                var failureMethod = resultGenericType
                    .GetMethods(BindingFlags.Public | BindingFlags.Static)
                    .FirstOrDefault(m =>
                        m.Name == "Failure" &&
                        m.GetParameters().Length == 4 &&
                        m.GetParameters()[3].ParameterType == typeof(Dictionary<string, string[]>));

                if (failureMethod != null)
                {
                    var failureInstance = failureMethod.Invoke(null, new object?[] { requestName, message, statusCode, errors });
                    return (TResponse)failureInstance!;
                }
            }

            throw new InvalidOperationException($"Unsupported response type: {responseType.Name}");
        }

        // Machine-readable marker (Errors["code"] = [code]) so the frontend can react
        // uniformly (e.g. a distinct "feature not in your plan" UI) without parsing message text.
        public static TResponse CreateFailureWithCode<TResponse>(
            string requestName, string message, int statusCode, string code)
            where TResponse : ResultBase
            => CreateFailure<TResponse>(requestName, message, statusCode, new Dictionary<string, string[]> { ["code"] = [code] });
    }
}
