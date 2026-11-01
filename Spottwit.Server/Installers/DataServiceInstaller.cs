using Microsoft.EntityFrameworkCore;
using Spottwit.Server.Data;
using Spottwit.Server.Repositories;
using Spottwit.Server.Services;

namespace Spottwit.Server.Installers
{
    public class DataServiceInstaller : IServiceInstaller
    {
        public void Install(IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(connectionString));

            var redisConnectionString = configuration.GetConnectionString("Redis") ?? "localhost:6380";
            services.AddStackExchangeRedisCache(options => options.Configuration = redisConnectionString);

            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped(typeof(IGenericService<>), typeof(GenericService<>));
        }
    }
}