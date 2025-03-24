using System;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Saj.WeatherDataProcessor
{
    public class WeatherDataReader
    {
        private readonly ILogger<WeatherDataReader> _logger;

        public WeatherDataReader(ILogger<WeatherDataReader> logger)
        {
            _logger = logger;
        }

        [Function(nameof(WeatherDataReader))]
        public async Task Run(
            [ServiceBusTrigger("weather-data-topic", "read-weather-data", Connection = "WeatherDataTopic")]
            ServiceBusReceivedMessage message,
            ServiceBusMessageActions messageActions)
        {
            _logger.LogInformation("Message ID: {id}", message.MessageId);
            _logger.LogInformation("Message Body: {body}", message.Body);
            _logger.LogInformation("Message Content-Type: {contentType}", message.ContentType);

             // Complete the message
            await messageActions.CompleteMessageAsync(message);
        }
    }
}
