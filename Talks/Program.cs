using Microsoft.EntityFrameworkCore;
using Talks.Data;
using Talks.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseOrleans(siloBuilder =>
{
    siloBuilder.UseConsulSiloClustering(options =>
    {
        var address = new Uri("http://consul:8500");
        options.ConfigureConsulClient(address);
    });
    siloBuilder.AddMemoryGrainStorageAsDefault();
    siloBuilder.UseDashboard(options =>
    {
        options.BasePath = "/dash";
    });
});

builder.Services.AddDbContext<MetersDbContext>(o =>
{
    o.UseNpgsql(builder.Configuration.GetConnectionString(nameof(MetersDbContext)),
            pg => pg.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery))
        .UseSnakeCaseNamingConvention();

    o.EnableDetailedErrors()
        .EnableSensitiveDataLogging()
        .LogTo(Console.WriteLine,
            new[] { DbLoggerCategory.Database.Command.Name },
            LogLevel.Information);
}, ServiceLifetime.Transient, ServiceLifetime.Singleton);

builder.Services.AddTransient<Func<MetersDbContext>>(sp => sp.GetRequiredService<MetersDbContext>);
// Add services to the container.
builder.Services.AddGrpc();
builder.Services.AddGrpcReflection();

var app = builder.Build();

app.Services.GetRequiredService<MetersDbContext>().Database.EnsureCreated();

app.MapGrpcReflectionService();

// Configure the HTTP request pipeline.
app.MapGrpcService<DirectMetersService>();
app.MapGrpcService<OrleansMetersService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();
