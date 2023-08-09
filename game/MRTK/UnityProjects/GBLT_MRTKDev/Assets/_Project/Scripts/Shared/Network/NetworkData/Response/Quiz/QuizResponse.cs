using MessagePack;
using System;

namespace Shared.Network
{
    [Serializable]
    [MessagePackObject(true)]
    public class QuizCollectionListDto : BaseDbDto
    {
        public QuizCollectionDto[] Collections { get; set; }
        public int Total => Collections != null ? Collections.Length : 0;
    }

    [Serializable]
    [MessagePackObject(true)]
    public class QuizCollectionDto : BaseDbDto
    {
        public string Description { get; set; }
        public string Configuration { get; set; }

        public QuizDto[] Quizzes { get; set; }
    }

    [Serializable]
    [MessagePackObject(true)]
    public class QuizDto : BaseDbDto
    {
        public string Question { get; set; }
        public string ThumbNail { get; set; }
        public string Model { get; set; }
        public string Image { get; set; }
        public int Duration { get; set; }

        public string[] Answers { get; set; }
        public int CorrectIdx { get; set; }
    }
}
