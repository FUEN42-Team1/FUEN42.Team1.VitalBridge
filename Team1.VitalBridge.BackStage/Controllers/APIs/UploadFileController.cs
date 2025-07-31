using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using MvcTips.Site.Models.Utilities;

namespace Team1.VitalBridge.BackStage.Controllers.APIs
{
    [Route("api/[controller]")]
    [ApiController]
    public class UploadFileController : ControllerBase
    {
        private readonly IWebHostEnvironment _env;

        public UploadFileController(IWebHostEnvironment env)
        {
            _env = env;
        }

        [HttpPost]
        [RequestSizeLimit(10 * 1024 * 1024)] // 限制最大上傳 10MB
        public async Task<IActionResult> UploadFile([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("請選擇要上傳的檔案。");
            var fileTablePath = @"\\localhost\mssqlserver\VitalBridgeDB\MyFileTableDir";
            var savedFileInfo = await UploadFileHelper.SaveUploadedFile(file, fileTablePath);
            //var uploads = Path.Combine(_env.WebRootPath ?? "wwwroot", "uploads");

            //var savedFileInfo = await UploadFileHelper.SaveUploadedFile(file, uploads);
            if (savedFileInfo.Length==0 || savedFileInfo.FilePath ==null)
                return BadRequest(savedFileInfo.FileName);
            return Ok(new
            {
                savedFileInfo.FileName,
                savedFileInfo.Length,
                FilePath = Url.Action("GetFile", "UploadFile", new { fileName = savedFileInfo.FileName })
            });
        }

        [HttpGet("GetFile")]
        public IActionResult GetFile(string fileName)
        {
            var safeFileName = Path.GetFileName(fileName); // 防止路徑穿越
            var fileTablePath = @"\\localhost\mssqlserver\VitalBridgeDB\MyFileTableDir";
            var filePath = Path.Combine(fileTablePath, safeFileName);

            if (!System.IO.File.Exists(filePath))
                return NotFound();

            var contentType = GetContentType(filePath); // ✅ 正確 MIME 類型
            var fileBytes = System.IO.File.ReadAllBytes(filePath);
            return File(fileBytes, contentType);
        }
        private string GetContentType(string path)
        {
            var provider = new FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(path, out var contentType))
            {
                contentType = "application/octet-stream"; // 預設值
            }
            return contentType;
        }
    }
}
