using System.Text.Json;

namespace RiaMoneyTransferAPI.Model
{
    /// <summary>
    /// Result of an order-tracking call. Wraps the raw response returned by the
    /// Ria tracking endpoint along with a success flag and an optional error message.
    /// </summary>
    public class OrderTrackingResult
    {
        /// <summary>
        /// The order number / PIN that was tracked.
        /// </summary>
        public string OrderNumber { get; set; } = string.Empty;

        /// <summary>
        /// True when the tracking endpoint returned a successful response.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Human-readable error message when <see cref="Success"/> is false.
        /// </summary>
        public string? Error { get; set; }

        /// <summary>
        /// The raw JSON payload returned by the tracking endpoint. The upstream
        /// response shape is not fixed, so it is surfaced as-is for the caller.
        /// </summary>
        public JsonElement? Data { get; set; }

        public static OrderTrackingResult Failure(string orderNumber, string error) =>
            new OrderTrackingResult
            {
                OrderNumber = orderNumber,
                Success = false,
                Error = error
            };
    }
}
