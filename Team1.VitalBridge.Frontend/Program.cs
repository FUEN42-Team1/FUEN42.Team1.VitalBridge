using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Security.Claims;
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
            //builder.Services.AddSwaggerGen();
            builder.Services.AddSwaggerGen(c =>
            {
                c.CustomSchemaIds(type => type.FullName); // 使用完整命名空間作為 schemaId
            });

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




            //// JWT（驗 Access Token）
            //builder.Services.AddAuthentication("Bearer")
            //    .AddJwtBearer(opt =>
            //    {
            //        opt.TokenValidationParameters = new()
            //        {
            //            ValidIssuer = cfg["Jwt:Issuer"],
            //            ValidAudience = cfg["Jwt:Audience"],
            //            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(cfg["Jwt:AccessKey"]!)),
            //            ClockSkew = TimeSpan.FromSeconds(60)
            //        };
            //    });



            builder.Services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(opt =>
            {
                opt.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = cfg["Jwt:Issuer"],
                    ValidAudience = cfg["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                                              Encoding.UTF8.GetBytes(cfg["Jwt:AccessKey"]!)
                                           ),

                    // 明確開啟驗證旗標（雖多數預設為 true，但寫出來避免環境差異）
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,

                    ClockSkew = TimeSpan.FromMinutes(2),  // 放寬一點避免時間飄移
                    NameClaimType = "sub",                // 你的 token 用 sub 作為 userId
                    RoleClaimType = ClaimTypes.Role       // token 內的 role 會映射到 User.IsInRole
                };

                // 最小日誌（可暫時留著除錯，用完可刪）
                opt.Events = new JwtBearerEvents
                {
                    OnTokenValidated = ctx =>
                    {
                        Console.WriteLine("[JWT OK] sub=" + ctx.Principal?.FindFirst("sub")?.Value);
                        return Task.CompletedTask;
                    },
                    OnAuthenticationFailed = ctx =>
                    {
                        Console.WriteLine("[JWT FAIL] " + ctx.Exception.Message);
                        return Task.CompletedTask;
                    }
                };
            });





            builder.Services.AddAuthorization();




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

            //KueiFu DI注入
            builder.Services.AddScoped<IJwtService, JwtService>();
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IMemberService, MemberService>();
            builder.Services.AddScoped<ILocationService, LocationService>();


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
