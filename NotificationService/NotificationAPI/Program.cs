var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddControllers();

builder.Services.AddHttpClient();
builder.Services.AddSingleton<INotificationService, NotificationService>();
builder.Services.AddHostedService<NotificationPollingService>();

builder.Configuration.AddJsonFile("Config/notification-preferences.json", optional: false);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.Run();