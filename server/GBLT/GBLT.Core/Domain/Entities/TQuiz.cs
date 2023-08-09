namespace Core.Entity
{
    public class TQuiz : BaseEntity, IAggregateRoot
    {
        public string Name { get; set; }
        public string Question { get; set; }
        public string ThumbNail { get; set; }
        public string Model { get; set; }
        public string Image { get; set; }

        public string[] Answers { get; set; }
        public int CorrectIdx { get; set; }
        public int Duration { get; set; }

        public TQuizCollection Collection { get; set; }
    }
}