using Microsoft.EntityFrameworkCore;
using Spottwit.Server.Data;
using Spottwit.Server.Extensions;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.Seq(Environment.GetEnvironmentVariable("SEQ_URL") ?? "http://localhost:5341")
    .CreateBootstrapLogger();

try
{
    Log.Information("App Started...");
    
    var builder = WebApplication.CreateBuilder(args);
    var redisConnectionString = builder.Configuration.GetConnectionString("Redis") ?? "localhost:6380";
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.Seq("http://seq:5341"));

    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseNpgsql(connectionString));

    builder.Services.AddControllers();
    builder.Services.AddApplicationServices();
    
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = redisConnectionString;
    });

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    var app = builder.Build();

    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        dbContext.Database.Migrate();
    }

    app.UseDefaultFiles();
    app.UseStaticFiles();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();

    app.UseAuthorization();

    app.MapGet("/api/test-log", (ILogger<Program> logger) =>
    {
        logger.LogInformation("Hello! Test log in Seq. Time: {Time}", DateTime.UtcNow);
        logger.LogWarning("Test Warning (Warning).");

        try
        {
            throw new InvalidOperationException("Test exception Seq!");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Test catch Seq.");
        }

        return Results.Ok(new { message = "Logs added!" });
    });

    app.MapControllers();

    app.MapFallbackToFile("/index.html");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application shuts down at start due to unexpected issues!");
}
finally
{
    Log.CloseAndFlush();
}