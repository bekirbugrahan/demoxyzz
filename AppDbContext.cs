using Microsoft.EntityFrameworkCore;
using AzureSqlWebApiSample.Models;

namespace AzureSqlWebApiSample.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<TodoItem> Todos => Set<TodoItem>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TodoItem>(e =>
            {
                e.ToTable("Todos");
                e.HasKey(x => x.Id);
                e.Property(x => x.Title).HasMaxLength(256).IsRequired();
                e.Property(x => x.CreatedUtc).HasDefaultValueSql("SYSUTCDATETIME()");
            });
        }
    }
}
