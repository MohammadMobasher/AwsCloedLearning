
using Amazon.SQS;
using Amazon.SQS.Model;
using Customers.Consumer.Message;
using MediatR;
using Microsoft.Extensions.Options;
using System.Net.WebSockets;
using System.Text.Json;

namespace Customers.Consumer
{
    public class QueueConsumerService : BackgroundService
    {

        private readonly IAmazonSQS _amazonSQS;
        private readonly IOptions<QueueSettings> _queueSettings;
        private readonly IMediator _mediator;
        private readonly ILogger _logger;

        public QueueConsumerService(IAmazonSQS amazonSQS, IOptions<QueueSettings> queueSettings, IMediator mediator, ILogger logger)
        {
            _amazonSQS = amazonSQS;
            _queueSettings = queueSettings;
            _mediator = mediator;
            _logger = logger;
        }

        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var queueUrlResponse = await this._amazonSQS.GetQueueUrlAsync(_queueSettings.Value.QueueName, stoppingToken);

            var receivedMessageRequest = new ReceiveMessageRequest
            {
                QueueUrl = queueUrlResponse.QueueUrl,
                AttributeNames = new List<string> { "All" },
                MessageAttributeNames = new List<string> { "All" },
                MaxNumberOfMessages = 1,
            };

            while (!stoppingToken.IsCancellationRequested)
            {

                var response = await this._amazonSQS.ReceiveMessageAsync(receivedMessageRequest, stoppingToken);

                foreach (var message in response.Messages)
                {
                    var messageType = message.MessageAttributes["MessageType"].StringValue;
                    var type = Type.GetType(messageType);
                    if (type is null)
                    {
                        _logger.LogWarning($"Unknown message type {messageType}");
                        continue;
                    }
                    var typedMessage = (ISqsMessage)JsonSerializer.Deserialize(message.Body, type!)!;

                    try
                    {
                        await _mediator.Send(typedMessage);
                    }
                    catch (Exception e)
                    {
                        this._logger.LogError(e.ToString());
                        continue;
                    }
                    

                    await this._amazonSQS.DeleteMessageAsync(queueUrlResponse.QueueUrl, message.MessageId);
                }

                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
