using System.Text.Json;
using RiaMoneyTransferAPI.Model;

namespace RiaMoneyTransferAPI.Service
{
    /// <summary>
    /// Client for the Ria Partner / Wholesale API (rialink.net). Covers the
    /// pay-order and send-order operations used by agents.
    /// </summary>
    public interface IPartnerOrderService
    {
        /// <summary>POST /PayOrders/Order/CashPickUp — search a cash-pickup order.</summary>
        Task<PartnerApiResult> CashPickUpAsync(JsonElement body, CancellationToken cancellationToken = default);

        /// <summary>POST /SendOrders/Order/Release — release a send order.</summary>
        Task<PartnerApiResult> ReleaseAsync(JsonElement body, CancellationToken cancellationToken = default);

        /// <summary>GET /SendOrders/Orders/SCOrderNo/{id}/Status — status by sending-agent order number.</summary>
        Task<PartnerApiResult> GetStatusByScOrderNoAsync(string scOrderNo, CancellationToken cancellationToken = default);

        /// <summary>GET /SendOrders/Orders/PCOrderNo/{id}/Status — status by Ria (PC) order number.</summary>
        Task<PartnerApiResult> GetStatusByPcOrderNoAsync(string pcOrderNo, CancellationToken cancellationToken = default);

        /// <summary>GET /SendOrders/Orders/SCOrderNo/{id}?StagedOrdersOnly — staged (not-yet-ready) orders.</summary>
        Task<PartnerApiResult> GetStagedByScOrderNoAsync(string scOrderNo, CancellationToken cancellationToken = default);
    }
}
