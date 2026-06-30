using Microsoft.EntityFrameworkCore;
using JobTracking.Api.Models;

namespace JobTracking.Api.Data;

public class CustomerRepository : ICustomerRepository
{
    private readonly JobTrackingDbContext _db;

    public CustomerRepository(JobTrackingDbContext db)
    {
        _db = db;
    }

    public async Task<List<Customer>> GetAllAsync()
    {
        return await _db.Customers.OrderBy(c => c.Name).ToListAsync();
    }

    public async Task<Customer?> GetByIdAsync(int id)
    {
        return await _db.Customers.FindAsync(id);
    }

    public async Task<Customer> CreateAsync(Customer customer)
    {
        _db.Customers.Add(customer);
        await _db.SaveChangesAsync();
        return customer;
    }

    public async Task<Customer> UpdateAsync(Customer customer)
    {
        _db.Customers.Update(customer);
        await _db.SaveChangesAsync();
        return customer;
    }

    public async Task DeleteAsync(int id)
    {
        var customer = await _db.Customers.FindAsync(id);
        if (customer is not null)
        {
            _db.Customers.Remove(customer);
            await _db.SaveChangesAsync();
        }
    }

    public async Task<bool> HasJobsAsync(int customerId)
    {
        return await _db.Jobs.AnyAsync(j => j.CustomerId == customerId);
    }

    public async Task<Dictionary<int, int>> GetCustomerJobCountsAsync()
    {
        return await _db.Jobs
            .GroupBy(j => j.CustomerId)
            .Select(g => new { CustomerId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.CustomerId, x => x.Count);
    }
}
