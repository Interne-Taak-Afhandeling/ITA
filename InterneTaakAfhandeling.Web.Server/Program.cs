using InterneTaakAfhandeling.Web.Server.Config;
using InterneTaakAfhandeling.Web.Server.Features;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<ResourcesConfig>();
var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

if (app.Environment.IsDevelopment() || app.Environment.EnvironmentName == "Docker")
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.UseItaSecurityHeaders();
app.MapControllers();
app.MapFallbackToFile("/index.html");

app.Run();