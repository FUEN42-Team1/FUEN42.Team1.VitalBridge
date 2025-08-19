using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Text;
using Team1.VitalBridge.Frontend.Interfaces;
using Team1.VitalBridge.Frontend.Models.EFModels;
using Team1.VitalBridge.Frontend.Models.Services;

namespace Team1.VitalBridge.Frontend
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var cfg = builder.Configuration;//加的
            // Add services to the container.

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
                
                options.AddPolicy("FileProtocolPolicy", policy =>
                {
                    policy
                        .SetIsOriginAllowed(_ => true)  // 允許所有來源，包括 file://
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials();        // 支援認證 cookies
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

            // 6. HTTP ???A?? - ????O?? API ????????
            builder.Services.AddHttpClient();

            // 7. ??x?]?w
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




            // CORS：允許帶 Cookie（Credentials）
            builder.Services.AddCors(o =>
            {
                o.AddPolicy("FE", p => p
                    .WithOrigins(cfg.GetSection("Cors:Frontend").Get<string[]>())
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials());
            });

            // JWT（驗 Access Token）
            builder.Services.AddAuthentication("Bearer")
                .AddJwtBearer(opt =>
                {
                    opt.TokenValidationParameters = new()
                    {
                        ValidIssuer = cfg["Jwt:Issuer"],
                        ValidAudience = cfg["Jwt:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(cfg["Jwt:AccessKey"]!)),
                        ClockSkew = TimeSpan.FromSeconds(30)
                    };
                });



            //DI注入
            builder.Services.AddDbContext<AppDBContext>(options =>
options.UseSqlServer(
builder.Configuration.GetConnectionString("DefaultConnection"),
sqlServerOptions => sqlServerOptions.EnableRetryOnFailure(
maxRetryCount: 10,  // 最多重試 10 次
maxRetryDelay: TimeSpan.FromSeconds(30), // 重試之間的延遲時間
errorNumbersToAdd: null // null 表示使用預設的 SQL Server 錯誤碼
)
)
);

            builder.Services.AddScoped<IJwtService, JwtService>();
            builder.Services.AddScoped<IAuthService, AuthService>();











            builder.Services.AddHttpContextAccessor();
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








            app.UseCors("FE");// 允許前端跨域請求，並帶上 Cookie（Credentials）
            app.UseAuthentication();// 啟用身份驗證
            app.UseAuthorization();


            app.MapControllers();



            app.Run();
        }
    }
}
