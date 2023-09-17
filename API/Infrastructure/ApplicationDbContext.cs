using HallOfFame.Model;
using Microsoft.EntityFrameworkCore;

namespace HallOfFame.Infrastructure
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext()
        { }
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        { }

        public virtual DbSet<Person> Persons => Set<Person>();
        public virtual DbSet<Skill> Skills => Set<Skill>();
    }
}