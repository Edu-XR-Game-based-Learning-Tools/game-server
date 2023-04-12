using MagicOnion;
using MemoryPack;
using System;

namespace Shared.Network
{
    public interface IGenericService : IService<IGenericService>
    {
        UnaryResult<ClientVerificationData> VerifyClient(string clientVersion);

        UnaryResult<DateTime> GetServerTime();

        UnaryResult<EnvironmentGenericConfig> GetGenericConfig();
    }

    [Serializable]
    [MemoryPackable]
    public partial struct EnvironmentGenericConfig
    {
        public string LauncherUrl { get; set; }
        public string[] EndPoints { get; set; }
        public string[] ContractEndPoints { get; set; }
    }
}