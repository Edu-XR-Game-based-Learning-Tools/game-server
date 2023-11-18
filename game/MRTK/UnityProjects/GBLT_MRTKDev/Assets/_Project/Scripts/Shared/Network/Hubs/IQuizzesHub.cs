using MagicOnion;
using System.Threading.Tasks;

namespace Shared.Network
{
    public interface IQuizzesHub : IStreamingHub<IQuizzesHub, IQuizzesHubReceiver>
    {
        // The method must return `ValueTask`, `ValueTask<T>`, `Task` or `Task<T>` and can have up to 15 parameters of any type.
        Task<QuizzesStatusResponse> JoinAsync(JoinQuizzesData data);

        Task LeaveAsync();

        // Host
        Task<QuizCollectionListDto> GetCollections();

        Task StartGame(QuizCollectionDto data);

        Task DonePreview();

        Task EndQuestion();

        Task NextQuestion();

        Task EndSession();

        // Player
        Task Answer(AnswerData data);

        Task CmdToKeepAliveConnection();
    }

    public interface IQuizzesHubReceiver
    {
        // The method must have a return type of `void` and can have up to 15 parameters of any type.
        void OnJoin(QuizzesStatusResponse status, QuizzesUserData user);

        void OnLeave(QuizzesStatusResponse status, QuizzesUserData user);

        void OnStart(QuizzesStatusResponse status);

        void OnDonePreview();

        void OnEndQuestion(QuizzesStatusResponse status);

        void OnNextQuestion(QuizzesStatusResponse status);

        void OnEndQuiz(QuizzesStatusResponse status);

        void OnEndSession();

        // To host
        void OnAnswer(AnswerData data);
    }
}
