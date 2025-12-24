using Microsoft.OpenApi.Models;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.WithOrigins("http://localhost:8083")
        .AllowAnyHeader()
        .AllowAnyMethod()
));

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "HSE Shop Gateway API", Version = "v1" });
});

WebApplication app = builder.Build();

//просто health
app.MapGet("/", () => Results.Ok("GatewayService is alive"));

//Swagger UI на Gateway, но с 3 вкладками
//gateway + orders + payments
app.UseSwagger(c =>
{
    c.RouteTemplate = "swagger/{documentName}/swagger.json";
});

app.UseSwaggerUI(c =>
{
    c.RoutePrefix = "swagger";
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Gateway");
    c.SwaggerEndpoint("/orders/swagger/v1/swagger.json", "OrdersService");
    c.SwaggerEndpoint("/payments/swagger/v1/swagger.json", "PaymentsService");
});

app.UseRouting();
app.UseCors();
app.MapReverseProxy().RequireCors();

app.Run();