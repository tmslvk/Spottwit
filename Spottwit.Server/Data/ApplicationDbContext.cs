using Microsoft.EntityFrameworkCore;
using Spottwit.Server.Model;
using System.Reflection;

namespace Spottwit.Server.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.RegisterAllEntities(Assembly.GetExecutingAssembly());
        }
    }
}