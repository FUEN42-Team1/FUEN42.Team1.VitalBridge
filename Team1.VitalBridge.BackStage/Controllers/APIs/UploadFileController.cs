using System.ComponentModel;
using System.Net;
using System.Runtime.InteropServices;
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
        private const string FileTableUNCPath = @"\\40.76.107.125\mssqlserver\VitalBridgeDB\MyFileTableDir";
        private const string FileTableUser = "prjTeam1";
        private const string FileTablePassword = "VitalBridge123";
        private const string FileTableDomain = "prjTeam1";

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

            using (new NetworkConnection(FileTableUNCPath, new NetworkCredential(FileTableUser, FileTablePassword, FileTableDomain)))
            {
                var savedFileInfo = await UploadFileHelper.SaveUploadedFile(file, FileTableUNCPath);

                if (savedFileInfo.Length == 0 || savedFileInfo.FilePath == null)
                    return BadRequest(savedFileInfo.FileName);

                return Ok(new
                {
                    savedFileInfo.FileName,
                    savedFileInfo.Length,
                    FilePath = Url.Action("GetFile", "UploadFile/" + savedFileInfo.FileName)
                });
            }


            //var fileTablePath = @"\\40.76.107.125\mssqlserver\VitalBridgeDB\MyFileTableDir";
            //var savedFileInfo = await UploadFileHelper.SaveUploadedFile(file, fileTablePath);

            //if (savedFileInfo.Length==0 || savedFileInfo.FilePath ==null)
            //    return BadRequest(savedFileInfo.FileName);
            //return Ok(new
            //{
            //    savedFileInfo.FileName,
            //    savedFileInfo.Length,
            //    FilePath = Url.Action("GetFile", "UploadFile", new { fileName = savedFileInfo.FileName })
            //});
        }
        [HttpGet("GetFile/{fileName}")]
        public IActionResult GetFile2(string fileName)
        {
            var safeFileName = Path.GetFileName(fileName); // 防止路徑穿越

            using (new NetworkConnection(FileTableUNCPath, new NetworkCredential(FileTableUser, FileTablePassword, FileTableDomain)))
            {
                var filePath = Path.Combine(FileTableUNCPath, safeFileName);

                if (!System.IO.File.Exists(filePath))
                    return NotFound();

                var contentType = GetContentType(filePath); // ✅ 正確 MIME 類型
                var fileBytes = System.IO.File.ReadAllBytes(filePath);


                Response.Headers["Cache-Control"] = "public,max-age=604800"; // 快取一週

                return File(fileBytes, contentType);
            }


            //var fileTablePath = @"\\localhost\mssqlserver\VitalBridgeDB\MyFileTableDir";
            //var filePath = Path.Combine(fileTablePath, safeFileName);

            //if (!System.IO.File.Exists(filePath))
            //    return NotFound();

            //var contentType = GetContentType(filePath); // ✅ 正確 MIME 類型
            //var fileBytes = System.IO.File.ReadAllBytes(filePath);
            //return File(fileBytes, contentType);
        }

        [HttpGet("GetFile")]
        public IActionResult GetFile(string fileName)
        {
            var safeFileName = Path.GetFileName(fileName); // 防止路徑穿越

            using (new NetworkConnection(FileTableUNCPath, new NetworkCredential(FileTableUser, FileTablePassword, FileTableDomain)))
            {
                var filePath = Path.Combine(FileTableUNCPath, safeFileName);

                if (!System.IO.File.Exists(filePath))
                    return NotFound();

                var contentType = GetContentType(filePath); // ✅ 正確 MIME 類型
                var fileBytes = System.IO.File.ReadAllBytes(filePath);


                Response.Headers["Cache-Control"] = "public,max-age=604800"; // 快取一週

                return File(fileBytes, contentType);
            }


            //var fileTablePath = @"\\localhost\mssqlserver\VitalBridgeDB\MyFileTableDir";
            //var filePath = Path.Combine(fileTablePath, safeFileName);

            //if (!System.IO.File.Exists(filePath))
            //    return NotFound();

            //var contentType = GetContentType(filePath); // ✅ 正確 MIME 類型
            //var fileBytes = System.IO.File.ReadAllBytes(filePath);
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
