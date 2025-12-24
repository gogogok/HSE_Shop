WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
WebApplication app = builder.Build();

app.UseDefaultFiles(); //ищет index.html
app.UseStaticFiles();

app.MapGet("/health", () => Results.Ok("ok"));

app.Run();