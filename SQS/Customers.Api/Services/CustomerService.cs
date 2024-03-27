using Customers.Api.Contracts.Messages;
using Customers.Api.Domain;
using Customers.Api.Mapping;
using Customers.Api.Messaging;
using Customers.Api.Repositories;
using FluentValidation;
using FluentValidation.Results;

namespace Customers.Api.Services;

public class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IGitHubService _gitHubService;
    private readonly ISqsMessanger _sqsMessanger;

    public CustomerService(ICustomerRepository customerRepository,
        IGitHubService gitHubService,
        ISqsMessanger sqsMessanger)
    {
        _customerRepository = customerRepository;
        _gitHubService = gitHubService;
        _sqsMessanger = sqsMessanger;
    }

    public async Task<bool> CreateAsync(Customer customer)
    {
        var existingUser = await _customerRepository.GetAsync(customer.Id);
        if (existingUser is not null)
        {
            var message = $"A user with id {customer.Id} already exists";
            throw new ValidationException(message, GenerateValidationError(nameof(Customer), message));
        }

        var isValidGitHubUser = await _gitHubService.IsValidGitHubUser(customer.GitHubUsername);
        if (!isValidGitHubUser)
        {
            var message = $"There is no GitHub user with username {customer.GitHubUsername}";
            throw new ValidationException(message, GenerateValidationError(nameof(customer.GitHubUsername), message));
        }

        var customerDto = customer.ToCustomerDto();
        var result = await _customerRepository.CreateAsync(customerDto);
        if (result)
        {
            await this._sqsMessanger.SendMessageAsync(customer.ToCustomerCreatedMassage());
        }
        return result;
    }

    public async Task<Customer?> GetAsync(Guid id)
    {
        var customerDto = await _customerRepository.GetAsync(id);
        return customerDto?.ToCustomer();
    }

    public async Task<IEnumerable<Customer>> GetAllAsync()
    {
        var customerDtos = await _customerRepository.GetAllAsync();
        return customerDtos.Select(x => x.ToCustomer());
    }

    public async Task<bool> UpdateAsync(Customer customer)
    {
        var customerDto = customer.ToCustomerDto();

        var isValidGitHubUser = await _gitHubService.IsValidGitHubUser(customer.GitHubUsername);
        if (!isValidGitHubUser)
        {
            var message = $"There is no GitHub user with username {customer.GitHubUsername}";
            throw new ValidationException(message, GenerateValidationError(nameof(customer.GitHubUsername), message));
        }

        var result = await _customerRepository.UpdateAsync(customerDto);
        if (result)
        {
            await this._sqsMessanger.SendMessageAsync(customer.ToCustomerUpdatedMassage());
        }
        return result;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var result = await _customerRepository.DeleteAsync(id);
        if (result)
        {
            await this._sqsMessanger.SendMessageAsync(new CustomerDeleted { Id = id });
        }
        return result;
    }

    private static ValidationFailure[] GenerateValidationError(string paramName, string message)
    {
        return new[]
        {
            new ValidationFailure(paramName, message)
        };
    }
}
