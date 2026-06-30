using JobTracking.Api.Models;

namespace JobTracking.Api.Data;

public interface ICustomerRepository
{
    Task<List<Customer>> GetAllAsync();
    Task<Customer?> GetByIdAsync(int id);
    Task<Customer> CreateAsync(Customer customer);
    Task<Customer> UpdateAsync(Customer customer);
    Task DeleteAsync(int id);
    Task<bool> HasJobsAsync(int customerId);
    Task<Dictionary<int, int>> GetCustomerJobCountsAsync();
}
