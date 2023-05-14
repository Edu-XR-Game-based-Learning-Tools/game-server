namespace Core.Entity
{
    public class TQuiz : BaseEntity
    {
        public string Name { get; set; }
        public string Quetsion { get; set; }
        public string ThumbNail { get; set; }
        public string Model { get; set; }
        public string Image { get; set; }

        public string[] Answers { get; set; }
        public int CorrectIdx { get; set; }
    }
}