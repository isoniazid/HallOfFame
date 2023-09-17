namespace HallOfFame.Model
{
    public class Person
    {
        public long Id {get; set;}

        public string Name {get; set;} = null!;

        public string DisplayName {get; set;} = null!;

        public List<Skill>? Skills {get; set;}
    }
}