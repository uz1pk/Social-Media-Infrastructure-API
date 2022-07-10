using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TweetAPI.Domain;

namespace TweetAPI.Data
{
    public class DBContext : IdentityDbContext
    {
        public DBContext(DbContextOptions<DBContext> options)
            : base(options)
        {

        }

        public DbSet<Post> Posts { get; set; }
    }
}