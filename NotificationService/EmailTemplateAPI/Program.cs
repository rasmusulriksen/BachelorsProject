var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register the repository and service with proper dependency injection
builder.Services.AddSingleton<IEmailTemplateRepository, EmailTemplateRepository>();
builder.Services.AddSingleton<IEmailTemplateService, EmailTemplateService>();

// Configure HTTP client
builder.Services.AddHttpClient("MessageQueueClient", client => {
    // Configure any default client settings here if needed
});

// Register the background service
builder.Services.AddHostedService<EmailTemplateBackgroundService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.UseAuthorization();

app.MapControllers();

app.Run();