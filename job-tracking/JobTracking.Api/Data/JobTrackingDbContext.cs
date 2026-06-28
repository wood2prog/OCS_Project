using Microsoft.EntityFrameworkCore;
using JobTracking.Api.Models;

namespace JobTracking.Api.Data;

public class JobTrackingDbContext : DbContext
{
    public JobTrackingDbContext(DbContextOptions<JobTrackingDbContext> options) : base(options) { }

    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Job> Jobs => Set<Job>();
    public DbSet<Milestone> Milestones => Set<Milestone>();
    public DbSet<ChangeOrder> ChangeOrders => Set<ChangeOrder>();
    public DbSet<Document> Documents => Set<Document>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Customer>(e =>
        {
            e.HasKey(c => c.Id);
            e.Property(c => c.Name).IsRequired();
        });

        modelBuilder.Entity<Job>(e =>
        {
            e.HasKey(j => j.Id);
            e.Property(j => j.JobNumber).ValueGeneratedOnAdd();
            e.HasOne(j => j.Customer).WithMany().HasForeignKey(j => j.CustomerId);
            e.Property(j => j.JobName).IsRequired();
            e.Property(j => j.CreatedAt).HasDefaultValueSql("datetime('now')");
        });

        modelBuilder.Entity<Milestone>(e =>
        {
            e.HasKey(m => m.Id);
            e.HasOne(m => m.Job).WithMany(j => j.Milestones).HasForeignKey(m => m.JobId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(m => m.ChangeOrder).WithMany(co => co.Milestones).HasForeignKey(m => m.ChangeOrderId).OnDelete(DeleteBehavior.NoAction);
            e.Property(m => m.Label).IsRequired();
        });

        modelBuilder.Entity<ChangeOrder>(e =>
        {
            e.HasKey(co => co.Id);
            e.HasOne(co => co.Job).WithMany(j => j.ChangeOrders).HasForeignKey(co => co.JobId).OnDelete(DeleteBehavior.Cascade);
            e.Property(co => co.CreatedAt).HasDefaultValueSql("datetime('now')");
        });

        modelBuilder.Entity<Document>(e =>
        {
            e.HasKey(d => d.Id);
            e.HasOne(d => d.Job).WithMany(j => j.Documents).HasForeignKey(d => d.JobId).OnDelete(DeleteBehavior.Cascade);
            e.Property(d => d.Bucket).IsRequired();
            e.Property(d => d.UploadedAt).HasDefaultValueSql("datetime('now')");
        });
    }
}
