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
}
