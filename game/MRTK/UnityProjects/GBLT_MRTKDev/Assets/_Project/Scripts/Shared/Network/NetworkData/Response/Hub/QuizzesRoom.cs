using MessagePack;
using Shared.Extension;
using System.Linq;

namespace Shared.Network
{
    [System.Serializable]
    [MessagePackObject(true)]
    public class QuizzesUserData
    {
        public PublicUserData UserData { get; set; }
        public System.Guid? QuizzesConnectionId { get; set; }
        public int Score { get; set; }
        public int Rank { get; set; }
        public int Index { get; set; }
        public bool IsHost => Index == -1;
        public int? CorrectIdx { get; set; } = null;
        public bool IsConnected { get; set; } = false;
    }

    [System.Serializable]
    [MessagePackObject(true)]
    public class QuizzesStatusResponse : GeneralResponse
    {
        public string Id { get; set; }
        public QuizzesUserData Self { get; set; }
        public QuizzesUserData[] AllInRoom { get; set; }
        public QuizzesUserData[] Others => AllInRoom.WhereNot((ele) => ele.Index == Self.Index).ToArray();
        public int Amount => AllInRoom.Length + 1;
        public JoinQuizzesData JoinQuizzesData { get; set; }
        public QuizCollectionDto QuizCollection { get; set; }
    }
}
