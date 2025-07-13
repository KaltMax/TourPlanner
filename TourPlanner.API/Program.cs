using Microsoft.EntityFrameworkCore;
using TourPlanner.BLL.Services;
using TourPlanner.BLL.Interfaces;
using TourPlanner.BLL.Mappers;
using TourPlanner.DAL.Repositories;
using TourPlanner.DAL.Interfaces;
using TourPlanner.DAL.Context;
using TourPlanner.BLL.Configurations;
using TourPlanner.Logging;

namespace TourPlanner.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Configure logging with log4net
            builder.Logging.ClearProviders();
            builder.Logging.AddLog4Net("log4net.config");
            // register your wrapper for DI, so every layer can inject ILoggerWrapper<T>:
            builder.Services.AddSingleton(typeof(ILoggerWrapper<>), typeof(Log4NetLoggerWrapper<>));

            // Add services to the container.
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // Add PostgreSQL EF DbContext
            builder.Services.AddDbContext<TourPlannerDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("TourPlannerDb")));

            // Register BLL and DAL Services
            builder.Services.AddScoped<ITourService, TourService>();
            builder.Services.AddScoped<ITourRepository, TourRepository>();
            builder.Services.AddScoped<ITourLogService, TourLogService>();
            builder.Services.AddScoped<ITourLogRepository, TourLogRepository>();

            // Register the Mapper
            builder.Services.AddScoped<IMapper, Mapper>();

            // Bind OpenRouteService API Key from appsettings.json
            builder.Services.Configure<OpenRouteServiceOptions>(builder.Configuration.GetSection("OpenRouteService"));

            // Register RouteService and inject HttpClient
            builder.Services.AddHttpClient<IRouteService, RouteService>();

            var app = builder.Build();

            // Apply migrations at startup
            using (var scope = app.Services.CreateScope())
            {
                
                var dbContext = scope.ServiceProvider.GetRequiredService<TourPlannerDbContext>();
                var dbLogger = scope.ServiceProvider.GetRequiredService<ILoggerWrapper<Program>>();

                try
                {
                    dbLogger.LogInformation("Applying database migrations on startup");
                    dbContext.Database.Migrate();
                    dbLogger.LogInformation("Database migrations applied successfully");
                }
                catch (Exception ex)
                {
                    dbLogger.LogError(ex, "An error occured while applying database migrations");
                }
                
            }

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();

            var appLogger = app.Services.GetRequiredService<ILoggerWrapper<Program>>();
            appLogger.LogInformation("TourPlanner API is starting up");

            app.Run();
        }
    }
}
