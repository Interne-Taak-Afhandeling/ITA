using InterneTaakAfhandeling.Web.Server.Config; 
using InterneTaakAfhandeling.Web.Server.Authentication;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.RegisterServices(builder.Configuration);

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
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
app.MapFallbackToFile("/index.html");

app.Run();
