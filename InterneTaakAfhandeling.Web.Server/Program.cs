using InterneTaakAfhandeling.Web.Server.Config; 
using InterneTaakAfhandeling.Web.Server.Authentication;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.RegisterServices(builder.Configuration);

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

if (app.Environment.IsDevelopment() || app.Environment.EnvironmentName == "Docker")
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Add authentication middleware
// Temporary disabled: app.UseAuthentication();
app.UseAuthorization();

app.UseItaSecurityHeaders();
app.MapControllers();
// Temporary disabled: app.MapITAAuthEndpoints();
app.MapFallbackToFile("/index.html");

app.Run();