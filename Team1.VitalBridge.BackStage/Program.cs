using Team1.VitalBridge.BackStage.Models.Interface;
using Team1.VitalBridge.BackStage.Models.Repository;
using Team1.VitalBridge.BackStage.Models.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using Team1.VitalBridge.BackStage.Models.EFModels;
using Team1.VitalBridge.BackStage.Models.Repositories;
using Team1.VitalBridge.BackStage.Models.Services;
using Team1.VitalBridge.BackStage.Models.Interface;
using Team1.VitalBridge.BackStage.Models.Repository;
using Team1.VitalBridge.BackStage.Models.Service;
using Team1.VitalBridge.BackStage.Models.Interfaces;

namespace Team1.VitalBridge.BackStage
{ 
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            // 添加 CORS 服務 - 支援 file:// 協議
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowFileProtocol", policy =>
                {
                    policy
                        .SetIsOriginAllowed(_ => true)  // 允許所有來源，包括 file://
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials();
                });
            });

			// 註冊 IProductCategoryRepository,ProductCategoryService>到DI
			builder.Services.AddScoped<IProductCategoryRepository, ProductCategoryRepository>();
			builder.Services.AddScoped<ProductCategoryService>();

			// 獲取 appsettings.json 中名為 "DefaultConnection" 的連線字串
			var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

            //// 註冊 AppDbContext 服務
            //// 使用 SQL Server 資料庫，並傳入連線字串
            //builder.Services.AddDbContext<AppDbContext>(options =>
            //    options.UseSqlServer(connectionString));

            // 註冊 AppDbContext
            //builder.Services.AddDbContext<AppDbContext>(options =>
            //    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("DefaultConnection"),
                    sqlServerOptions => sqlServerOptions.EnableRetryOnFailure(
                        maxRetryCount: 10,  // 最多重試 10 次
                        maxRetryDelay: TimeSpan.FromSeconds(30), // 重試之間的延遲時間
                        errorNumbersToAdd: null // null 表示使用預設的 SQL Server 錯誤碼
                    )
                )
            );
			

			// Johnny start
			// 註冊 ContentCategoryService 和 ContentCategoryRepository
			builder.Services.AddScoped<IContentCategoryRepository, ContentCategoryRepository>();
            builder.Services.AddScoped<IContentCategoryService, ContentCategoryService>();
            // 註冊 ContentArticleService 和 ContentArticleRepository
            builder.Services.AddScoped<IContentArticleRepository, ContentArticleRepository>();
            builder.Services.AddScoped<IContentArticleService, ContentArticleService>();
            // 註冊 ContentCommentService 和 ContentCommentRepository
            builder.Services.AddScoped<IContentCommentRepository, ContentCommentRepository>();
            builder.Services.AddScoped<IContentCommentService, ContentCommentService>();
            // 註冊  MediaRepository
            builder.Services.AddScoped<IMediaRepository, MediaRepository>();
            //Johnny end






            builder.Services.AddScoped<IPlateRepository, PlateRepository>();
            builder.Services.AddScoped<PlateService>();
            builder.Services.AddScoped<IPlateImageRepository, PlateImageRepository>();
            builder.Services.AddScoped<PlateImageService>();
            builder.Services.AddScoped<INotifyRepository, NotifyRepository>();
            builder.Services.AddScoped<NotifyService>();
            builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
            builder.Services.AddScoped<CategoryService>();
            builder.Services.AddScoped<INotifyUserRepository, NotifyUserRepository>();
            builder.Services.AddScoped<NotifyUserService>();


            //KueiFu
            //註冊生成JWT Token 服務
            builder.Services.AddScoped<JwtService>();
            //縣市、鄉鎮服務
            builder.Services.AddScoped<LocationService>();



            var jwtSection = builder.Configuration.GetSection("JwtSettings");
            var secret = jwtSection["Secret"]; // 從 appsettings.json 讀取秘密金鑰
            var issuer = jwtSection["Issuer"];

            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "MemberJwtScheme";
                options.DefaultChallengeScheme = "MemberJwtScheme";
                //options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                //options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

            })
           .AddCookie("ExternalCookie")
           // --- 使用輔助方法配置 JWT-in-Cookie 驗證 ---
           .AddJwtBearer("MemberJwtScheme",ConfigureJwtBearerOptions("member_auth_token", "MemberJwtScheme", jwtSection, signingKey, issuer))
           .AddJwtBearer("InstitutionJwtScheme",ConfigureJwtBearerOptions("institution_auth_token", "InstitutionJwtScheme", jwtSection, signingKey, issuer))
           .AddJwtBearer("AdminJwtScheme",ConfigureJwtBearerOptions("admin_auth_token", "AdminJwtScheme", jwtSection, signingKey, issuer))
           .AddGoogle(options =>
           {
               options.SignInScheme = "ExternalCookie";
               options.ClientId = builder.Configuration["GoogleLogin:ClientId"];
               options.ClientSecret = builder.Configuration["GoogleLogin:ClientSecret"];

               options.Events.OnRemoteFailure = (context) =>
               {
                   context.HandleResponse();
                   var errorMessage = "Google 登入失敗。";
                   if (context.Failure != null)
                   {
                       // 判斷是否為使用者拒絕授權 (access_denied)
                       if (context.Failure.Message.Contains("access_denied", StringComparison.OrdinalIgnoreCase))
                       {
                           errorMessage = "您已取消 Google 登入。如果您想使用 Google 登入，請重試並授權我們的應用程式。";
                       }
                       // 您可以在這裡加入更多對 context.Failure.Message 的判斷，以提供更精確的錯誤訊息
                       Console.WriteLine($"Google 遠端驗證失敗: {context.Failure.Message}");
                   }
                   //顯示錯誤訊息給使用者

                   context.Response.Redirect("/Auth/Login");

                   return Task.CompletedTask; // 表示非同步操作已完成
               };
           });





            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            // 添加 CORS 中間件 - 必須在 UseRouting 之前
            app.UseCors("AllowFileProtocol");

            app.UseRouting();
            app.MapControllers();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            // ================== 應用程式啟動 ==================
            
            // 記錄啟動資訊
            var logger = app.Services.GetRequiredService<ILogger<Program>>();
            logger.LogInformation("VitalBridge BackStage 後台管理系統正在啟動...");
            logger.LogInformation("環境: {Environment}", app.Environment.EnvironmentName);
            logger.LogInformation("後台管理端點: https://localhost:7242");
            logger.LogInformation("?? 後台機構管理功能已準備就緒");

            app.Run();
        }


        static Action<JwtBearerOptions> ConfigureJwtBearerOptions(string cookieName,string audienceKey,IConfigurationSection jwtSettings, SymmetricSecurityKey signingKey,string issuer                          
)
        {
            return options =>
            {
                // 從 Audiences 字典讀取對應的 audience
                var validAudience = jwtSettings[$"Audiences:{audienceKey}"];

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = issuer,

                    ValidateAudience = true,
                    ValidAudience = validAudience,   //  走 Audiences:<key>

                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = signingKey,

                    // 建議保留一點 clock skew，避免極端邊界誤差
                    ClockSkew = TimeSpan.FromMinutes(1),

                    NameClaimType = ClaimTypes.Name,
                    RoleClaimType = ClaimTypes.Role
                };

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        // 只有當 Header 沒有 Bearer token 時，才從 Cookie 補
                        if (string.IsNullOrWhiteSpace(context.Token))
                        {
                            if (context.Request.Cookies.TryGetValue(cookieName, out var token) && !string.IsNullOrWhiteSpace(token))
                            {
                                context.Token = token;
                            }
                        }
                        return Task.CompletedTask;
                    },
                    OnAuthenticationFailed = context =>
                    {
                        Console.WriteLine($"JWT Authentication for {cookieName} failed: {context.Exception.Message}");
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = context =>
                    {
                        Console.WriteLine($"JWT Token for {cookieName} successfully validated!");
                        return Task.CompletedTask;
                    },
                    OnChallenge = async context =>
                    {
                        // 若是 API 請求（接收 JSON），給 401 JSON；否則導去對應登入頁
                        context.HandleResponse();

                        var acceptsJson = context.Request.Headers["Accept"].ToString().Contains("application/json", StringComparison.OrdinalIgnoreCase)
                                          || string.Equals(context.Request.Headers["X-Requested-With"], "XMLHttpRequest", StringComparison.OrdinalIgnoreCase);

                        if (acceptsJson)
                        {
                            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                            context.Response.ContentType = "application/json; charset=utf-8";
                            await context.Response.WriteAsync("{\"error\":\"unauthorized\"}");
                            return;
                        }

                        string loginPath = cookieName switch
                        {
                            "member_auth_token" => "/Auth/Login",
                            "institution_auth_token" => "/Institution/InstitutionAuth/Login",
                            "admin_auth_token" => "/Admin/AdminAuth/Login",
                            _ => "/Auth/Login"
                        };

                        // 帶回 ReturnUrl（可視需求決定是否加）
                        var returnUrl = Uri.EscapeDataString(context.Request.Path + context.Request.QueryString);
                        context.Response.Redirect($"{loginPath}?ReturnUrl={returnUrl}");
                    }
                };
            };
        }







    }






}
