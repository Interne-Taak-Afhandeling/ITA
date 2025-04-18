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
app.UseAuthentication();
app.UseAuthorization();

app.UseItaSecurityHeaders();
app.MapControllers();
app.MapITAAuthEndpoints();
app.MapFallbackToFile("/index.html").AllowAnonymous();

app.Run();