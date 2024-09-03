using ChatSystem.Coordinator.App.Configurations;
using ChatSystem.Coordinator.App.Consumers;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ChatSystem.Chat.Client.Extensions;
using ChatSystem.Chat.Client.Configurations;
using ChatSystem.Coordinator.App.Services;


var host = Host.CreateDefaultBuilder(args)
    .ConfigureHostConfiguration(configHost => configHost.AddEnvironmentVariables())
    .ConfigureAppConfiguration((context, config) =>
    {
        // Load the appsettings.json and appsettings.{Environment}.json files
        config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
              .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional: true, reloadOnChange: true)
              .AddEnvironmentVariables();
    })
    .ConfigureServices((context, services) =>
    {
        ChatApiClientConfiguration chatApiClientConfiguration = new ChatApiClientConfiguration();
        context.Configuration.Bind("ChatApiClientConfiguration", chatApiClientConfiguration);
        services.AddChatApiClient(chatApiClientConfiguration);
        //services.AddDistributedMemoryCache();

        services.AddStackExchangeRedisCache(redisOptions =>
        {
            string connection = context.Configuration.GetConnectionString("RedisConnection")!;
            redisOptions.Configuration = connection;

        });

        services.Configure<RabbitMqConfiguration>(context.Configuration.GetSection(nameof(RabbitMqConfiguration)));
        services.Configure<ConnectionStrings>(context.Configuration.GetSection(nameof(ConnectionStrings)));

        RabbitMqConfiguration rabbitMqConfiguration = new RabbitMqConfiguration();
        context.Configuration.Bind("RabbitMqConfiguration", rabbitMqConfiguration);

        var rabbitMqSettings = context.Configuration.GetSection("RabbitMqConfiguration").Get<RabbitMqConfiguration>();

        services.AddScoped<ICachingService, CachingService>();
        services.AddScoped<IRedisQueue, RedisQueue>();

        // Register MassTransit
        services.AddMassTransit(x =>
        {
            x.AddConsumer<SessionCreatedConsumer>();
            x.AddConsumer<OverflowAgentsConsumer>();
            x.AddConsumer<SessionEndedConsumer>();

            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(rabbitMqConfiguration.HostName, h =>
                {
                    h.Username(rabbitMqConfiguration.Username);
                    h.Password(rabbitMqConfiguration.Password);
                });

                // Configure FIFO queue
                cfg.ReceiveEndpoint("sessions-queue", e =>
                {
                    e.ConfigureConsumer<SessionCreatedConsumer>(context);
                    e.PrefetchCount = 1; // Ensure only one message is processed at a time for FIFO
                });

                cfg.ReceiveEndpoint("overflow-agents-queue", e =>
                {
                    e.ConfigureConsumer<OverflowAgentsConsumer>(context);
                    e.PrefetchCount = 1; // Ensure only one message is processed at a time for FIFO
                });

                cfg.ReceiveEndpoint("session-ends-queue", e =>
                {
                    e.ConfigureConsumer<SessionEndedConsumer>(context);
                    e.PrefetchCount = 1; // Ensure only one message is processed at a time for FIFO
                });
            });
        });

    })
    .ConfigureLogging(logging =>
    {
        logging.ClearProviders();
        logging.AddConsole();
    })
    .Build();

// Run the application
await host.RunAsync();

// Define services and classes below