using System;
using System.Collections.Generic;
using System.Linq;
using System.Net; 
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Text.Json; 
using API.Errors; 



namespace API.Middleware
{
    public class ExceptionMiddleware
    {
        public readonly RequestDelegate _next; 
        private readonly ILogger<ExceptionMiddleware> _logger;
        public readonly IHostEnvironment _env; 
        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IHostEnvironment env)
        {
            _env = env;
            _logger = logger;
            _next = next;

        }

        public async Task InvokeAsync (HttpContext context)
        {
            try{
                await _next(context);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int) HttpStatusCode.InternalServerError; 

                var response = _env.IsDevelopment()
                ? new APIException(context.Response.StatusCode, ex.Message, ex.StackTrace?.ToString())
                : new APIException(context.Response.StatusCode, "Internal Server Error", "");

                var options = new JsonSerializerOptions{PropertyNamingPolicy = JsonNamingPolicy.CamelCase};

                var json = JsonSerializer.Serialize(response, options);
                await context.Response.WriteAsync(json);
            }
        }
    }
}