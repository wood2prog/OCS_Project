using JobTracking.App.Models;

namespace JobTracking.App.Services;

public interface IJobService
{
    Task<List<Job>> GetJobsAsync();
}
