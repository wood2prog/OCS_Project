using JobTracking.Api.Models;

namespace JobTracking.Api.Data;

public interface IJobRepository
{
    Task<Job> CreateAsync(Job job);
    Task<Job?> GetByIdAsync(int id);
    Task<List<Job>> GetAllActiveAsync();
    Task<List<Job>> GetAllAsync();
    Task UpdateAsync(Job job);
    Task<ChangeOrder> CreateChangeOrderAsync(int jobId, string description, decimal? amount);
    Task<Document> AddDocumentAsync(int jobId, string bucket, string fileName, string storagePath, string? notes);
    Task<List<Document>> GetDocumentsAsync(int jobId);
}
