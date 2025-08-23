namespace Team1.VitalBridge.Frontend.Models.DTOs.Auth
{
    public class TokenRes
    {
        public string AccessToken { get; set; }
        public int ExpiresInSeconds { get; set; }

        public TokenRes(string accessToken, int expiresInSeconds)
        {
            AccessToken = accessToken;
            ExpiresInSeconds = expiresInSeconds;
        }
    }
}