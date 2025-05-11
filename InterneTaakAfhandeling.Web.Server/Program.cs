using InterneTaakAfhandeling.Web.Server.Config; 
using InterneTaakAfhandeling.Web.Server.Authentication;
using InterneTaakAfhandeling.Web.Server.Middleware;

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

app.UseHttpsRedirection();

// Add authentication middleware
app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<AuthorizationGuardMiddleware>();

app.UseItaSecurityHeaders();
app.MapControllers();
app.MapITAAuthEndpoints();
app.MapFallbackToFile("/index.html"); 

app.Run();