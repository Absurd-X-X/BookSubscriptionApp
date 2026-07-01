namespace Infrastructure.Settings
{
    public class PaystackSettings
    {
        public string SecretKey { get; set; } = default!;
        public string BaseUrl { get; set; } = default!;
        public string CallbackUrl { get; set; } = default!;
    }
}
