using NeuroTrack.Context;
using NeuroTrack.Repositories;
using NeuroTrack.Repositories.Interfaces;
using NeuroTrack.Services;
using NeuroTrack.Services.Interfaces;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers();

        // Swagger
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        // CORS configurado
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", policy =>
                policy
                    .AllowAnyOrigin()
                    .AllowAnyHeader()
                    .AllowAnyMethod()
            );
        });

        // DbContext
        builder.Services.AddDbContext<NeuroTrackContext>();

        // Repositories
        builder.Services.AddScoped<IGsDailyLogsRepository, GsDailyLogsRepository>();
        builder.Services.AddScoped<IGsLimitsRepository, GsLimitsRepository>();
        builder.Services.AddScoped<IGsScoresRepository, GsScoresRepository>();
        builder.Services.AddScoped<IGsPredictionsRepository, GsPredictionsRepository>();

        // Services
        builder.Services.AddScoped<IGsDailyLogsServices, GsDailyLogsServices>();
        builder.Services.AddScoped<IGsLimitsServices, GsLimitsServices>();
        builder.Services.AddScoped<IGsScoresServices, GsScoresServices>();
        builder.Services.AddScoped<IGsPredictionsServices, GsPredictionsServices>();

        var app = builder.Build();

        // Swagger
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "NeuroTrack API v1");
            c.RoutePrefix = "swagger";
        });

        // Middleware
        app.UseHttpsRedirection();
        app.UseCors("AllowAll");

        app.MapControllers();
        app.Run();
    }
}