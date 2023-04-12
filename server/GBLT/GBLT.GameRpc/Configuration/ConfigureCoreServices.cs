using Core.Configuration;
using Core.Entity;
using Infrastructure;
using RpcService.Authentication;
using RpcService.Service;
using Shared.Network;

namespace RpcService.Configuration
{
    public static class ConfigureCoreServices
    {
        public static IServiceCollection AddCoreServices(this IServiceCollection services)
        {
            services.AddScoped(typeof(IReadRepository<>), typeof(EfRepository<>));
            services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));

            // Grpc service
            services.AddSingleton<VerifySessionFilter>();
            services.AddScoped<IRpcAuthService, AuthService>();
            services.AddScoped<IGenericService, GenericService>();

            services.AddInjectServices();

            return services;
        }
    }
}