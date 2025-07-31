using Azure;
using Microsoft.VisualBasic;
using static System.Net.Mime.MediaTypeNames;

namespace MvcTips.Site.Models.Utilities
{
	public class UploadFileHelper
	{
		public static async Task<UploadedFileInfo> SaveUploadedFile(IFormFile file, string targetPath)
		{
			if (file == null || file.Length == 0)
			{
				return null;
			}

			// 取原始副檔名
			string extension = Path.GetExtension(file.FileName);
            // 使用 Guid 產生新檔名
            string fileName = $"{Guid.NewGuid().ToString("N")}{extension}";

			string filePath = Path.Combine(targetPath, fileName);
			if (!Directory.Exists(targetPath))
			{
				Directory.CreateDirectory(targetPath);
			}

			using (var stream = new FileStream(filePath, FileMode.Create))
			{
				await file.CopyToAsync(stream);
			}

			return new UploadedFileInfo
			{
				FileName = fileName,
				Length = file.Length,
				FilePath = filePath
			};
		}
	}

	public class UploadedFileInfo
	{
		public string FileName { get; set; }
		public long Length { get; set; }
		public string FilePath { get; set; }
	}
}
