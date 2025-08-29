using System.Text.Json.Serialization;
using Team1.VitalBridge.Frontend.Interfaces.Security;

namespace Team1.VitalBridge.Frontend.Models.Services.Security
{
    public class RecaptchaVerifier : IRecaptchaVerifier
    {
        private readonly HttpClient _http;
        private readonly ILogger<RecaptchaVerifier> _logger;
        private readonly string _secret;

        public RecaptchaVerifier(HttpClient http, IConfiguration cfg, ILogger<RecaptchaVerifier> logger)
        {
            _http = http;
            _logger = logger;
            _secret = cfg["Recaptcha:SecretKey"]
                      ?? throw new InvalidOperationException("Missing Recaptcha:SecretKey");
        }

        public async Task<bool> VerifyAsync(string token, string? remoteIp = null, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(token)) return false;

            var form = new Dictionary<string, string>
            {
                ["secret"] = _secret,
                ["response"] = token,
                // ["remoteip"] = remoteIp ?? ""
            };

            using var resp = await _http.PostAsync(
                "https://www.google.com/recaptcha/api/siteverify",
                new FormUrlEncodedContent(form), ct);

            if (!resp.IsSuccessStatusCode) return false;

            var data = await resp.Content.ReadFromJsonAsync<GoogleRes>(cancellationToken: ct);
            _logger.LogInformation("reCAPTCHA success={Success}, host={Host}, errors={Errors}",
                data?.Success, data?.Hostname, string.Join(",", data?.ErrorCodes ?? new()));

            return data?.Success == true;
        }

        private sealed class GoogleRes
        {
            [JsonPropertyName("success")] public bool Success { get; set; }
            [JsonPropertyName("hostname")] public string? Hostname { get; set; }
            [JsonPropertyName("error-codes")] public List<string>? ErrorCodes { get; set; }
        }
    }
}
