using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using SnsPublisher;
using System.Text.Json;

var customerCreated = new CustomerCreated
{
    Email = "m.mobasher.z@gmail.com",
    FullName = "Mohammad Moabasher",
    DateOfBirth = new DateTime(1993, 1, 1),
    GitHubUserName = "MohammadMobasher",
    Id = Guid.NewGuid(),
};

var snsClient = new AmazonSimpleNotificationServiceClient();
var topicArnResponse = await snsClient.FindTopicAsync("customer");

var publishRequest = new PublishRequest
{
    TargetArn = topicArnResponse.TopicArn,
    Message = JsonSerializer.Serialize(customerCreated),
    MessageAttributes = new Dictionary<string, MessageAttributeValue>
    {
        {
            "MessageType", new MessageAttributeValue
            {
                DataType = "string",
                StringValue = nameof(CustomerCreated),

            }
        }
    },
};

var response = await snsClient.PublishAsync(publishRequest);