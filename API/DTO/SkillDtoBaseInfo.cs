using System.Text.Json.Serialization;
using HallOfFame.Model;

namespace HallOfFame.DTO
{
    public class SkillDtoBaseInfo : IMapFrom<Skill>
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = null!;

        [JsonPropertyName("level")]
        public byte Level { get; set; }

    }
}