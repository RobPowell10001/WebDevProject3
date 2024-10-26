using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Entities;

namespace WebDevProject3.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<Entities.Movie> Movie { get; set; } = default!;
        public DbSet<Entities.Actor> Actor { get; set; } = default!;
        public DbSet<Entities.Role> Role { get; set; } = default!;
    }
}
