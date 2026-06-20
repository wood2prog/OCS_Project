using Microsoft.AspNetCore.Mvc;
using JobTracking.Api.Data;
using JobTracking.Api.Models;

namespace JobTracking.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class JobsController : ControllerBase
{
    private readonly IJobRepository _jobRepo;
    private readonly ICustomerRepository _customerRepo;

    public JobsController(IJobRepository jobRepo, ICustomerRepository customerRepo)
    {
        _jobRepo = jobRepo;
        _customerRepo = customerRepo;
    }

    [HttpGet]
    public async Task<ActionResult<List<Job>>> GetAllActive()
    {
        return await _jobRepo.GetAllActiveAsync();
    }

    [HttpGet("archive")]
    public async Task<ActionResult<List<Job>>> GetAll()
    {
        return await _jobRepo.GetAllAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Job>> GetById(int id)
    {
        var job = await _jobRepo.GetByIdAsync(id);
        if (job is null)
            return NotFound();
        return job;
    }

    [HttpPost]
    public async Task<ActionResult<Job>> Create([FromBody] CreateJobDto dto)
    {
        var customer = await _customerRepo.GetByIdAsync(dto.CustomerId);
        if (customer is null)
            return BadRequest("Customer not found");

        var job = new Job
        {
            CustomerId = dto.CustomerId,
            JobName = dto.JobName,
            LeadDate = dto.LeadDate,
            StartDate = dto.StartDate,
            DeliveryDate = dto.DeliveryDate,
            QuoteAmount = dto.QuoteAmount,
        };

        var created = await _jobRepo.CreateAsync(job);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPatch("{id}")]
    public async Task<ActionResult<Job>> Update(int id, [FromBody] JobUpdateDto dto)
    {
        var job = await _jobRepo.GetByIdAsync(id);
        if (job is null)
            return NotFound();

        if (dto.JobName is not null) job.JobName = dto.JobName;
        if (dto.LeadDate is not null) job.LeadDate = dto.LeadDate;
        if (dto.StartDate is not null) job.StartDate = dto.StartDate;
        if (dto.DeliveryDate is not null) job.DeliveryDate = dto.DeliveryDate;
        if (dto.QuoteAmount is not null) job.QuoteAmount = dto.QuoteAmount;

        await _jobRepo.UpdateAsync(job);
        return job;
    }

    [HttpPost("{id}/documents")]
    public async Task<ActionResult<Document>> AddDocument(int id, [FromBody] CreateDocumentDto dto)
    {
        var job = await _jobRepo.GetByIdAsync(id);
        if (job is null)
            return NotFound();

        var doc = await _jobRepo.AddDocumentAsync(id, dto.Bucket, dto.FileName, dto.StoragePath, dto.Notes);
        return CreatedAtAction(nameof(GetById), new { id }, doc);
    }

    [HttpGet("{id}/documents")]
    public async Task<ActionResult<List<Document>>> GetDocuments(int id)
    {
        var job = await _jobRepo.GetByIdAsync(id);
        if (job is null)
            return NotFound();

        return await _jobRepo.GetDocumentsAsync(id);
    }

    [HttpPost("{id}/changeorders")]
    public async Task<ActionResult<ChangeOrder>> CreateChangeOrder(int id, [FromBody] CreateChangeOrderDto dto)
    {
        var job = await _jobRepo.GetByIdAsync(id);
        if (job is null)
            return NotFound();

        var co = await _jobRepo.CreateChangeOrderAsync(id, dto.Description, dto.Amount);
        return CreatedAtAction(nameof(GetById), new { id }, co);
    }

    [HttpPatch("{id}/milestones/{milestoneId}")]
    public async Task<ActionResult<Job>> ToggleMilestone(int id, int milestoneId, [FromBody] MilestoneToggleDto dto)
    {
        var job = await _jobRepo.GetByIdAsync(id);
        if (job is null)
            return NotFound();

        var milestone = job.Milestones.FirstOrDefault(m => m.Id == milestoneId);
        if (milestone is null)
            return NotFound("Milestone not found");

        milestone.IsComplete = dto.IsComplete;
        milestone.CompletedBy = dto.CompletedBy;
        milestone.CompletedDate = dto.IsComplete ? DateTime.UtcNow : null;

        await _jobRepo.UpdateAsync(job);
        return job;
    }
}

public record JobUpdateDto
{
    public string? JobName { get; init; }
    public DateTime? LeadDate { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? DeliveryDate { get; init; }
    public decimal? QuoteAmount { get; init; }
}

public record MilestoneToggleDto
{
    public bool IsComplete { get; init; }
    public string? CompletedBy { get; init; }
}

public record CreateChangeOrderDto
{
    public string Description { get; init; } = string.Empty;
    public decimal? Amount { get; init; }
}

public record CreateDocumentDto
{
    public string Bucket { get; init; } = string.Empty;
    public string FileName { get; init; } = string.Empty;
    public string StoragePath { get; init; } = string.Empty;
    public string? Notes { get; init; }
}
