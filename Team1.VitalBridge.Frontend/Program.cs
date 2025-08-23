using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Text;
using Team1.VitalBridge.Frontend.Hubs;
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

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // 註冊 HttpClient 服務（為 ImageProxyController 使用）
            builder.Services.AddHttpClient();
            //註冊 SignalR 的服務
            builder.Services.AddSignalR();
            // CORS：允許帶 Cookie（Credentials）
            builder.Services.AddCors(o =>
            {
                o.AddPolicy("FE", p => p
                    .WithOrigins(cfg.GetSection("Cors:Frontend").Get<string[]>())
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials());
            });

            //讀取Google登入設定
//            var googleConfig = builder.Configuration.GetSection("GoogleLogin");

//            builder.Services.AddAuthentication(options =>
//            {
//                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
//                options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
//            })
//.AddCookie()
//.AddGoogle(options =>
//{
//    options.ClientId = googleConfig["ClientId"];
//    options.ClientSecret = googleConfig["ClientSecret"];
//    options.CallbackPath = "/api/auth/google/callback";
//});



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

            builder.Services.AddScoped<IJwtService, JwtService>();
            builder.Services.AddScoped<IAuthService, AuthService>();

            builder.Services.AddHttpContextAccessor();
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseCors("FE");// 允許前端跨域請求，並帶上 Cookie（Credentials）
            app.UseAuthentication();// 啟用身份驗證
            app.UseAuthorization();
            app.MapHub<ChatHub>("/chathub");
            app.MapControllers();

            app.Run();
        }
    }
}
