namespace SpaceA.WebApi.Options
{
    public class MinioOptions
    {
        public const string PREFIX = "Minio";

        public string Endpoint { get; set; }
        public bool Secure { get; set; }
        public string Bucket { get; set; }
        public string AccessKey { get; set; }
        public string SecretKey { get; set; }
    }
}
