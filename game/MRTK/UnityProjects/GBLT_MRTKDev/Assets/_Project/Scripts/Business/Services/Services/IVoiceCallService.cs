using Cysharp.Threading.Tasks;

namespace Core.Business
{
    public interface IVoiceCallService
    {
        public UniTask JoinChannelAsync(string token = null, string channel = null);

        public UniTask LeaveChannelAsync();

        public (byte[], int) GetSamples();
    }
}
