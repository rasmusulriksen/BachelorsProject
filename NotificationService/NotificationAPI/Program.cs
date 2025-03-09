var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddHttpClient();
builder.Services.AddSingleton<INotificationService, NotificationService>();
builder.Services.AddHostedService<NotificationPollingService>();

builder.Configuration.AddJsonFile("Config/notification-preferences.json", optional: false);

var app = builder.Build();

app.MapControllers();

app.Run();