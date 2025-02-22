using Dapr;
using Dapr.Client;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddDapr();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add your services
builder.Services.AddScoped<IEmailTemplateService, EmailTemplateService>();
builder.Services.AddScoped<IEmailTemplateRepository, EmailTemplateRepository>();

var app = builder.Build();

// Enable Swagger regardless of environment
app.UseSwagger();
app.UseSwaggerUI();

app.UseRouting();
app.UseAuthorization();

app.MapControllers();

app.Run("http://+:80"); // Explicitly set port 80