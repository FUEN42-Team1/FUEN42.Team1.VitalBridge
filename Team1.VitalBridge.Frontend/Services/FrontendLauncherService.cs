using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Team1.VitalBridge.Frontend.Services
{
    /// <summary>
    /// 前端自動啟動服務
    /// 在開發環境下自動開啟 org-search.html 檔案
    /// </summary>
    public class FrontendLauncherService : IHostedService
    {
        private readonly ILogger<FrontendLauncherService> _logger;
        private readonly IWebHostEnvironment _environment;
        private readonly IConfiguration _configuration;
        private Timer? _timer;

        // HTML 檔案路徑
        private const string HtmlFilePath = @"c:\Users\User\Desktop\Team1.VitalBridge.Frontend\org-search.html";

        public FrontendLauncherService(
            ILogger<FrontendLauncherService> logger,
            IWebHostEnvironment environment,
            IConfiguration configuration)
        {
            _logger = logger;
            _environment = environment;
            _configuration = configuration;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("FrontendLauncherService 啟動中...");

            // 只在開發環境執行
            if (!_environment.IsDevelopment())
            {
                _logger.LogInformation("非開發環境，跳過自動啟動 HTML 檔案");
                return Task.CompletedTask;
            }

            // 延遲 5 秒後執行
            _timer = new Timer(LaunchHtmlFile, null, TimeSpan.FromSeconds(5), Timeout.InfiniteTimeSpan);

            _logger.LogInformation("已設定 5 秒後自動開啟 org-search.html");
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("FrontendLauncherService 正在停止...");
            
            _timer?.Change(Timeout.Infinite, 0);
            _timer?.Dispose();

            return Task.CompletedTask;
        }

        private void LaunchHtmlFile(object? state)
        {
            try
            {
                _logger.LogInformation("嘗試開啟 HTML 檔案: {FilePath}", HtmlFilePath);

                // 檢查檔案是否存在
                if (!File.Exists(HtmlFilePath))
                {
                    _logger.LogWarning("HTML 檔案不存在: {FilePath}", HtmlFilePath);
                    return;
                }

                // 根據作業系統開啟檔案
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    // Windows: 使用預設瀏覽器開啟
                    var processStartInfo = new ProcessStartInfo
                    {
                        FileName = HtmlFilePath,
                        UseShellExecute = true, // 使用系統預設程式開啟
                        Verb = "open"
                    };

                    Process.Start(processStartInfo);
                    _logger.LogInformation("已成功開啟 HTML 檔案: {FilePath}", HtmlFilePath);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    // Linux: 使用 xdg-open
                    Process.Start("xdg-open", HtmlFilePath);
                    _logger.LogInformation("已使用 xdg-open 開啟 HTML 檔案: {FilePath}", HtmlFilePath);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    // macOS: 使用 open
                    Process.Start("open", HtmlFilePath);
                    _logger.LogInformation("已使用 open 開啟 HTML 檔案: {FilePath}", HtmlFilePath);
                }
                else
                {
                    _logger.LogWarning("不支援的作業系統，無法自動開啟 HTML 檔案");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "開啟 HTML 檔案時發生錯誤: {Message}", ex.Message);
            }
            finally
            {
                // 一次性執行，清理 timer
                _timer?.Dispose();
                _timer = null;
            }
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}