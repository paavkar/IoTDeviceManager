namespace IoTDeviceManager.server.Models.Auth
{
    public class RefreshToken
    {
        public string Id { get; set; }
        public string Token { get; set; }
        public DateTimeOffset Expires { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public bool Revoked { get; set; } = false;
        public string UserId { get; set; }
    }
}
