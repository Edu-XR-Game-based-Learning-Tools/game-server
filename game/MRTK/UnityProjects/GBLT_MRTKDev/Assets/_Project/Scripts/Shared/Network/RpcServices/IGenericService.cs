using MagicOnion;
using MessagePack;
using System;

namespace Shared.Network
{
    public interface IGenericService : IService<IGenericService>
    {
        UnaryResult<ClientVerificationData> VerifyClient(string clientVersion);

        UnaryResult<DateTime> GetServerTime();

        UnaryResult<byte[]> GetDefinitions();

        UnaryResult<EnvironmentGenericConfig> GetGenericConfig();
    }

    [Serializable]
    [MessagePackObject(true)]
    public struct AgoraConfig
    {
        public string AppId { get; set; }
        public string AppChannel { get; set; }
        public string Token { get; set; }
    }

    [Serializable]
    [MessagePackObject(true)]
    public partial struct EnvironmentGenericConfig
    {
        public string LauncherUrl { get; set; }
        public string[] EndPoints { get; set; }
        public AgoraConfig AgoraConfig { get; set; }
    }
}
