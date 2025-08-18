using Microsoft.EntityFrameworkCore;
using Team1.VitalBridge.BackStage.Models.EFModels;
using Team1.VitalBridge.Frontend.Middleware;
using Team1.VitalBridge.Frontend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Team1.VitalBridge.Frontend
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ================== 服務註冊 ==================

            // 1. 資料庫服務 - Entity Framework (包含前端搜尋索引配置)
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("DefaultConnection"),
                    sqlServerOptions => sqlServerOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,  // 最多重試 5 次
                        maxRetryDelay: TimeSpan.FromSeconds(30), // 最大重試延遲時間
                        errorNumbersToAdd: null // 使用預設的 SQL Server 錯誤號碼
                    )
                )
            );

            // 2. CORS 設定 - 允許本地 HTML 檔案存取
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("FrontendPolicy", policy =>
                {
                    policy
                        .AllowAnyOrigin()           // 允許任何來源（包括 file:// 和 null origin）
                        .AllowAnyMethod()           // 允許所有 HTTP 方法
                        .AllowAnyHeader();          // 允許所有標頭
                });
                
                options.AddPolicy("StrictPolicy", policy =>
                {
                    policy
                        .WithOrigins("https://localhost:7184", "http://localhost:5184", "file://")
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials();        // 支援認證 cookies
                });
            });

            // 3. 控制器服務
            builder.Services.AddControllers(options =>
            {
                // 自訂模型綁定設定
                options.SuppressAsyncSuffixInActionNames = false;
            })
            .ConfigureApiBehaviorOptions(options =>
            {
                // 自訂模型驗證錯誤回應
                options.InvalidModelStateResponseFactory = context =>
                {
                    var errors = context.ModelState
                        .Where(x => x.Value?.Errors.Count > 0)
                        .SelectMany(x => x.Value!.Errors)
                        .Select(x => x.ErrorMessage)
                        .ToList();

                    var response = new
                    {
                        Success = false,
                        Message = "資料驗證失敗",
                        ErrorCode = "VALIDATION_ERROR",
                        Data = errors,
                        Timestamp = DateTime.UtcNow
                    };

                    return new BadRequestObjectResult(response);
                };
            });

            // 4. API 文件服務 - Swagger
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
                {
                    Title = "VitalBridge Frontend API",
                    Version = "v1",
                    Description = "VitalBridge 前端機構搜尋 API (包含高效能索引優化)",
                    Contact = new Microsoft.OpenApi.Models.OpenApiContact
                    {
                        Name = "VitalBridge Team",
                        Email = "support@vitalbridge.com"
                    }
                });

                // 加入 XML 註解
                var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                if (File.Exists(xmlPath))
                {
                    options.IncludeXmlComments(xmlPath);
                }
            });

            // 5. 背景服務 - 自動啟動 HTML
            builder.Services.AddHostedService<FrontendLauncherService>();

            // 6. 日誌設定
            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();
            builder.Logging.AddDebug();
            
            if (builder.Environment.IsDevelopment())
            {
                builder.Logging.SetMinimumLevel(LogLevel.Information);
            }
            else
            {
                builder.Logging.SetMinimumLevel(LogLevel.Warning);
            }

            // ================== 應用程式建置 ==================
            var app = builder.Build();

            // ================== 中間件管道設定 ==================

            // 1. 全域錯誤處理（必須在最前面）
            app.UseGlobalExceptionHandler();

            // 2. 開發環境特定中間件
            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/swagger/v1/swagger.json", "VitalBridge Frontend API v1");
                    options.RoutePrefix = "swagger"; // Swagger UI 路徑
                    options.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List);
                });
            }
            else
            {
                // 生產環境錯誤處理
                app.UseExceptionHandler("/error");
                app.UseHsts(); // HTTP Strict Transport Security
            }

            // 3. 安全性中間件
            app.UseHttpsRedirection();

            // 4. CORS 中間件（必須在 Authorization 之前）
            app.UseCors("FrontendPolicy");

            // 5. 靜態檔案服務（如果需要）
            app.UseStaticFiles();

            // 6. 路由中間件
            app.UseRouting();

            // 7. 授權中間件
            app.UseAuthorization();

            // 8. 控制器路由
            app.MapControllers();

            // 9. 預設路由重定向到 Swagger（開發環境）
            if (app.Environment.IsDevelopment())
            {
                app.MapGet("/", () => Results.Redirect("/swagger"));
            }

            // ================== 應用程式啟動 ==================
            
            // 記錄啟動資訊
            var logger = app.Services.GetRequiredService<ILogger<Program>>();
            logger.LogInformation("VitalBridge Frontend API 正在啟動...");
            logger.LogInformation("環境: {Environment}", app.Environment.EnvironmentName);
            logger.LogInformation("API 端點: https://localhost:7184");
            logger.LogInformation("?? 已整合前端搜尋效能索引配置 (13個高效能索引)");
            
            app.Run();
        }
    }
}
