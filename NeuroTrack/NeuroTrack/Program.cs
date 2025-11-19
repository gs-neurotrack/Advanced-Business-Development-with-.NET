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
        
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        
        builder.Services.AddDbContext<NeuroTrackContext>();

        builder.Services.AddScoped<IGsDailyLogsRepository, GsDailyLogsRepository>();
        builder.Services.AddScoped<IGsLimitsRepository, GsLimitsRepository>();
        builder.Services.AddScoped<IGsScoresRepository, GsScoresRepository>();

        builder.Services.AddScoped<IGsDailyLogsServices, GsDailyLogsServices>();
        builder.Services.AddScoped<IGsLimitsServices, GsLimitsServices>();
        builder.Services.AddScoped<IGsScoresServices, GsScoresServices>();
        
        var app = builder.Build();
        
        app.UseSwagger(); 
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "NeuroTrack API v1");
            c.RoutePrefix = "swagger";
        });

        app.MapControllers();

        app.Run();
    }
}