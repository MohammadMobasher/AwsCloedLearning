using Customers.Consumer.Message;
using MediatR;

namespace Customers.Consumer.Messages
{
    public class DeletedCustomerHandler : IRequestHandler<CustomerDeleted>
    {
        private readonly ILogger<CreatedCustomerHandler> _logger;

        public DeletedCustomerHandler(ILogger<CreatedCustomerHandler> logger)
        {
            _logger = logger;
        }

        public Task Handle(CustomerDeleted request, CancellationToken cancellationToken)
        {
            _logger.LogInformation(request.Id.ToString());
            return Unit.Task;
        }
    }
}
