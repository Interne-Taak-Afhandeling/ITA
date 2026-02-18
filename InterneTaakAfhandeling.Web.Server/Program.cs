using InterneTaakAfhandeling.Web.Server.Config;
using InterneTaakAfhandeling.Web.Server.Authentication;
using InterneTaakAfhandeling.Web.Server.Data;
using Microsoft.EntityFrameworkCore;

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
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();  
}
app.Run(); 
