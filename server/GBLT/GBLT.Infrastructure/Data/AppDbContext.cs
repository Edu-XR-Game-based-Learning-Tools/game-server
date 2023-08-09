using Core.Entity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure
{
    public class AppDbContext : DbContext
    {
        public DbSet<TUser> User { get; set; }
        public DbSet<TMeta> Meta { get; set; }
        public DbSet<TQuiz> Quiz { get; set; }
        public DbSet<TQuizCollection> QuizCollection { get; set; }

        public AppDbContext(
            DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<TUser>()
                .HasMany(u => u.QuizCollections)
                .WithOne(h => h.Owner)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<TQuizCollection>()
                .HasMany(u => u.Quizzes)
                .WithOne(h => h.Collection)
                .OnDelete(DeleteBehavior.Cascade);
        }

        public override int SaveChanges()
        {
            OnBeforeSaving();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            OnBeforeSaving();
            return base.SaveChangesAsync(cancellationToken);
        }

        private void OnBeforeSaving()
        {
            var entities = ChangeTracker.Entries()
                .Where(r => r.Entity is BaseEntity && (r.State == EntityState.Added || r.State == EntityState.Modified));

            foreach (var entry in entities)
            {
                switch (entry.State)
                {
                    case EntityState.Modified:
                        entry.Property("CreatedAt").IsModified = false;
                        ((BaseEntity)entry.Entity).UpdatedAt = DateTime.UtcNow;
                        break;

                    case EntityState.Added:
                        ((BaseEntity)entry.Entity).CreatedAt = DateTime.UtcNow;
                        ((BaseEntity)entry.Entity).UpdatedAt = DateTime.UtcNow;
                        break;
                }
            }
        }
    }
}