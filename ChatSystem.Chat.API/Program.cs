using ChatSystem.Chat.API.Handlers;
using ChatSystem.Chat.API.Layers.Application.Infrastructure.Common.Infrastructure;
using ChatSystem.Chat.API.Layers.Infrastructure;
using ChatSystem.Chat.API;
using Serilog;
using ChatSystem.Chat.API.Extensions;
using ChatSystem.Chat.API.Layers.Application.Configurations;
using MassTransit;
using ChatSystem.Chat.API.Layers.Application.Consumers;
using ChatSystem.Chat.API.Layers.Application.Infrastructure.Common.Application.Services;


Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

Log.Information("Starting up");

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Add services to the container.

    builder.Services.AddControllers();
    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    builder.Services.AddDistributedMemoryCache();
    builder.Services.RegisterCurrentUserService();
    builder.Services.RegisterApplicationServices();
    builder.Services.AddMediator();
    builder.Services.AddInfrastructureServices(builder.Configuration);
    builder.Services.AddProblemDetails();
    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

    builder.Services.AddStackExchangeRedisCache(redisOptions =>
    {
        string connection = builder.Configuration.GetConnectionString("RedisConnection")!;

        redisOptions.Configuration = connection;

    });

    builder.Services.Configure<RabbitMqConfiguration>(builder.Configuration.GetSection(nameof(RabbitMqConfiguration)));
    builder.Services.Configure<ConnectionStrings>(builder.Configuration.GetSection(nameof(ConnectionStrings)));

    RabbitMqConfiguration rabbitMqConfiguration = new RabbitMqConfiguration();
    builder.Configuration.Bind("RabbitMqConfiguration", rabbitMqConfiguration);
    // Configure MassTransit with RabbitMQ
    builder.Services.AddMassTransit(x =>
    {
        x.AddConsumer<ChatUpdateConsumer>();
        x.UsingRabbitMq((context, cfg) =>
        {
            cfg.Host(rabbitMqConfiguration.HostName, h =>
            {
                h.Username(rabbitMqConfiguration.Username);
                h.Password(rabbitMqConfiguration.Password);
            });

            cfg.ReceiveEndpoint("chat-update-queue", e =>
            {
                e.ConfigureConsumer<ChatUpdateConsumer>(context);
                e.PrefetchCount = 1;
            });

        });

    });

    builder.Services
    .AddApiVersioning(options =>
    {
        options.AssumeDefaultVersionWhenUnspecified = true;
    })
    .AddApiExplorer(options =>
    {
        // Add the versioned API explorer, which also adds IApiVersionDescriptionProvider service
        // note: the specified format code will format the version as "'v'major[.minor][-status]"
        options.GroupNameFormat = "'v'VVV";

        // note: this option is only necessary when versioning by url segment. the SubstitutionFormat
        // can also be used to control the format of the API version in route templates
        options.SubstituteApiVersionInUrl = true;
    });
    var app = builder.Build();
    app.UseExceptionHandler();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();

    app.UseAuthorization();

    app.MapControllers();

    using var scope = app.Services.CreateScope();
    var chatSeeder = new ChatSeeder(builder.Configuration.GetConnectionString("DatabaseConnection")!, scope.ServiceProvider.GetRequiredService<ICurrentUserService>(), scope.ServiceProvider.GetRequiredService<ICachingService>());

    await chatSeeder.SeedDefaultData();
    await chatSeeder.SeedDefaultCache();

    app.Run();

}
catch (Exception ex)
{
    Log.Fatal(ex, "Unhandled exception while bootstrapping application");
}
finally
{
    Log.Information("Shutting down...");
    Log.CloseAndFlush();
}