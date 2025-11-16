using NeuroTrack.Context;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers();
        
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        
        builder.Services.AddDbContext<NeuroTrackContext>();
        
        var app = builder.Build();
        
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<NeuroTrackContext>();

            var count = db.GsUser.Count();

            Console.WriteLine($"Usuários cadastrados: {count}");
        }
        
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