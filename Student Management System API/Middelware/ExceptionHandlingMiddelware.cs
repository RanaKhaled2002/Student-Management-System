using Microsoft.AspNetCore.Http.HttpResults;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;
using FluentValidation;
using FluentValidation.Results;

namespace Student_Management_System_API.Middelware
{
    public class ExceptionHandlingMiddelware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddelware> _logger;

        public ExceptionHandlingMiddelware(RequestDelegate next, ILogger<ExceptionHandlingMiddelware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (FluentValidation.ValidationException ex)
            {
                _logger.LogWarning(ex, "Validation error");

                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;

                var errorResponse = new
                {
                    statusCode = context.Response.StatusCode,
                    error = "Validation Failed",
                    errors = ex.Errors.Select(e => new { e.PropertyName, e.ErrorMessage })
                };

                var json = JsonSerializer.Serialize(errorResponse);
                await context.Response.WriteAsync(json);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled Exception Occurred");

                var response = context.Response;
                response.ContentType = "application/json";

                var statuesCode = ex switch
                {
                    ArgumentNullException => HttpStatusCode.BadRequest,
                    ArgumentException => HttpStatusCode.BadRequest,
                    UnauthorizedAccessException  => HttpStatusCode.Unauthorized,
                    KeyNotFoundException => HttpStatusCode.NotFound,
                    _ => HttpStatusCode.InternalServerError
                };

                response.StatusCode = (int)statuesCode;

                var errorResponse = new
                {
                    statusCode = response.StatusCode,
                    error = ex.Message,
                    message = statuesCode switch
                    {
                        HttpStatusCode.BadRequest => "Invalid Input",
                        HttpStatusCode.Unauthorized => "Unauthorized Access",
                        HttpStatusCode.NotFound => "Data not found",
                        HttpStatusCode.InternalServerError => "Something Wrong",
                        _ => "An error occurred"
                    }
                };

                var json = JsonSerializer.Serialize(errorResponse);
                await response.WriteAsync(json);
            }
            
        }
    }
}
