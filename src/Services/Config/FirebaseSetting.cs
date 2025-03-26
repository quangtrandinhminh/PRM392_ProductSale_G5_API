namespace Services.Config
{
    public class FirebaseSetting
    {
        public static FirebaseSetting Instance { get; set; }
        
        public string ProjectId { get; set; }
        public string ClientEmail { get; set; }
        public string PrivateKey { get; set; }
    }
}
