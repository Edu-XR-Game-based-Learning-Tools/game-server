using LMS.Server.Core.Interfaces;
using Microsoft.Extensions.Configuration;

namespace LMS.Server.Infrastructure.Services
{
    public class GenericService : IGenericService
    {
        private readonly IConfiguration _configuration;

        public static GenericConfig GenericConfig { get; private set; }

        public GenericService(
            IConfiguration configuration)
        {
            _configuration = configuration;

            if (GenericConfig.JWTIssuer == "")
                GenericConfig = _configuration.GetSection("GenericConfig").Get<GenericConfig>();
        }
    }
}