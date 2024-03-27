using Amazon.SQS;
using Amazon.SQS.Model;
using SqsPublisher;
using System.Text.Json;

var sqsClient = new AmazonSQSClient();

var customer = new CustomerCreated
{
    Id = Guid.NewGuid(),
    Email = "m.mobasher.z@gmail.com",
    GitHubUserName = "MohammadMobasher",
    FullName = "Mohammad Mobasher",
    DateOfBirth = new DateTime(year: 1993, month: 01, day: 01),
};

var queueUrlResponse = await sqsClient.GetQueueUrlAsync("customer");

var response = await sqsClient.SendMessageAsync(new SendMessageRequest
{
    QueueUrl = queueUrlResponse.QueueUrl,
    MessageBody = JsonSerializer.Serialize(customer),
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
});

