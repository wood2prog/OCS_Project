using Microsoft.EntityFrameworkCore;
using JobTracking.Api.Models;

namespace JobTracking.Api.Data;

public class JobRepository : IJobRepository
{
    private readonly JobTrackingDbContext _db;

    public JobRepository(JobTrackingDbContext db)
    {
        _db = db;
    }

    public async Task<Job> CreateAsync(Job job)
    {
        var maxNumber = await _db.Jobs.MaxAsync(j => (int?)j.JobNumber) ?? 999;
        job.JobNumber = maxNumber + 1;
        job.CreatedAt = DateTime.UtcNow;

        foreach (var (order, label) in DefaultMilestones.All)
        {
            job.Milestones.Add(new Milestone
            {
                Order = order,
                Label = label,
                CompletedAt = null,
            });
        }

        _db.Jobs.Add(job);
        await _db.SaveChangesAsync();
        return job;
    }

    public async Task<Job?> GetByIdAsync(int id)
    {
        return await _db.Jobs
            .Include(j => j.Customer)
            .Include(j => j.Milestones.Where(m => m.ChangeOrderId == null).OrderBy(m => m.Order))
            .Include(j => j.ChangeOrders)
                .ThenInclude(co => co.Milestones.OrderBy(m => m.Order))
            .FirstOrDefaultAsync(j => j.Id == id);
    }

    public async Task<List<Job>> GetAllActiveAsync()
    {
        var allJobs = await _db.Jobs
            .Include(j => j.Customer)
            .Include(j => j.Milestones.Where(m => m.ChangeOrderId == null))
            .ToListAsync();

        var sevenDaysAgo = DateTime.UtcNow.AddDays(-7);
        return allJobs
            .Where(j =>
            {
                var highest = j.Milestones
                    .Where(m => m.CompletedAt != null && m.ChangeOrderId == null)
                    .MaxBy(m => m.Order);
                if (highest?.Order == 12 && highest.CompletedAt <= sevenDaysAgo)
                    return false;
                return true;
            })
            .OrderByDescending(j => j.CreatedAt)
            .ToList();
    }

    public async Task<List<Job>> GetAllAsync()
    {
        return await _db.Jobs
            .Include(j => j.Customer)
            .Include(j => j.Milestones.Where(m => m.ChangeOrderId == null))
            .OrderByDescending(j => j.CreatedAt)
            .ToListAsync();
    }

    public async Task<ChangeOrder> CreateChangeOrderAsync(int jobId, string description, decimal? amount)
    {
        var job = await _db.Jobs.FirstAsync(j => j.Id == jobId);

        var co = new ChangeOrder
        {
            JobId = jobId,
            Description = description,
            Amount = amount,
        };
        _db.ChangeOrders.Add(co);
        await _db.SaveChangesAsync();

        foreach (var (order, label) in DefaultMilestones.All)
        {
            _db.Milestones.Add(new Milestone
            {
                JobId = jobId,
                ChangeOrderId = co.Id,
                Order = order,
                Label = label,
                CompletedAt = null,
            });
        }

        job.ChangeOrderTotal = (job.ChangeOrderTotal ?? 0) + (amount ?? 0);
        await _db.SaveChangesAsync();

        return (await _db.ChangeOrders
            .Include(c => c.Milestones.OrderBy(m => m.Order))
            .FirstAsync(c => c.Id == co.Id))!;
    }

    public async Task<Document> AddDocumentAsync(int jobId, string bucket, string fileName, string storagePath, string? notes)
    {
        var doc = new Document
        {
            JobId = jobId,
            Bucket = bucket,
            FileName = fileName,
            StoragePath = storagePath,
            Notes = notes,
        };
        _db.Documents.Add(doc);
        await _db.SaveChangesAsync();
        return doc;
    }

    public async Task<List<Document>> GetDocumentsAsync(int jobId)
    {
        return await _db.Documents
            .Where(d => d.JobId == jobId)
            .OrderByDescending(d => d.UploadedAt)
            .ToListAsync();
    }

    public async Task UpdateAsync(Job job)
    {
        _db.Jobs.Update(job);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var job = await _db.Jobs.FindAsync(id);
        if (job is not null)
        {
            _db.Jobs.Remove(job);
            await _db.SaveChangesAsync();
        }
    }
}
