using MassTransit;
using MessageConsumer;
using MessageConsumer.Services;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using Serilog;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

builder.Host
    .UseSerilog((context, loggerConfiguration) =>
    {
        loggerConfiguration
            .ReadFrom.Configuration(context.Configuration);
    })
    .ConfigureServices(services =>
    {
        const string metricName = "RedditPayload";
        services
            .AddSingleton<IMetrics>(new Metrics(metricName))
            .AddOpenTelemetryMetrics(opts => opts
                .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(Assembly.GetExecutingAssembly().GetName().Name))
                .AddMeter(metricName)
                .AddAspNetCoreInstrumentation()
                .AddRuntimeInstrumentation()
                .AddProcessInstrumentation()
                .AddOtlpExporter((exporterOptions, metricReaderOptions) =>
                {
                    exporterOptions.Endpoint = new Uri(builder.Configuration["Otlp:Endpoint"]);
                    metricReaderOptions.PeriodicExportingMetricReaderOptions.ExportIntervalMilliseconds = 10000;
                })
            );

        
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

                cfg.ReceiveEndpoint("redditMessages", ep =>
                {
                    ep.PrefetchCount = 16;
                    ep.UseMessageRetry(r => r.Interval(2, 100));
                    ep.ConfigureConsumer<RedditLiveThreadPayloadConsumer>(context);
                });

            });

            x.AddConsumer<RedditLiveThreadPayloadConsumer>();
        });

    });

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();


app.Run();
