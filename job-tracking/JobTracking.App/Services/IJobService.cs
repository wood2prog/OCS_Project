using JobTracking.App.Models;

namespace JobTracking.App.Services;

public interface IJobService
{
    Task<List<Job>> GetJobsAsync();
    Task<Job> UpdateMilestoneAsync(int jobId, int milestoneId, bool complete);
    Task<Job> CreateJobAsync(CreateJobRequest dto);
    Task<Job> UpdateJobAsync(int jobId, UpdateJobRequest dto);
    Task DeleteJobAsync(int jobId);
    Task<List<Customer>> GetCustomersAsync();
}
