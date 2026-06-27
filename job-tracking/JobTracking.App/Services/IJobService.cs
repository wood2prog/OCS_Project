using JobTracking.App.Models;

namespace JobTracking.App.Services;

public interface IJobService
{
    Task<List<Job>> GetJobsAsync();
    Task<Job> ToggleMilestoneAsync(int jobId, int milestoneId, bool isComplete);
    Task<Job> CreateJobAsync(CreateJobRequest dto);
    Task<List<Customer>> GetCustomersAsync();
}
