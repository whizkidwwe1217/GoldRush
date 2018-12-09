using System;
using Microsoft.AspNetCore.Mvc;

namespace GoldRush
{
    public class ErrorResponse
    {
        public int StatusCode { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
        public string Details { get; set; }
        public string Exception { get; set; }
        public string Source { get; set; }
        public object StackTrace { get; internal set; }
        public object InnerException { get; internal set; }
    }

    public static class HttpResponseExtension
    {
        public static ObjectResult FailStatus(this ControllerBase controller, int statusCode, ErrorResponse payload)
        {
            return controller.StatusCode(statusCode, payload);
        }

        public static ObjectResult FailStatus(this ControllerBase controller, int statusCode, string message, string details, string exception, string source, string stackTrace, object innerException)
        {
            return controller.StatusCode(statusCode, new ErrorResponse
            {
                StatusCode = statusCode,
                Message = message,
                Details = details,
                Exception = exception,
                Source = source,
                StackTrace = stackTrace,
                InnerException = innerException
            });
        }

        public static ObjectResult FailStatus(this ControllerBase controller, int statusCode, string message, Exception exception)
        {
            return controller.StatusCode(statusCode, new ErrorResponse
            {
                StatusCode = statusCode,
                Message = message,
                Details = exception.Message,
                Exception = exception.GetType().Name,
                InnerException = exception.InnerException
            });
        }
    }
}