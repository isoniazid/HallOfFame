using System.Text.Json.Serialization;
using HallOfFame.Model;

namespace HallOfFame.DTO
{
    public class PersonDtoCreate : IMapTo<Person>
    {
        [JsonPropertyName("name")]
        public string Name {get; set;} = null!;

        [JsonPropertyName("displayName")]
        public string DisplayName {get; set;} = null!;

        [JsonPropertyName("skills")]
        public List<SkillDtoCreate>? Skills {get; set;}    
    }
}