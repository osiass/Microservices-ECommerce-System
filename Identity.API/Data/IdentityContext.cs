using Identity.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace Identity.API.Data
{
    public class IdentityContext: DbContext
    {
        public IdentityContext(DbContextOptions<IdentityContext> options) : base(options) { }
        public DbSet<AppUser> AppUsers { get; set; }
    }
}
