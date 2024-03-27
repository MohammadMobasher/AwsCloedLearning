using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Customers.Api.Messaging
{
    public class SqsMessanger : ISqsMessanger
    {

        private readonly IAmazonSQS _sqsClient;
        private readonly IOptions<QueueSettings> _queueSettings;
        private string? _queueUrl;

        public SqsMessanger(IAmazonSQS sqsClient, IOptions<QueueSettings> queueSettings)
        {
            _sqsClient = sqsClient;
            _queueSettings = queueSettings;
        }

        public async Task<SendMessageResponse> SendMessageAsync<T>(T message)
        {
            string queueRequestUrl = await GetQueueUrlAsync();

            var sendMessageRequest = new SendMessageRequest
            {
                QueueUrl = queueRequestUrl,
                MessageBody = JsonSerializer.Serialize(message),
                MessageAttributes = new Dictionary<string, MessageAttributeValue>
                {
                    {
                        "MessageType", new MessageAttributeValue
                        {
                            DataType = "string",
                            StringValue = nameof(T)
                        }
                    }
                },

            };

            return await this._sqsClient.SendMessageAsync(sendMessageRequest);
        }

        private async Task<string> GetQueueUrlAsync()
        {
            if(_queueUrl != null)
            {
                return _queueUrl;
            }

            _queueUrl = (await this._sqsClient.GetQueueUrlAsync(this._queueSettings.Value.QueueName)).QueueUrl;
            return _queueUrl;
        }
    }
}
