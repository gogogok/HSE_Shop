using PaymentsService.Options;
using PaymentsService.Services;
using Microsoft.EntityFrameworkCore;
using PaymentsService.Data;
using PaymentsService.Messaging;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

//Controllers
builder.Services.AddControllers();

//Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<KafkaOptions>(builder.Configuration.GetSection("Kafka"));
builder.Services.Configure<DbOptions>(builder.Configuration.GetSection("Database"));
builder.Services.AddDbContext<PaymentsDbContext>(opt => opt.UseNpgsql(builder.Configuration.GetConnectionString("Db")));
builder.Services.AddScoped<AccountsAppService>();
builder.Services.AddScoped<PaymentProcessingService>();
builder.Services.AddSingleton<IJsonMessageSerializer, JsonMessageSerializer>();
builder.Services.AddHostedService<OrderPaymentRequestedConsumerHostedService>();
builder.Services.AddSingleton<IKafkaProducerFactory, KafkaProducerFactory>();
builder.Services.AddHostedService<OutboxPublisherHostedService>();

WebApplication app = builder.Build();

using (IServiceScope scope = app.Services.CreateScope())
{
    PaymentsDbContext db = scope.ServiceProvider.GetRequiredService<PaymentsDbContext>();
    db.Database.Migrate();
}

//Swagger
app.UseSwagger(c =>
{
    c.PreSerializeFilters.Add((swagger, httpReq) =>
    {
        swagger.Servers = new List<Microsoft.OpenApi.Models.OpenApiServer>
        {
            new() { Url = "/payments" }
        };
    });
});
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/payments/swagger/v1/swagger.json", "Payments Service API");
});

app.MapControllers();
app.MapGet("/health", () => Results.Ok("ok"));

app.Run();