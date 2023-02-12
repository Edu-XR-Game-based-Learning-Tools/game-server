using Core.Entity;
using Core.Service;
using Infrastructure;
using RpcService.Authentication;
using RpcService.Hub;
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

            services.AddSingleton<JwtTokenService>();

            // Data service
            services.AddSingleton<IRedisDataService, RedisDataService>();
            services.AddSingleton<IDefinitionDataService, DefinitionDataService>();
            services.AddScoped<IMetaDataService, MetaDataService>();
            services.AddScoped<IUserDataService, UserDataService>();
            services.AddScoped<IUserAccountDataService, UserAccountDataService>();

            // Internal data handle service
            services.AddSingleton<VerifySessionFilter>();
            services.AddHttpClient<IHttpService, HttpService>();
            services.AddScoped<IMetaService, MetaService>();

            // Grpc service
            services.AddScoped<IAuthServices, AuthenService>();
            services.AddScoped<IGenericServices, GenericService>();

            return services;
        }

        public static IServiceCollection AddDelegateService(this IServiceCollection services)
        {
            services.AddScoped<GoogleService>();
            services.AddScoped<FirebaseService>();
            services.AddScoped<PasswordService>();
            services.AddScoped<LoginServiceResolver>(serviceProvider => accountType =>
            {
                return accountType switch
                {
                    AccountType.PASSWORD => serviceProvider.GetService<PasswordService>(),
                    AccountType.GOOGLE => serviceProvider.GetService<GoogleService>(),
                    AccountType.FACEBOOK or AccountType.FIREBASE => serviceProvider.GetService<FirebaseService>(),
                    _ => null,
                };
            });

            return services;
        }
    }
}