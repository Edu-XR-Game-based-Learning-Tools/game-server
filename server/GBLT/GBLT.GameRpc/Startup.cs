using Core.Configuration;
using Core.Entity;
using Core.Service;
using Grpc.Net.Client;
using Infrastructure;
using MagicOnion.Server;
using MessagePipe;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RedLockNet;
using RedLockNet.SERedis;
using RpcService.Authentication;
using RpcService.Configuration;
using Shared.Network;
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
            services.AddDbContext<AppIdentityDbContext>(options =>
            {
                options.UseNpgsql(Configuration.GetConnectionString("DefaultConnection"), b => b.MigrationsAssembly("GBLT.Infrastructure"));
            });
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseNpgsql(Configuration.GetConnectionString("DefaultConnection"), b => b.MigrationsAssembly("GBLT.Infrastructure"));
                options.EnableSensitiveDataLogging(true);
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
            AddIdentityAndUserRule(services);

            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

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

            InitStartup(app);
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
                ValidateIssuer = false,
                //ValidIssuer = jwtAppSettingOptions[nameof(JwtIssuerOptions.Issuer)],

                ValidateAudience = false,
                //ValidAudience = jwtAppSettingOptions[nameof(JwtIssuerOptions.Audience)],

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

        private static void AddIdentityAndUserRule(IServiceCollection services)
        {
            // add identity
            var identityBuilder = services.AddIdentityCore<TIdentityUser>(o =>
            {
                // configure identity options
                o.Password.RequireDigit = false;
                o.Password.RequireLowercase = false;
                o.Password.RequireUppercase = false;
                o.Password.RequireNonAlphanumeric = false;
                o.Password.RequiredLength = 6;
            });

            identityBuilder = new IdentityBuilder(identityBuilder.UserType, typeof(IdentityRole), identityBuilder.Services);
            identityBuilder.AddEntityFrameworkStores<AppIdentityDbContext>()
                .AddDefaultTokenProviders()
                .AddRoles<IdentityRole>();
        }

        private static void InitializeDatabase(IServiceScope scope)
        {
            scope.ServiceProvider.GetRequiredService<AppIdentityDbContext>().Database.Migrate();
            scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.Migrate();
        }

        private static async void AddInitialRolesToDatabase(IServiceScope scope)
        {
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            foreach (var roleName in Enum.GetNames(typeof(Enums.Role)))
            {
                bool roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                    await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        private static async void InitStartup(IApplicationBuilder app)
        {
            using IServiceScope scope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope();
            InitializeDatabase(scope);
            AddInitialRolesToDatabase(scope);
            scope.ServiceProvider.GetRequiredService<IDefinitionDataService>();
        }
    }
}