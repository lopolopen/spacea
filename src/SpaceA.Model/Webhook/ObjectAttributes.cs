using System.Text.Json.Serialization;

namespace SpaceA.Model.Webhook
{
    public class ObjectAttributes
    {
        public string Id { get; set; }

        [JsonPropertyName("target_branch")]
        public string TargetBranch { get; set; }

        [JsonPropertyName("source_branch")]
        public string SourceBranch { get; set; }

        public string State { get; set; }

        public string Url { get; set; }

        [JsonPropertyName("merge_status")]
        public string MergeStatus { get; set; }
    }
}