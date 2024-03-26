using Amazon.SQS;
using Customers.Consumer;
using System.Reflection;


var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<QueueSettings>(builder.Configuration.GetSection("QueueSettings"));
builder.Services.AddSingleton<IAmazonSQS, AmazonSQSClient>();
builder.Services.AddHostedService<QueueConsumerService>();
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

var app = builder.Build();
app.Run();
