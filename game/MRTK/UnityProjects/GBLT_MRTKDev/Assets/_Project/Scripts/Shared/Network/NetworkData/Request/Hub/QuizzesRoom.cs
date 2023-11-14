using MessagePack;

namespace Shared.Network
{
    public enum ToolType
    {
        Quizzes,
    }

    public enum QuizzesStatus
    {
        Pending,
        InProgress,
        End,
    }

    [System.Serializable]
    [MessagePackObject(true)]
    public class JoinQuizzesData
    {
        public PrivateUserData UserData { get; set; }
        public string RoomId { get; set; }
        public QuizzesStatus QuizzesStatus { get; set; } = QuizzesStatus.Pending;
        public int CurrentQuestionIdx { get; set; }
    }

    [System.Serializable]
    [MessagePackObject(true)]
    public class InviteToGameData
    {
        public ToolType ToolType { get; set; }
        public string RoomId { get; set; }
        public JoinQuizzesData JoinQuizzesData { get; set; }
    }

    [System.Serializable]
    [MessagePackObject(true)]
    public class AnswerData
    {
        public QuizzesUserData UserData { get; set; }
        public int AnswerIdx { get; set; }
    }
}
