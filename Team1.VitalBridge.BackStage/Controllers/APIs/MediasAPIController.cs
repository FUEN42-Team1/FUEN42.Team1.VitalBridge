using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using MvcTips.Site.Models.Utilities;
using System.ComponentModel;
using System.Net;
using System.Runtime.InteropServices;
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
        private const string FileTableUNCPath = @"\\40.76.107.125\mssqlserver\VitalBridgeDB\MyFileTableDir";
        private const string FileTableUser = "prjTeam1";
        private const string FileTablePassword = "VitalBridge123";
        private const string FileTableDomain = "prjTeam1";

        public MediasAPIController(IWebHostEnvironment env, AppDbContext context)
        {
            _env = env;
            this._context = context;
        }

        
        [HttpPost]
        public async Task<IActionResult> UploadFile([FromForm] IFormFile upload)
        {
            try
            {
                using (new NetworkConnection(FileTableUNCPath, new NetworkCredential(FileTableUser, FileTablePassword, FileTableDomain)))
                {
                    var fileTablePath = FileTableUNCPath;

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
            using (new NetworkConnection(FileTableUNCPath, new NetworkCredential(FileTableUser, FileTablePassword, FileTableDomain)))
            {
                var fileName = _context.FileStreams
                .Where(f => f.Id == fileId)
                .Select(f => f.FileName)
                .FirstOrDefault();

                var safeFileName = Path.GetFileName(fileName); // 防止路徑穿越
                var fileTablePath = FileTableUNCPath;
                var filePath = Path.Combine(fileTablePath, safeFileName);

                if (!System.IO.File.Exists(filePath))
                    return NotFound();


                var contentType = GetContentType(filePath); // ✅ 正確 MIME 類型
                var fileBytes = System.IO.File.ReadAllBytes(filePath);
                return File(fileBytes, contentType, fileName);
            }
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
        public class NetworkConnection : IDisposable
        {
            private readonly string _networkName;

            public NetworkConnection(string networkName, NetworkCredential credentials)
            {
                _networkName = networkName;

                var netResource = new NetResource
                {
                    Scope = 2, // Global
                    Type = 1,  // Disk
                    DisplayType = 3,
                    RemoteName = networkName
                };

                var result = WNetAddConnection2(netResource, credentials.Password,
                    $@"{credentials.Domain}\{credentials.UserName}", 0);

                if (result != 0)
                    throw new Win32Exception(result);
            }

            public void Dispose()
            {
                WNetCancelConnection2(_networkName, 0, true);
            }

            [DllImport("mpr.dll")]
            private static extern int WNetAddConnection2(NetResource netResource, string password, string username, int flags);

            [DllImport("mpr.dll")]
            private static extern int WNetCancelConnection2(string name, int flags, bool force);

            [StructLayout(LayoutKind.Sequential)]
            public class NetResource
            {
                public int Scope;
                public int Type;
                public int DisplayType;
                public int Usage;
                public string LocalName;
                public string RemoteName;
                public string Comment;
                public string Provider;
            }
        }
    }
}
