using System.Text.Json.Serialization;

namespace SpaceA.Model.Webhook
{
    public class WebhookRequest
    {
        [JsonPropertyName("object_kind")]
        public string ObjectKind { get; set; }

        [JsonPropertyName("user_username")]
        public string UserName { get; set; }

        public string Ref { get; set; }

        public User User { get; set; }

        public Assignees Assignees { get; set; }

        public Project Project { get; set; }

        [JsonPropertyName("object_attributes")]
        public ObjectAttributes ObjectAttributes { get; set; }
    }
}
