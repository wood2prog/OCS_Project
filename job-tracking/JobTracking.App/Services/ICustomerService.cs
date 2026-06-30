using JobTracking.App.Models;

namespace JobTracking.App.Services;

public interface ICustomerService
{
    Task<List<Customer>> GetCustomersAsync();
    Task<Customer> CreateCustomerAsync(CreateCustomerRequest dto);
    Task<Customer> UpdateCustomerAsync(int id, UpdateCustomerRequest dto);
    Task DeleteCustomerAsync(int id);
}
