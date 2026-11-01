using Serilog;
using Spottwit.Server.Data;
using MuslinkReact.Server.Extensions;
using Microsoft.EntityFrameworkCore;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.Seq(Environment.GetEnvironmentVariable("SEQ_URL") ?? "http://localhost:5341")
    .CreateBootstrapLogger();

try
{
    Log.Information("App Started...");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) => configuration
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.Seq("http://seq:5341"));

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    builder.Services.InstallServicesFromAssembly(builder.Configuration);

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

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapGet("/api/test-log", (ILogger<Program> logger) =>
    {
        logger.LogInformation("Hello! Test log in Seq. Time: {Time}", DateTime.UtcNow);
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