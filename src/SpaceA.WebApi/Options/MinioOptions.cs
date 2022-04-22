namespace SpaceA.WebApi.Options
{
    public class MinioOptions
    {
        public const string PREFIX = "MinIO";

        public string Endpoint { get; set; }
        public bool Secure { get; set; }
        public string AccessKey { get; set; }
        public string SecretKey { get; set; }
    }
}
