using System.Text.Json.Serialization;

namespace SpaceA.Model.GitLab
{
    public class Hook
    {
        public uint Id { get; set; }
        public string Url { get; set; }
        [JsonPropertyName("push_events")]
        public bool PushEvents { get; set; }
    }
}