using System.Text.Json;

namespace RiaMoneyTransferAPI.Model
{
    /// <summary>
    /// Result of a Ria Partner / Wholesale API call. Wraps the upstream HTTP
    /// status and raw JSON payload along with a success flag.
    /// </summary>
    public class PartnerApiResult
    {
        public bool Success { get; set; }

        /// <summary>
        /// Upstream HTTP status code, when a response was received.
        /// </summary>
        public int? StatusCode { get; set; }

        /// <summary>
        /// Error message when <see cref="Success"/> is false.
        /// </summary>
        public string? Error { get; set; }

        /// <summary>
        /// Raw JSON payload returned by the partner API. Surfaced as-is because
        /// the upstream response shapes are not publicly documented.
        /// </summary>
        public JsonElement? Data { get; set; }

        public static PartnerApiResult Failure(string error, int? statusCode = null) =>
            new PartnerApiResult { Success = false, Error = error, StatusCode = statusCode };
    }
}
