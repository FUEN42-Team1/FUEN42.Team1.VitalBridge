namespace Team1.VitalBridge.Frontend.Interfaces.Security
{
    public interface IRecaptchaVerifier
    {
        Task<bool> VerifyAsync(string token, string? remoteIp = null, CancellationToken ct = default);
    }
}
