using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MyRazorApp.Services;

var builder = WebApplication.CreateBuilder(args);

// Configure services
builder.Services.AddRazorPages();

// Configure SmtpSettings from appsettings.json
builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));

// Register EmailService with dependency injection
builder.Services.AddScoped<IEmailService, EmailService>();

var app = builder.Build();

// Verify SmtpSettings on startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    var smtpSettings = services.GetRequiredService<IOptions<SmtpSettings>>().Value;

    logger.LogInformation($"SMTP Host: {smtpSettings.Host}");
    logger.LogInformation($"SMTP Port: {smtpSettings.Port}");
    logger.LogInformation($"SMTP Username: {smtpSettings.Username}");
    logger.LogInformation($"SMTP UseSSL: {smtpSettings.UseSSL}");
}

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
