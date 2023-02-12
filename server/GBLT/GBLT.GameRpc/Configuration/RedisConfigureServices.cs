using RedLockNet.SERedis;
using RedLockNet.SERedis.Configuration;
using System.Net;

namespace RpcService.Configuration
{
    public class RedisConfig
    {
        public string Host { get; set; }
        public int[] Ports { get; set; }
        public string Password { get; set; }
    }

    public static class RedisConfigureServices
    {
        private const string RedisConfigFieldName = "Redises";
        private const string RedisPasswordFieldName = "Pass";

        public static string GetRedisConnectionString(IConfiguration configuration)
        {
            string connectStr = "", password = "";
            var redisNodes = configuration.GetSection(RedisConfigFieldName);
            foreach (IConfigurationSection node in redisNodes.GetChildren())
            {
                var host = node.GetValue<string>("Host");
                var port = node.GetValue<int>("Port");
                connectStr += host + ":" + port + ",";
                password = node.GetValue<string>(RedisPasswordFieldName);
            }
            connectStr += "password=" + password;

            return connectStr;
        }

        public static RedLockFactory GetRedloadEndpoints(IConfiguration configuration)
        {
            var redisNodes = configuration.GetSection(RedisConfigFieldName);
            List<EndPoint> endPoints = new();
            string password = "";
            foreach (IConfigurationSection node in redisNodes.GetChildren())
            {
                var host = node.GetValue<string>("Host");
                var port = node.GetValue<int>("Port");
                password = node.GetValue<string>(RedisPasswordFieldName);
                endPoints.Add(new DnsEndPoint(host, port));
            }
            var redlockEndPoints = new List<RedLockEndPoint>
                {
                    new RedLockEndPoint(endPoints)
                    {
                        Password = password
                    }
                };

            return RedLockFactory.Create(redlockEndPoints);
        }
    }
}