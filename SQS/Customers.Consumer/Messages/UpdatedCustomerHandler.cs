using Customers.Consumer.Message;
using MediatR;

namespace Customers.Consumer.Messages
{
    public class UpdatedCustomerHandler : IRequestHandler<CustomerUpdated>
    {

        private readonly ILogger<UpdatedCustomerHandler> _logger;

        public UpdatedCustomerHandler(ILogger<UpdatedCustomerHandler> logger)
        {
            _logger = logger;
        }

        public Task Handle(CustomerUpdated request, CancellationToken cancellationToken)
        {
            _logger.LogInformation(request.GitHubUsername);
            return Unit.Task;
        }
    }
}
