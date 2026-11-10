using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using RiaMoneyTransferAPI.Model;

namespace RiaMoneyTransferAPI.Service
{
    /// <summary>
    /// Calls the Ria Partner / Wholesale API (rialink.net). Every request carries
    /// the agent credentials via the <c>ria-AgentId</c> and
    /// <c>Ocp-Apim-Subscription-Key</c> headers, read from configuration.
    /// </summary>
    public class PartnerOrderService : IPartnerOrderService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly PartnerApiOptions _options;
        private readonly ILogger<PartnerOrderService> _logger;

        public PartnerOrderService(
            IHttpClientFactory httpClientFactory,
            IOptions<PartnerApiOptions> options,
            ILogger<PartnerOrderService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _options = options.Value;
            _logger = logger;
        }

        public Task<PartnerApiResult> CashPickUpAsync(JsonElement body, CancellationToken cancellationToken = default) =>
            SendAsync(HttpMethod.Post, "/PayOrders/Order/CashPickUp", body, cancellationToken);

        public Task<PartnerApiResult> ReleaseAsync(JsonElement body, CancellationToken cancellationToken = default) =>
            SendAsync(HttpMethod.Post, "/SendOrders/Order/Release", body, cancellationToken);

        public Task<PartnerApiResult> GetStatusByScOrderNoAsync(string scOrderNo, CancellationToken cancellationToken = default) =>
            SendAsync(HttpMethod.Get, $"/SendOrders/Orders/SCOrderNo/{Uri.EscapeDataString(scOrderNo)}/Status", null, cancellationToken);

        public Task<PartnerApiResult> GetStatusByPcOrderNoAsync(string pcOrderNo, CancellationToken cancellationToken = default) =>
            SendAsync(HttpMethod.Get, $"/SendOrders/Orders/PCOrderNo/{Uri.EscapeDataString(pcOrderNo)}/Status", null, cancellationToken);

        public Task<PartnerApiResult> GetStagedByScOrderNoAsync(string scOrderNo, CancellationToken cancellationToken = default) =>
            SendAsync(HttpMethod.Get, $"/SendOrders/Orders/SCOrderNo/{Uri.EscapeDataString(scOrderNo)}?StagedOrdersOnly", null, cancellationToken);

        private async Task<PartnerApiResult> SendAsync(
            HttpMethod method,
            string relativePath,
            JsonElement? body,
            CancellationToken cancellationToken)
        {
            if (!_options.IsConfigured)
            {
                return PartnerApiResult.Failure(
                    "Partner API is not configured. Set RiaPartnerApi:AgentId and RiaPartnerApi:SubscriptionKey.");
            }

            if (!Uri.TryCreate(CombineUrl(_options.BaseUrl, relativePath), UriKind.Absolute, out var uri))
            {
                return PartnerApiResult.Failure("Partner API base URL is invalid.");
            }

            var client = _httpClientFactory.CreateClient();

            using var httpRequest = new HttpRequestMessage(method, uri);
            httpRequest.Headers.TryAddWithoutValidation("ria-AgentId", _options.AgentId);
            httpRequest.Headers.TryAddWithoutValidation("Ocp-Apim-Subscription-Key", _options.SubscriptionKey);
            httpRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (body.HasValue && method != HttpMethod.Get)
            {
                httpRequest.Content = new StringContent(body.Value.GetRawText(), Encoding.UTF8, "application/json");
            }

            try
            {
                using var response = await client.SendAsync(httpRequest, cancellationToken);
                var content = await response.Content.ReadAsStringAsync(cancellationToken);

                JsonElement? data = null;
                if (!string.IsNullOrWhiteSpace(content) && LooksLikeJson(content))
                {
                    try
                    {
                        using var doc = JsonDocument.Parse(content);
                        data = doc.RootElement.Clone();
                    }
                    catch (JsonException)
                    {
                        // Non-JSON body; leave data null and fall through.
                    }
                }

                if (!response.IsSuccessStatusCode)
                {
                    return new PartnerApiResult
                    {
                        Success = false,
                        StatusCode = (int)response.StatusCode,
                        Error = $"Partner API returned {(int)response.StatusCode} ({response.ReasonPhrase}).",
                        Data = data
                    };
                }

                return new PartnerApiResult
                {
                    Success = true,
                    StatusCode = (int)response.StatusCode,
                    Data = data
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Partner API call failed: {Method} {Path}", method, relativePath);
                return PartnerApiResult.Failure("Partner API request failed.");
            }
        }

        private static string CombineUrl(string baseUrl, string relativePath) =>
            $"{baseUrl.TrimEnd('/')}/{relativePath.TrimStart('/')}";

        private static bool LooksLikeJson(string content)
        {
            var trimmed = content.TrimStart();
            return trimmed.StartsWith("{") || trimmed.StartsWith("[");
        }
    }
}
