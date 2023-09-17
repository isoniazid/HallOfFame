using System.Text.Json.Serialization;

namespace HallOfFame.DTO
{
    public class PersonDtoUpdate : IMapTo<Model.Person>
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = null!;

        [JsonPropertyName("displayName")]
        public string DisplayName { get; set; } = null!;

        [JsonPropertyName("skills")]
        public List<SkillDtoCreate>? Skills { get; set; }
    }
}
