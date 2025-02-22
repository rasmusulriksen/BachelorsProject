var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddDapr();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();
builder.Services.AddDaprClient();
builder.Services.AddScoped<INotificationService, NotificationService>();

builder.Configuration.AddJsonFile("Config/notification-preferences.json", optional: false);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCloudEvents();
app.MapControllers();
app.MapSubscribeHandler();

app.Run("http://+:80");