using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Customers.Api.Messaging
{
    public class SnsMessanger : ISnsMessanger
    {

        private readonly IAmazonSimpleNotificationService _snsClient;
        private readonly IOptions<TopicSettings> _topicSettings;
        private string? _tokenArn;

        public SnsMessanger(IAmazonSimpleNotificationService sqsClient, IOptions<TopicSettings> queueSettings)
        {
            _snsClient = sqsClient;
            _topicSettings = queueSettings;
        }
         
        public async Task<PublishResponse> PublishMessageAsync<T>(T message)
        {
            string queueRequestUrl = await GetQueueUrlAsync();

            var sendMessageRequest = new PublishRequest
            {
                TopicArn = queueRequestUrl,
                Message = JsonSerializer.Serialize(message),
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

            return await this._snsClient.PublishAsync(sendMessageRequest);
        }

        private async Task<string> GetQueueUrlAsync()
        {
            if(_tokenArn != null)
            {
                return _tokenArn;
            }

            _tokenArn = (await this._snsClient.FindTopicAsync(this._topicSettings.Value.TopicName)).TopicArn;
            return _tokenArn;
        }
    }
}
