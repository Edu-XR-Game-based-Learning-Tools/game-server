using MagicOnion;
using MemoryPack;

namespace Shared.Network
{
    [MemoryPackable]
    public partial struct SignInData
    {
        public AuthType AuthType { get; set; }
        public string Code { get; set; }
        public string MetaData { get; set; }

        [MemoryPackIgnore]
        public bool IsFailed => string.IsNullOrEmpty(Code) || string.IsNullOrEmpty(MetaData);
    }

    public interface IAuthServices : IService<IAuthServices>
    {
        UnaryResult<string> GetLoginData(AuthType authType, string metaData = "");

        UnaryResult<AuthenticationData> SignIn(SignInData signInData);
    }
}