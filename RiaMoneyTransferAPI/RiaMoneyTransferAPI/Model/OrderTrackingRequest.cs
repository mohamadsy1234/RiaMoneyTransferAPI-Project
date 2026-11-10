using System.ComponentModel.DataAnnotations;

namespace RiaMoneyTransferAPI.Model
{
    /// <summary>
    /// Request payload used to track a Ria money-transfer order (PIN / order number).
    /// Mirrors the body expected by the Ria customer-toolkit tracking endpoint.
    /// </summary>
    public class OrderTrackingRequest
    {
        /// <summary>
        /// The order number / PIN of the transfer to track.
        /// </summary>
        [Required]
        public string OrderNumber { get; set; } = string.Empty;

        /// <summary>
        /// When true, the tracking response includes the detailed event timeline.
        /// </summary>
        public bool GetEventDetails { get; set; } = true;

        /// <summary>
        /// When true, the tracking response includes the estimated time of arrival.
        /// </summary>
        public bool GetEta { get; set; } = true;
    }
}
