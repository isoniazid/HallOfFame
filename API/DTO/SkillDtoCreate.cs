using System.Text.Json.Serialization;
using HallOfFame.Model;

namespace HallOfFame.DTO
{
    public class SkillDtoCreate : IMapTo<Skill>
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = null!;

        [JsonPropertyName("level")]
        public byte Level { get; set; }
    }
}