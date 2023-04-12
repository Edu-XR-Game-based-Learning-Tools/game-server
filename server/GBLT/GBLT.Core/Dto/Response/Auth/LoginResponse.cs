using Shared.Network;

namespace Core.Dto
{
    [Serializable]
    public class LoginResponse : GeneralResponse
    {
        public AccessToken AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }
}