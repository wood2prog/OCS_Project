using Microsoft.AspNetCore.Mvc;
using JobTracking.Api.Data;
using JobTracking.Api.Models;

namespace JobTracking.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly ICustomerRepository _repo;

    public CustomersController(ICustomerRepository repo)
    {
        _repo = repo;
    }

    [HttpGet]
    public async Task<ActionResult<List<Customer>>> GetAll()
    {
        var customers = await _repo.GetAllAsync();
        var jobCounts = await _repo.GetCustomerJobCountsAsync();
        foreach (var c in customers)
            c.JobCount = jobCounts.GetValueOrDefault(c.Id, 0);
        return customers;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Customer>> GetById(int id)
    {
        var customer = await _repo.GetByIdAsync(id);
        if (customer is null)
            return NotFound();
        return customer;
    }

    [HttpPost]
    public async Task<ActionResult<Customer>> Create([FromBody] CreateCustomerDto dto)
    {
        var customer = new Customer
        {
            Name = dto.Name,
            Email = dto.Email,
            Phone = dto.Phone,
            Notes = dto.Notes,
        };
        var created = await _repo.CreateAsync(customer);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPatch("{id}")]
    public async Task<ActionResult<Customer>> Update(int id, [FromBody] UpdateCustomerDto dto)
    {
        var customer = await _repo.GetByIdAsync(id);
        if (customer is null)
            return NotFound();

        if (dto.Name is not null) customer.Name = dto.Name;
        if (dto.Email is not null) customer.Email = dto.Email;
        if (dto.Phone is not null) customer.Phone = dto.Phone;
        if (dto.Notes is not null) customer.Notes = dto.Notes;

        await _repo.UpdateAsync(customer);
        return customer;
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        var customer = await _repo.GetByIdAsync(id);
        if (customer is null)
            return NotFound();

        var jobCounts = await _repo.GetCustomerJobCountsAsync();
        if (jobCounts.TryGetValue(id, out var count))
            return Conflict(new { message = $"Cannot delete: {count} jobs reference this customer." });

        await _repo.DeleteAsync(id);
        return Ok();
    }
}
