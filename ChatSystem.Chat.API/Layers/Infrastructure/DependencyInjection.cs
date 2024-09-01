using ChatSystem.Chat.API.Layers.Application.Infrastructure.Common.Infrastructure;
using ChatSystem.Chat.API.Layers.Infrastructure.Data.Interceptors;
using ChatSystem.Chat.API.Layers.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ChatSystem.Chat.API.Layers.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DatabaseConnection");

            services.AddScoped<AuditableEntityInterceptor>();
            services.AddScoped<DispatchDomainEventsInterceptor>();

            services.AddDbContext<IChatDbContext, ChatDbContext>((sp, options) =>
            {
                options.AddInterceptors
                (
                    sp.GetService<AuditableEntityInterceptor>(),
                    sp.GetService<DispatchDomainEventsInterceptor>()
                );
                options.UseSqlServer(connectionString);
            });

            return services;
        }
    }
}
