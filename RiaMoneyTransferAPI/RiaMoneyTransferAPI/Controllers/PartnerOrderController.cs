using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using RiaMoneyTransferAPI.Service;

namespace RiaMoneyTransferAPI.Controllers
{
    /// <summary>
    /// Endpoints that proxy the Ria Partner / Wholesale API (rialink.net).
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class PartnerOrderController : ControllerBase
    {
        private readonly IPartnerOrderService _partnerOrderService;

        public PartnerOrderController(IPartnerOrderService partnerOrderService)
        {
            _partnerOrderService = partnerOrderService;
        }

        /// <summary>Search a cash-pickup order. Forwards the request body to the partner API.</summary>
        [HttpPost("/api/PartnerOrder/cash-pickup")]
        public async Task<IActionResult> CashPickUp([FromBody] JsonElement body, CancellationToken cancellationToken)
        {
            var result = await _partnerOrderService.CashPickUpAsync(body, cancellationToken);
            return ToActionResult(result);
        }

        /// <summary>Release a send order. Forwards the request body to the partner API.</summary>
        [HttpPost("/api/PartnerOrder/release")]
        public async Task<IActionResult> Release([FromBody] JsonElement body, CancellationToken cancellationToken)
        {
            var result = await _partnerOrderService.ReleaseAsync(body, cancellationToken);
            return ToActionResult(result);
        }

        /// <summary>Order status by sending-agent order number (SCOrderNo).</summary>
        [HttpGet("/api/PartnerOrder/sc/{scOrderNo}/status")]
        public async Task<IActionResult> StatusByScOrderNo(string scOrderNo, CancellationToken cancellationToken)
        {
            var result = await _partnerOrderService.GetStatusByScOrderNoAsync(scOrderNo, cancellationToken);
            return ToActionResult(result);
        }

        /// <summary>Order status by Ria order number (PCOrderNo).</summary>
        [HttpGet("/api/PartnerOrder/pc/{pcOrderNo}/status")]
        public async Task<IActionResult> StatusByPcOrderNo(string pcOrderNo, CancellationToken cancellationToken)
        {
            var result = await _partnerOrderService.GetStatusByPcOrderNoAsync(pcOrderNo, cancellationToken);
            return ToActionResult(result);
        }

        /// <summary>Staged (not-yet-ready) orders by sending-agent order number.</summary>
        [HttpGet("/api/PartnerOrder/sc/{scOrderNo}/staged")]
        public async Task<IActionResult> StagedByScOrderNo(string scOrderNo, CancellationToken cancellationToken)
        {
            var result = await _partnerOrderService.GetStagedByScOrderNoAsync(scOrderNo, cancellationToken);
            return ToActionResult(result);
        }

        private IActionResult ToActionResult(Model.PartnerApiResult result)
        {
            if (result.Success)
            {
                return Ok(result);
            }

            // Not configured → 503; upstream error → 502.
            var status = result.StatusCode.HasValue
                ? StatusCodes.Status502BadGateway
                : StatusCodes.Status503ServiceUnavailable;

            return StatusCode(status, result);
        }
    }
}
