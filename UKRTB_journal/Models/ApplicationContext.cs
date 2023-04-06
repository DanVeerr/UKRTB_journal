using Microsoft.EntityFrameworkCore;

namespace UKRTB_journal.Models
{
    public class ApplicationContext : DbContext
    {
        public DbSet<FileDescription> Files { get; set; }
        public DbSet<MetodFileDescription> MetodFiles { get; set; }
        public DbSet<Student> Students { get; set; }

        public DbSet<Group> Groups { get; set; }

        public ApplicationContext(DbContextOptions<ApplicationContext> options)
            : base(options)
        {
            Database.EnsureCreated();
        }
    }
}
