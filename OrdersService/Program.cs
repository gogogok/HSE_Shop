using OrdersService.Data;
using OrdersService.Messaging;
using OrdersService.Options;
using OrdersService.Services;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;


WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<KafkaOptions>(builder.Configuration.GetSection("Kafka"));
builder.Services.AddControllers()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<OrdersDbContext>(opt =>
{
    opt.UseNpgsql(builder.Configuration.GetConnectionString("Db"));

});

builder.Services.AddScoped<OrdersAppService>();
builder.Services.AddSingleton<IJsonMessageSerializer, JsonMessageSerializer>();
builder.Services.AddSingleton<IKafkaProducerFactory, KafkaProducerFactory>();
builder.Services.AddHostedService<OutboxPublisherHostedService>();
builder.Services.AddHostedService<PaymentResultConsumerHostedService>();

builder.Services.AddHttpClient("payments", c =>
{
    c.BaseAddress = new Uri("http://paymentsservice:8080");
});


WebApplication app = builder.Build();

using (IServiceScope scope = app.Services.CreateScope())
{
    OrdersDbContext db = scope.ServiceProvider.GetRequiredService<OrdersService.Data.OrdersDbContext>();
    db.Database.Migrate();
}

app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/orders/health", () => Results.Ok("ok"));
app.MapControllers();

app.Run();