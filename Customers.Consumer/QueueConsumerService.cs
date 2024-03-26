
using Amazon.SQS;
using Amazon.SQS.Model;
using Customers.Consumer.MessageHandlers;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Customers.Consumer
{
    public class QueueConsumerService : BackgroundService
    {

        private readonly IAmazonSQS _amazonSQS;
        private readonly IOptions<QueueSettings> _queueSettings;

        public QueueConsumerService(IAmazonSQS amazonSQS, IOptions<QueueSettings> queueSettings)
        {
            _amazonSQS = amazonSQS;
            _queueSettings = queueSettings;
        }

        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var queueUrlResponse = await this._amazonSQS.GetQueueUrlAsync("customer", stoppingToken);

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

                    switch (messageType)
                    {
                        case nameof(CustomerCreated):
                            var customerCreatedObject = JsonSerializer.Deserialize<CustomerCreated>(message.Body);
                            break;

                        case nameof(CustomerUpdated):
                            var customerUpdatedObject = JsonSerializer.Deserialize<CustomerUpdated>(message.Body);
                            break;

                        case nameof(CustomerDeleted):
                            var customerDeletedObject = JsonSerializer.Deserialize<CustomerDeleted>(message.Body);
                            break;

                    }

                    await this._amazonSQS.DeleteMessageAsync(queueUrlResponse.QueueUrl, message.MessageId);
                }

                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
