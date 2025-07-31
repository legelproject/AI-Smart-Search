using AI_Smart_Search.Model;
using Microsoft.EntityFrameworkCore;

namespace AI_Smart_Search.Dbcontext
{
    public class AIDbContext:DbContext
    {
        public AIDbContext(DbContextOptions<AIDbContext> options) : base(options)
        {
        }
        public DbSet<QApair> QApairs { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
    }
}
