using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System;

namespace GoldRush.Infrastructure.Filters
{
    /// <summary>
    /// Handles cancellation of http requests.
    /// </summary>
    public class RequestAbortedFilter : ExceptionFilterAttribute
    {
        private readonly ILogger _logger;

        public RequestAbortedFilter(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<RequestAbortedFilter>();
        }
        public override void OnException(ExceptionContext context)
        {
            if (context.Exception is OperationCanceledException)
            {
                _logger.LogWarning("Request was cancelled");
                context.ExceptionHandled = true;
                context.Result = new StatusCodeResult(499);
            }
        }
    }
}
