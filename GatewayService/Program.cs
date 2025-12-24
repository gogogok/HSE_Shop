using Microsoft.OpenApi.Models;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "HSE Shop Gateway API", Version = "v1" });
});

WebApplication app = builder.Build();

// просто health
app.MapGet("/", () => Results.Ok("GatewayService is alive"));

// ВАЖНО: Swagger UI на Gateway, но с 3 “вкладками”:
// gateway + orders + payments
app.UseSwagger(c =>
{
    // gateway own spec (будет минимальный)
    c.RouteTemplate = "swagger/{documentName}/swagger.json";
});

app.UseSwaggerUI(c =>
{
    c.RoutePrefix = "swagger";
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Gateway");
    c.SwaggerEndpoint("/orders/swagger/v1/swagger.json", "OrdersService");
    c.SwaggerEndpoint("/payments/swagger/v1/swagger.json", "PaymentsService");
});


// reverse proxy
app.MapReverseProxy();

app.Run();