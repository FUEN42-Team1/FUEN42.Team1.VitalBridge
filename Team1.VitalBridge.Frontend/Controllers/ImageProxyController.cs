using Microsoft.AspNetCore.Mvc;

namespace Team1.VitalBridge.Frontend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ImageProxyController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public ImageProxyController(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        [HttpGet("{fileName}")]
        public async Task<IActionResult> GetImage(string fileName)
        {
            try
            {
                // 從後台 API 取得圖片
                var backendUrl = $"https://localhost:7184/api/UploadFile/GetFile?fileName={fileName}";
                var response = await _httpClient.GetAsync(backendUrl);

                if (response.IsSuccessStatusCode)
                {
                    var imageBytes = await response.Content.ReadAsByteArrayAsync();
                    var contentType = response.Content.Headers.ContentType?.ToString() ?? "image/jpeg";
                    
                    // 設定快取標頭
                    Response.Headers["Cache-Control"] = "public,max-age=604800"; // 快取一週
                    
                    return File(imageBytes, contentType);
                }

                return NotFound($"圖片檔案 {fileName} 不存在");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"取得圖片時發生錯誤: {ex.Message}");
            }
        }

        [HttpGet("direct/{fileName}")]
        public IActionResult GetImageDirect(string fileName)
        {
            // 直接重定向到後台 API
            var backendUrl = $"https://localhost:7184/api/UploadFile/GetFile?fileName={fileName}";
            return Redirect(backendUrl);
        }
    }
}