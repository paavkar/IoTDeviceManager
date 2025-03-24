namespace IoTDeviceManager.server.Models.Auth
{
    public class UserDTO
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public List<string> Roles { get; set; }
        public TokenInfo TokenInfo { get; set; }
    }
}
