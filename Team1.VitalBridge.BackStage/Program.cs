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

namespace Team1.VitalBridge.BackStage
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

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
            //Johnny end
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlServerOptions => sqlServerOptions.EnableRetryOnFailure(
            maxRetryCount: 10,  // 最多重試 10 次
            maxRetryDelay: TimeSpan.FromSeconds(30), // 重試之間的延遲時間
            errorNumbersToAdd: null // null 表示使用預設的 SQL Server 錯誤碼
        )
    )
);




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



            var jwtSettings = builder.Configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["Secret"]; // 從 appsettings.json 讀取秘密金鑰

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "MemberJwtScheme";
                options.DefaultChallengeScheme = "MemberJwtScheme";
                //options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                //options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

            })
           .AddCookie("ExternalCookie")
           // --- 使用輔助方法配置 JWT-in-Cookie 驗證 ---
           .AddJwtBearer("MemberJwtScheme", ConfigureJwtBearerOptions("member_auth_token", jwtSettings["MemberAudience"], jwtSettings))
           .AddJwtBearer("InstitutionJwtScheme", ConfigureJwtBearerOptions("institution_auth_token", jwtSettings["InstitutionAudience"], jwtSettings))
           .AddJwtBearer("AdminJwtScheme", ConfigureJwtBearerOptions("admin_auth_token", jwtSettings["AdminAudience"], jwtSettings))
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

            app.UseRouting();
            app.MapControllers();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }


        static Action<JwtBearerOptions> ConfigureJwtBearerOptions(string cookieName, string validAudience, IConfigurationSection jwtSettings)
        {
            return options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,

                    ValidIssuer = jwtSettings["Issuer"],
                    ValidAudience = validAudience, // 從參數傳入
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Secret"])),
                    ClockSkew = TimeSpan.Zero,
                    NameClaimType = ClaimTypes.Name,
                    RoleClaimType = ClaimTypes.Role
                };

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        // 從參數傳入的 cookieName 中讀取 Token 並作驗證
                        context.Token = context.Request.Cookies[cookieName];
                        return Task.CompletedTask;
                    },
                    OnAuthenticationFailed = context =>
                    {   // 驗證失敗時的處理
                        Console.WriteLine($"JWT Authentication for {cookieName} failed: {context.Exception.Message}");
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = context =>
                    {
                        // 驗證成功時的處理
                        Console.WriteLine($"JWT Token for {cookieName} successfully validated!");
                        return Task.CompletedTask;
                    },
                    OnChallenge = async context =>
                    {
                        context.HandleResponse();
                        // 直接進行重定向，不區分請求類型
                        var loginPath = "";
                        // ... (您的 loginPath 判斷邏輯保持不變) ...
                        if (cookieName == "member_auth_token")
                        {
                            loginPath = "/Auth/Login";
                        }
                        else if (cookieName == "institution_auth_token")
                        {
                            loginPath = "/Institution/InstitutionAuth/Login";
                        }
                        else if (cookieName == "admin_auth_token")
                        {
                            loginPath = "/Admin/AdminAuth/Login";
                        }
                        else
                        {
                            loginPath = "/Auth/Login";
                        }
                        context.Response.Redirect(loginPath);
                        // context.Response.Redirect(loginPath + "?ReturnUrl=" + context.Request.Path + context.Request.QueryString);
                    }



                };
            };
        }





    }






}
