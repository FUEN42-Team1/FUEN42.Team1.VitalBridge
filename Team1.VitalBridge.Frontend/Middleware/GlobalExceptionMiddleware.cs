using System.Net;
using System.Text.Json;
using Team1.VitalBridge.Frontend.Models.Responses;

namespace Team1.VitalBridge.Frontend.Middleware
{
    /// <summary>
    /// 全域錯誤處理中間件
    /// </summary>
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;
        private readonly IWebHostEnvironment _environment;

        public GlobalExceptionMiddleware(
            RequestDelegate next, 
            ILogger<GlobalExceptionMiddleware> logger,
            IWebHostEnvironment environment)
        {
            _next = next;
            _logger = logger;
            _environment = environment;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "發生未處理的例外: {Message}", ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            
            var response = new ApiResponse<object>();
            
            switch (exception)
            {
                case ArgumentException argEx:
                    response.Success = false;
                    response.Message = "請求參數錯誤";
                    response.ErrorCode = "INVALID_ARGUMENT";
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    break;
                    
                case UnauthorizedAccessException:
                    response.Success = false;
                    response.Message = "未經授權的存取";
                    response.ErrorCode = "UNAUTHORIZED";
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    break;
                    
                case KeyNotFoundException:
                    response.Success = false;
                    response.Message = "找不到請求的資源";
                    response.ErrorCode = "RESOURCE_NOT_FOUND";
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    break;
                    
                case TimeoutException:
                    response.Success = false;
                    response.Message = "請求逾時";
                    response.ErrorCode = "TIMEOUT";
                    context.Response.StatusCode = (int)HttpStatusCode.RequestTimeout;
                    break;
                    
                default:
                    response.Success = false;
                    response.Message = _environment.IsDevelopment() 
                        ? exception.Message 
                        : "系統發生錯誤，請稍後再試";
                    response.ErrorCode = "INTERNAL_ERROR";
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    break;
            }

            // 在開發環境中包含詳細的錯誤資訊
            if (_environment.IsDevelopment())
            {
                response.Data = new
                {
                    Exception = exception.GetType().Name,
                    Message = exception.Message,
                    StackTrace = exception.StackTrace
                };
            }

            var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            });

            await context.Response.WriteAsync(jsonResponse);
        }
    }

    /// <summary>
    /// 全域錯誤處理中間件擴展方法
    /// </summary>
    public static class GlobalExceptionMiddlewareExtensions
    {
        public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<GlobalExceptionMiddleware>();
        }
    }
}