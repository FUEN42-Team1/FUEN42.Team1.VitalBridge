using Microsoft.EntityFrameworkCore;
using Team1.VitalBridge.BackStage.Models.EFModels;
using Team1.VitalBridge.BackStage.Models.Interfaces;
using Team1.VitalBridge.BackStage.Models.Repositories;
using Team1.VitalBridge.BackStage.Models.Services;

namespace Team1.VitalBridge.BackStage
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            //// 獲取 appsettings.json 中名為 "DefaultConnection" 的連線字串
            //var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

            //// 註冊 AppDbContext 服務
            //// 使用 SQL Server 資料庫，並傳入連線字串
            //builder.Services.AddDbContext<AppDbContext>(options =>
            //    options.UseSqlServer(connectionString));

            // 註冊 AppDbContext
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            //Johnny
            // 註冊 ContentTypeCategoryPairRepository 服務
            builder.Services.AddScoped<IContentTypeCategoryPairRepository, ContentTypeCategoryPairRepository>();

            // 註冊 ContentTypeCategoryPairService 服務
            builder.Services.AddScoped<IContentTypeCategoryPairService, ContentTypeCategoryPairService>();

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

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
