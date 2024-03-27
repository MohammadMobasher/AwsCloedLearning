using Customers.Consumer.Message;
using MediatR;

namespace Customers.Consumer.Messages
{
    public class CreatedCustomerHandler : IRequestHandler<CustomerCreated>
    {

        private readonly ILogger<CreatedCustomerHandler> _logger;

        public CreatedCustomerHandler(ILogger<CreatedCustomerHandler> logger)
        {
            _logger = logger;
        }

        public Task Handle(CustomerCreated request, CancellationToken cancellationToken)
        {
            _logger.LogInformation(request.FullName);

            return Unit.Task;
        }
    }
}
