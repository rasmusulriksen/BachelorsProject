using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public class EmailPollingService : BackgroundService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<EmailPollingService> _logger;
    private readonly TimeSpan _pollingInterval = TimeSpan.FromSeconds(10);

    public EmailPollingService(IHttpClientFactory httpClientFactory, ILogger<EmailPollingService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Email polling service started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("MessageQueueClient");
                var response = await client.GetAsync("http://localhost:5204/api/messagequeue/poll", stoppingToken);

                if (response.IsSuccessStatusCode)
                {
                    var emailMessage = await response.Content.ReadAsStringAsync();
                    // Process the email message (deserialize, send email, etc.)
                    
                    _logger.LogInformation("Processed new email: {EmailMessage}", emailMessage);
                }
                else
                {
                    _logger.LogWarning("No new emails or processing failed: {StatusCode}", response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while polling for emails.");
            }

            await Task.Delay(_pollingInterval, stoppingToken);
        }

        _logger.LogInformation("Email polling service is stopping.");
    }
}