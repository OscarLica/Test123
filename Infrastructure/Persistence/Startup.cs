using Infrastructure.Persistence.Context;
using Infrastructure.Persistence.Initialization;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Persistence
{
    internal static class Startup
    {
        internal static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration config)
        {
            return services
                .AddDbContext<MasterDbContext>(options => options.UseSqlServer(config.GetConnectionString("MasterBaseConnection")))
                .AddDbContext<ProductDbContext>(options => options.UseSqlServer(config.GetConnectionString("BaseConnection")))
                .AddTransient<IDatabaseInitializer, DatabaseInitializer>()
                .AddTransient<ApplicationDbInitializer>()
                .AddTransient<ApplicationDbSeeder>();
        }
    }
}
