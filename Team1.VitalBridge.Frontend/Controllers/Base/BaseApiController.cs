using Microsoft.AspNetCore.Mvc;
using Team1.VitalBridge.Frontend.Models.Responses;

namespace Team1.VitalBridge.Frontend.Controllers.Base
{
    /// <summary>
    /// API 控制器基礎類別，提供統一的回應格式和錯誤處理
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public abstract class BaseApiController : ControllerBase
    {
        protected readonly ILogger _logger;

        protected BaseApiController(ILogger logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 建立成功回應
        /// </summary>
        /// <typeparam name="T">資料類型</typeparam>
        /// <param name="data">回應資料</param>
        /// <param name="message">成功訊息</param>
        /// <returns>統一格式的成功回應</returns>
        protected IActionResult SuccessResponse<T>(T data, string message = "查詢成功")
        {
            var response = ApiResponse<T>.CreateSuccess(data, message);
            return Ok(response);
        }

        /// <summary>
        /// 建立分頁成功回應
        /// </summary>
        /// <typeparam name="T">資料類型</typeparam>
        /// <param name="data">回應資料</param>
        /// <param name="totalCount">總筆數</param>
        /// <param name="currentPage">當前頁碼</param>
        /// <param name="pageSize">每頁筆數</param>
        /// <param name="message">成功訊息</param>
        /// <returns>統一格式的分頁成功回應</returns>
        protected IActionResult PagedSuccessResponse<T>(
            IEnumerable<T> data, 
            int totalCount, 
            int currentPage, 
            int pageSize, 
            string message = "查詢成功")
        {
            var response = PagedApiResponse<T>.CreatePagedSuccess(data, totalCount, currentPage, pageSize, message);
            return Ok(response);
        }

        /// <summary>
        /// 建立錯誤回應
        /// </summary>
        /// <param name="message">錯誤訊息</param>
        /// <param name="errorCode">錯誤代碼</param>
        /// <param name="statusCode">HTTP 狀態碼</param>
        /// <returns>統一格式的錯誤回應</returns>
        protected IActionResult ErrorResponse(string message, string errorCode = "QUERY_ERROR", int statusCode = 400)
        {
            var response = ApiResponse<object>.CreateError(message, errorCode);
            
            _logger.LogError("API Error - Code: {ErrorCode}, Message: {Message}", errorCode, message);
            
            return StatusCode(statusCode, response);
        }

        /// <summary>
        /// 處理例外並回傳錯誤回應
        /// </summary>
        /// <param name="ex">例外物件</param>
        /// <param name="customMessage">自訂錯誤訊息</param>
        /// <returns>統一格式的錯誤回應</returns>
        protected IActionResult HandleException(Exception ex, string? customMessage = null)
        {
            var message = customMessage ?? "系統發生錯誤，請稍後再試";
            var errorCode = "INTERNAL_ERROR";

            _logger.LogError(ex, "API Exception: {Message}", ex.Message);

            var response = ApiResponse<object>.CreateError(message, errorCode);
            return StatusCode(500, response);
        }

        /// <summary>
        /// 驗證模型狀態並回傳錯誤回應
        /// </summary>
        /// <returns>模型驗證錯誤回應，如果模型狀態有效則回傳 null</returns>
        protected IActionResult? ValidateModelState()
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .SelectMany(x => x.Value!.Errors)
                    .Select(x => x.ErrorMessage)
                    .ToList();

                var message = string.Join("; ", errors);
                return ErrorResponse(message, "VALIDATION_ERROR", 400);
            }

            return null;
        }

        /// <summary>
        /// 檢查資源是否存在
        /// </summary>
        /// <param name="resource">資源物件</param>
        /// <param name="resourceName">資源名稱</param>
        /// <returns>如果資源不存在則回傳錯誤回應，否則回傳 null</returns>
        protected IActionResult? CheckResourceExists(object? resource, string resourceName = "資源")
        {
            if (resource == null)
            {
                return ErrorResponse($"找不到指定的{resourceName}", "RESOURCE_NOT_FOUND", 404);
            }

            return null;
        }
    }
}