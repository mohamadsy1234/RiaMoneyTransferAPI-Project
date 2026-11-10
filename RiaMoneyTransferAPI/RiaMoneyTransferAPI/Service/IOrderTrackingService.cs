using RiaMoneyTransferAPI.Model;

namespace RiaMoneyTransferAPI.Service
{
    public interface IOrderTrackingService
    {
        /// <summary>
        /// Tracks a Ria money-transfer order by its order number / PIN.
        /// </summary>
        /// <param name="request">The order to track.</param>
        /// <param name="authToken">
        /// Optional bearer token supplied by the caller (e.g. forwarded from an
        /// authenticated session). When provided it is used as-is; otherwise an
        /// anonymous public session token is obtained automatically. A leading
        /// "Bearer " prefix is accepted and stripped.
        /// </param>
        /// <param name="cancellationToken">Cancellation token.</param>
        Task<OrderTrackingResult> TrackOrderAsync(OrderTrackingRequest request, string? authToken = null, CancellationToken cancellationToken = default);
    }
}
