namespace Core.Entity
{
    public class TQuizCollection : BaseEntity, IAggregateRoot
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Configuration { get; set; }

        public TUser Owner { get; set; }
        public IList<TQuiz> Quizzes { get; set; }
    }
}