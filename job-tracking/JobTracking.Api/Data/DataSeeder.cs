using JobTracking.Api.Models;

namespace JobTracking.Api.Data;

public static class DataSeeder
{
    public static async Task SeedAsync(JobTrackingDbContext db)
    {
        if (db.Customers.Any())
            return;

        var thompson = new Customer { Name = "Thompson", Email = "tom@thompson.com", Phone = "555-0101" };
        var riverbend = new Customer { Name = "Riverbend Construction", Email = "info@riverbend.com", Phone = "555-0102" };
        var garcia = new Customer { Name = "Garcia", Email = "carlos@garcia.com", Phone = "555-0103" };
        var maplewood = new Customer { Name = "Maplewood Homes", Email = "build@maplewood.com", Phone = "555-0104" };

        db.Customers.AddRange(thompson, riverbend, garcia, maplewood);
        await db.SaveChangesAsync();

        var jobNumber = 1000;
        var jobs = new[]
        {
            new Job
            {
                CustomerId = thompson.Id,
                JobName = "Thompson Kitchen Remodel",
                JobNumber = jobNumber++,
                Milestones = CreateMilestones(3)
            },
            new Job
            {
                CustomerId = riverbend.Id,
                JobName = "Riverbend Office Buildout",
                JobNumber = jobNumber++,
                Milestones = CreateMilestones(6)
            },
            new Job
            {
                CustomerId = garcia.Id,
                JobName = "Garcia Bathroom Vanity",
                JobNumber = jobNumber++,
                Milestones = CreateMilestones(7)
            },
            new Job
            {
                CustomerId = maplewood.Id,
                JobName = "Maplewood Spec House",
                JobNumber = jobNumber++,
                Milestones = CreateMilestones(0)
            },
        };

        db.Jobs.AddRange(jobs);
        await db.SaveChangesAsync();
    }

    private static List<Milestone> CreateMilestones(int completeUpTo)
    {
        var milestones = new List<Milestone>();
        foreach (var (order, label) in DefaultMilestones.All)
        {
            milestones.Add(new Milestone
            {
                Order = order,
                Label = label,
                CompletedAt = order <= completeUpTo ? DateTime.UtcNow : null,
            });
        }
        return milestones;
    }
}
