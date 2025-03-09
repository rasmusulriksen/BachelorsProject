var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// builder.Services.AddEndpointsApiExplorer();
// builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IEmailSenderService, EmailSenderService>();
builder.Services.AddHttpClient();
builder.Services.AddHostedService<EmailSenderPollingService>();

var app = builder.Build();

// if (app.Environment.IsDevelopment())
// {
//     app.UseSwagger();
//     app.UseSwaggerUI();
// }

app.MapControllers();

app.Run();