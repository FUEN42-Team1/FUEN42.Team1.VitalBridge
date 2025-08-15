using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using MvcTips.Site.Models.Utilities;
using Team1.VitalBridge.BackStage.Models.EFModels;
using FileStream = System.IO.FileStream;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Team1.VitalBridge.BackStage.Controllers.APIs
{
    [Route("api/[controller]")]
    [ApiController]
    public class MediasAPIController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public MediasAPIController(IWebHostEnvironment env, AppDbContext context)
        {
            _env = env;
            this._context = context;
        }

        //[HttpPost]
        //public async Task<IActionResult> UploadMedia(IFormFile upload)
        //{
        //    if (upload == null || upload.Length == 0)
        //        return BadRequest("No file uploaded.");

        //    // 1. Save to FileTable folder path
        //    var fileTablePath = @"\\localhost\mssqlserver\VitalBridgeDB\MyFileTableDir";

        //    // 取原始副檔名
        //    string extension = Path.GetExtension(upload.FileName);
        //    // 使用 Guid 產生新檔名
        //    string fileName = $"{Guid.NewGuid().ToString("N")}{extension}";

        //    string filePath = Path.Combine(fileTablePath, fileName);
        //    if (!Directory.Exists(fileTablePath))
        //    {
        //        Directory.CreateDirectory(fileTablePath);
        //    }

        //    using (var stream = new FileStream(filePath, FileMode.Create))
        //    {
        //        await upload.CopyToAsync(stream);
        //    }

        //    // 2. Create Media record
        //    var media = new Media
        //    {
        //        Name = fileName,
        //        FileId = _context.FileStreams.FirstOrDefault(f => f.FileName == fileName).Id,
        //        CreatedAt = DateTime.Now
        //    };
        //    _context.Medias.Add(media);
        //    await _context.SaveChangesAsync();

        //    //// 3. Return the URL for CKEditor to display
        //    //return Ok(new
        //    //{
        //    //    uploaded = true,
        //    //    url = $"/media/{media.FileId}" // Your download endpoint
        //    //});
        //    return Ok();
        //}

        [HttpPost]
        public async Task<IActionResult> UploadFile([FromForm] IFormFile upload)
        {
            try
            {
                var fileTablePath = @"\\localhost\mssqlserver\VitalBridgeDB\MyFileTableDir";

                if (upload == null || upload.Length == 0)
                {
                    return null;
                }

                // 取原始副檔名
                string extension = Path.GetExtension(upload.FileName);
                // 使用 Guid 產生新檔名
                string fileName = $"{Guid.NewGuid().ToString("N")}{extension}";

                string filePath = Path.Combine(fileTablePath, fileName);
                if (!Directory.Exists(fileTablePath))
                {
                    Directory.CreateDirectory(fileTablePath);
                }

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await upload.CopyToAsync(stream);
                }
                var FileId = _context.FileStreams.FirstOrDefault(f => f.FileName == fileName)?.Id;

                return Ok(new
                {
                    uploaded = true,
                    url = $"/api/MediasAPI/{FileId}"
                });
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine(ex);

                return Ok(new
                {
                    uploaded = false,
                    error = new { message = "Server error: " + ex.Message }
                });
            }
        }

        [HttpGet("{fileId}")]
        public IActionResult GetFile(int fileId)
        {
            var fileName = _context.FileStreams
                .Where(f => f.Id == fileId)
                .Select(f => f.FileName)
                .FirstOrDefault();
            var safeFileName = Path.GetFileName(fileName); // 防止路徑穿越
            var fileTablePath = @"\\localhost\mssqlserver\VitalBridgeDB\MyFileTableDir";
            var filePath = Path.Combine(fileTablePath, safeFileName);

            if (!System.IO.File.Exists(filePath))
                return NotFound();
            

            var contentType = GetContentType(filePath); // ✅ 正確 MIME 類型
            var fileBytes = System.IO.File.ReadAllBytes(filePath);
            return File(fileBytes, contentType, fileName);
            //return File(fileBytes, contentType);
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

        // PUT api/<MediasAPIController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<MediasAPIController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
