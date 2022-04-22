using System.Text.Json.Serialization;

namespace SpaceA.Model.GitLab
{
    public class Error
    {
        [JsonPropertyName("error")]
        public string ErrorKey { get; set; }
        [JsonPropertyName("error_description")]
        public string ErrorDescription { get; set; }
        public string Message { get; set; }
    }
}