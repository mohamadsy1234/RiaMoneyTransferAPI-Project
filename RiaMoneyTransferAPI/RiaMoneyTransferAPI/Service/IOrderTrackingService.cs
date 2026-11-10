using RiaMoneyTransferAPI.Model;

namespace RiaMoneyTransferAPI.Service
{
    public interface IOrderTrackingService
    {
        /// <summary>
        /// Tracks a Ria money-transfer order by its order number / PIN.
        /// Obtains a public session token and queries the tracking endpoint.
        /// </summary>
        Task<OrderTrackingResult> TrackOrderAsync(OrderTrackingRequest request, CancellationToken cancellationToken = default);
    }
}
