using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Npgsql;

namespace GoldRush.Core
{
    public class JsonExceptionMiddleware
    {
        private readonly RequestDelegate next;
        public JsonExceptionMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task Invoke(HttpContext context /* other dependencies */)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var status = StatusCodes.Status500InternalServerError;
            try
            {
                status = context.Response.StatusCode;
            }
            catch { }

            // If status is 200, this is more likely a SPA error. Use an error code.
            if (status == StatusCodes.Status200OK)
                status = StatusCodes.Status503ServiceUnavailable;

            var message = "An unexpected error has ocurred.";
            if (exception is MySqlException || exception is SqlException || exception is NpgsqlException)
            {
                status = StatusCodes.Status503ServiceUnavailable;
                message = "This is service is not available. Make sure that the database exists by running database migrations. See details for more info.";
            }

            ErrorResponse error;

            if (exception != null && exception.Message.Contains("Failed to proxy the request to http://localhost:4200/")
                && exception.Source.Equals("Microsoft.AspNetCore.SpaServices.Extensions"))
            {
                error = new ErrorResponse
                {
                    StatusCode = status,
                    Status = StringUtils.FindConstantName<int>(typeof(StatusCodes), status),
                    Message = message,
                    Details = exception?.Message,
                    Exception = exception?.GetType().Name,
                    Source = exception?.Source,
                    StackTrace = exception?.HelpLink,
                    InnerException = exception?.InnerException.Message
                };
            }
            else
            {
                error = new ErrorResponse
                {
                    StatusCode = status,
                    Status = StringUtils.FindConstantName<int>(typeof(StatusCodes), status),
                    Message = message,
                    Details = exception?.Message,
                    Exception = exception?.GetType().Name,
                    Source = exception?.Source,
                    StackTrace = exception?.StackTrace,
                    InnerException = exception?.InnerException
                };
            }

            var response = JsonConvert.SerializeObject(error, Formatting.None, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = status;
            return context.Response.WriteAsync(response);
        }
    }
}