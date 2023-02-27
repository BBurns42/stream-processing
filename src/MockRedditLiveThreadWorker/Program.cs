using MassTransit;
using MockRedditLiveThreadWorker;

var builder = WebApplication.CreateBuilder(args);
builder.Host
    .ConfigureServices(services =>
    {
        services.AddHostedService<Worker>();

        var rabbitMq = $"rabbitmq://{Environment.GetEnvironmentVariable("RABBIT_HOST") ?? throw new ConfigurationException("RABBIT_HOST is invalid.")}";
        var rabbitUser = Environment.GetEnvironmentVariable("RABBIT_USER") ?? "guest";
        var rabbitPass = Environment.GetEnvironmentVariable("RABBIT_PASS") ?? "guest";

        services.AddMassTransit(x =>
        {
            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(new Uri(rabbitMq), h =>
                {
                    h.Username(rabbitUser);
                    h.Password(rabbitPass);
                });
            });
        });
        
        services.AddOptions<MessageQueueServiceOptions>().Configure(o => o.QueueUri = new Uri($"{rabbitMq}"));
        services.AddSingleton<IProducerService, ProducerService>();
    });

var app = builder.Build();

app.Run();