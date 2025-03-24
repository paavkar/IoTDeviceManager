﻿namespace IoTDeviceManager.server.Models.Auth
{
    public class TokenInfo
    {
        public DateTime AccessTokenExpiresAt { get; set; }
        public TimeSpan? AccessTokenExpiresIn { get; set; }
        public DateTimeOffset? RefreshTokenExpiresAt { get; set; }
        public TimeSpan? RefreshTokenExpiresIn { get; set; }
    }
}
