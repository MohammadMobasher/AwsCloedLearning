using Amazon.SQS;
using Amazon.SQS.Model;

var cts = new CancellationTokenSource();
var sqsClient = new AmazonSQSClient();

var queueUrlResponse = await sqsClient.GetQueueUrlAsync("customer");

var receiveMessageRequest = new ReceiveMessageRequest 
{ 
    QueueUrl = queueUrlResponse.QueueUrl 
};

while (!cts.IsCancellationRequested)
{
    var response = await sqsClient.ReceiveMessageAsync(receiveMessageRequest, cts.Token);

    foreach (var message in response.Messages)
    {
        Console.WriteLine("Message Id : " + message.MessageId);
        Console.WriteLine("Body Body : " + message.Body);

         await sqsClient.DeleteMessageAsync(queueUrlResponse.QueueUrl, message.ReceiptHandle, cts.Token);
    }

    await Task.Delay(3000);
}

cts.Dispose();