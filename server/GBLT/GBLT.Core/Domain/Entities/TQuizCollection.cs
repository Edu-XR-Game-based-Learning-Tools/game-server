namespace Core.Entity
{
    public class TQuizCollection : BaseEntity
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Configuration { get; set; }

        public TUser Owner { get; private set; }
        public ICollection<TQuiz> Quizzes { get; private set; }
    }
}