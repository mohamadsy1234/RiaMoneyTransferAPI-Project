using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using RiaMoneyTransferAPI.Model;

namespace RiaMoneyTransferAPI.Service
{
    /// <summary>
    /// Talks to the public Ria money-transfer endpoints to track an order.
    /// It first requests a public session to obtain a JWT, then calls the
    /// customer-toolkit tracking endpoint with that token.
    ///
    /// This mirrors the public tracking flow:
    ///   1. GET  https://public.riamoneytransfer.com/Authorization/session
    ///   2. POST https://customertoolkit.riamoneytransfer.com/api/tracking/order/status
    /// </summary>
    public class OrderTrackingService : IOrderTrackingService
    {
        private const string SessionUrl = "https://public.riamoneytransfer.com/Authorization/session";
        private const string TrackingUrl = "https://customertoolkit.riamoneytransfer.com/api/tracking/order/status";

        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<OrderTrackingService> _logger;

        public OrderTrackingService(IHttpClientFactory httpClientFactory, ILogger<OrderTrackingService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<OrderTrackingResult> TrackOrderAsync(OrderTrackingRequest request, CancellationToken cancellationToken = default)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.OrderNumber))
            {
                return OrderTrackingResult.Failure(request?.OrderNumber ?? string.Empty, "Order number is required.");
            }

            var client = _httpClientFactory.CreateClient();

            string jwt;
            try
            {
                jwt = await GetSessionTokenAsync(client, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to obtain Ria public session token.");
                return OrderTrackingResult.Failure(request.OrderNumber, "Could not obtain a session token from Ria.");
            }

            if (string.IsNullOrWhiteSpace(jwt) || jwt.Contains(".."))
            {
                return OrderTrackingResult.Failure(request.OrderNumber, "Session token was empty or invalid.");
            }

            try
            {
                return await QueryTrackingAsync(client, jwt, request, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to query Ria tracking endpoint for order {OrderNumber}.", request.OrderNumber);
                return OrderTrackingResult.Failure(request.OrderNumber, "Could not retrieve tracking status from Ria.");
            }
        }

        private static async Task<string> GetSessionTokenAsync(HttpClient client, CancellationToken cancellationToken)
        {
            using var sessionRequest = new HttpRequestMessage(HttpMethod.Get, SessionUrl);
            sessionRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            using var sessionResponse = await client.SendAsync(sessionRequest, cancellationToken);
            sessionResponse.EnsureSuccessStatusCode();

            await using var stream = await sessionResponse.Content.ReadAsStreamAsync(cancellationToken);
            using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

            if (doc.RootElement.TryGetProperty("authToken", out var authToken) &&
                authToken.ValueKind == JsonValueKind.Object &&
                authToken.TryGetProperty("jwtToken", out var jwtToken) &&
                jwtToken.ValueKind == JsonValueKind.String)
            {
                return jwtToken.GetString() ?? string.Empty;
            }

            return string.Empty;
        }

        private static async Task<OrderTrackingResult> QueryTrackingAsync(
            HttpClient client,
            string jwt,
            OrderTrackingRequest request,
            CancellationToken cancellationToken)
        {
            var payload = new
            {
                getEventDetails = request.GetEventDetails,
                OrderNumber = request.OrderNumber,
                getEta = request.GetEta
            };

            using var trackingRequest = new HttpRequestMessage(HttpMethod.Post, TrackingUrl)
            {
                Content = new StringContent(
                    JsonSerializer.Serialize(payload),
                    Encoding.UTF8,
                    "application/json")
            };

            trackingRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
            trackingRequest.Headers.TryAddWithoutValidation("Client-Type", "PublicSite");
            trackingRequest.Headers.TryAddWithoutValidation("Apptype", "3");
            trackingRequest.Headers.TryAddWithoutValidation("Locale", "en-US");

            using var trackingResponse = await client.SendAsync(trackingRequest, cancellationToken);
            var body = await trackingResponse.Content.ReadAsStringAsync(cancellationToken);

            if (!trackingResponse.IsSuccessStatusCode)
            {
                return OrderTrackingResult.Failure(
                    request.OrderNumber,
                    $"Tracking endpoint returned {(int)trackingResponse.StatusCode} ({trackingResponse.ReasonPhrase}).");
            }

            JsonElement? data = null;
            if (!string.IsNullOrWhiteSpace(body))
            {
                using var doc = JsonDocument.Parse(body);
                data = doc.RootElement.Clone();
            }

            return new OrderTrackingResult
            {
                OrderNumber = request.OrderNumber,
                Success = true,
                Data = data
            };
        }
    }
}
