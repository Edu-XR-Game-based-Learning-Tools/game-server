using Core.Service;
using Microsoft.Extensions.DependencyInjection;

namespace Core.Configuration
{
    public static class CoreDIServices
    {
        public static IServiceCollection AddInjectServices(this IServiceCollection services)
        {
            // Data service
            services.AddSingleton<IRedisDataService, RedisDataService>();
            services.AddSingleton<IDefinitionDataService, DefinitionDataService>();
            services.AddScoped<IMetaDataService, MetaDataService>();
            services.AddScoped<IUserDataService, UserDataService>();

            // Internal data handle service
            services.AddHttpClient<IHttpService, HttpService>();
            services.AddScoped<IMetaService, MetaService>();
            services.AddScoped<IAuthService, PasswordService>();

            // Jwt
            services.AddSingleton<IJwtFactory, JwtFactory>();
            services.AddSingleton<IJwtTokenHandler, JwtTokenHandler>();
            services.AddSingleton<IJwtTokenValidator, JwtTokenValidator>();

            return services;
        }
    }
}