using Grpc.Net.Client;
using Infrastructure;
using LitJWT;
using LitJWT.Algorithms;
using MagicOnion.Server;
using MessagePipe;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RedLockNet;
using RedLockNet.SERedis;
using RpcService.Authentication;
using RpcService.Configuration;

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
            services.Configure<JwtTokenServiceOptions>(Configuration.GetSection("JwtToken"));
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    JwtTokenServiceOptions jwtOptions = Configuration.GetSection("JwtToken").Get<JwtTokenServiceOptions>();
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        IssuerSigningKey = new SymmetricSecurityKey(Convert.FromBase64String(jwtOptions.Secret)),
                        RequireExpirationTime = true,
                        RequireSignedTokens = true,
                        ClockSkew = TimeSpan.FromDays(jwtOptions.AuthTokenExpireHour),

                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                    };
#if DEBUG
                    options.RequireHttpsMetadata = false;
#endif
                });

            services.AddAuthorization();
            services.AddMessagePipe();

            services.AddCoreServices();
            services.AddDelegateService();

            services.AddControllersWithViews();
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
    }
}