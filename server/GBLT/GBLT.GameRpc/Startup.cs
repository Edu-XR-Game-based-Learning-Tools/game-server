using Core.Configuration;
using Core.Service;
using Grpc.Net.Client;
using Infrastructure;
using MagicOnion.Server;
using MessagePipe;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RedLockNet;
using RedLockNet.SERedis;
using RpcService.Authentication;
using RpcService.Configuration;
using System.Text;

namespace RpcService
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<AppDbContext>(c =>
            {
                c.UseNpgsql(Configuration.GetConnectionString("DefaultConnection"), b => b.MigrationsAssembly("GBLT.Infrastructure"));
                c.EnableSensitiveDataLogging(true);
            });
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = RedisConfigureServices.GetRedisConnectionString(Configuration);
            });
            services.AddSingleton<IDistributedLockFactory, RedLockFactory>(
                x => RedisConfigureServices.GetRedloadEndpoints(Configuration));

            services.AddGrpc();
            services.AddMagicOnion(options =>
            {
                options.GlobalFilters.Add<VerifySessionFilter>();
            });

            AddJwtAuthentication(services);

            services.AddAuthorization();
            services.AddMessagePipe();

            services.AddCoreServices();

            services.AddControllersWithViews();
            services.AddInjectServices();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapMagicOnionService();
            });

            AddSwaggerOnDevOnly(app);
        }

        private void AddSwaggerOnDevOnly(IApplicationBuilder app)
        {
            string environment = Configuration.GetValue<string>("Environment");
            bool isProd = environment.ToLower() == "production";
            if (!isProd)
                app.UseEndpoints(endpoints =>
                {
                    string rpcAddress = Configuration.GetValue<string>("Kestrel:Endpoints:Https:Url");
                    endpoints.MapMagicOnionHttpGateway("_", app.ApplicationServices.GetService<MagicOnionServiceDefinition>().MethodHandlers, GrpcChannel.ForAddress(rpcAddress));
                    endpoints.MapMagicOnionSwagger("swagger", app.ApplicationServices.GetService<MagicOnionServiceDefinition>().MethodHandlers, "/_/");
                });
        }

        private void AddJwtAuthentication(IServiceCollection services)
        {
            var authSettings = Configuration.GetSection(nameof(AuthSettings));
            services.Configure<AuthSettings>(authSettings);
            var signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(authSettings[nameof(AuthSettings.Secret)]));

            // jwt wire up
            var jwtAppSettingOptions = Configuration.GetSection(nameof(JwtIssuerOptions));

            // Configure JwtIssuerOptions
            services.Configure<JwtIssuerOptions>(options =>
            {
                options.Issuer = jwtAppSettingOptions[nameof(JwtIssuerOptions.Issuer)];
                options.Audience = jwtAppSettingOptions[nameof(JwtIssuerOptions.Audience)];
                options.SigningCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
            });

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwtAppSettingOptions[nameof(JwtIssuerOptions.Issuer)],

                ValidateAudience = true,
                ValidAudience = jwtAppSettingOptions[nameof(JwtIssuerOptions.Audience)],

                ValidateIssuerSigningKey = true,
                IssuerSigningKey = signingKey,

                RequireExpirationTime = false,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(configureOptions =>
            {
                configureOptions.ClaimsIssuer = jwtAppSettingOptions[nameof(JwtIssuerOptions.Issuer)];
                configureOptions.TokenValidationParameters = tokenValidationParameters;
                configureOptions.SaveToken = true;

                configureOptions.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                        {
                            context.Response.Headers.Add("Token-Expired", "true");
                        }
                        return Task.CompletedTask;
                    }
                };
            });
        }
    }
}