using InterneTaakAfhandeling.Web.Server.Config;
using InterneTaakAfhandeling.Web.Server.Authentication;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.EnvironmentName == "Docker")
{
    builder.Configuration.AddUserSecrets<Program>();
}

// Add services to the container.
builder.Services.RegisterServices(builder.Configuration);

var app = builder.Build();
app.UseExceptionHandler();
app.UseDefaultFiles();
app.UseStaticFiles();

if (app.Environment.IsDevelopment() || app.Environment.EnvironmentName == "Docker")
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Add authentication middleware
app.UseAuthentication();
app.UseAuthorization();

app.UseItaSecurityHeaders();
app.MapControllers();
app.MapITAAuthEndpoints();
app.MapHealthChecks("/healthz").AllowAnonymous();
app.MapFallbackToFile("/index.html");

app.Run();