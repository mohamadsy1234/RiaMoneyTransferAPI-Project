using Microsoft.AspNetCore.Mvc;
using RiaMoneyTransferAPI.Model;
using RiaMoneyTransferAPI.Service;

namespace RiaMoneyTransferAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IOrderTrackingService _orderTrackingService;

        public OrderController(IOrderTrackingService orderTrackingService)
        {
            _orderTrackingService = orderTrackingService;
        }

        /// <summary>
        /// Tracks a money-transfer order by its order number / PIN.
        /// </summary>
        /// <example>GET /api/Order/track/12598805132</example>
        [HttpGet("/api/Order/track/{orderNumber}")]
        public async Task<IActionResult> Track(string orderNumber, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(orderNumber))
            {
                return BadRequest("Order number is required.");
            }

            var request = new OrderTrackingRequest { OrderNumber = orderNumber };
            var result = await _orderTrackingService.TrackOrderAsync(request, cancellationToken);

            return result.Success ? Ok(result) : StatusCode(StatusCodes.Status502BadGateway, result);
        }

        /// <summary>
        /// Tracks a money-transfer order using a full request body, allowing the
        /// caller to control whether event details and ETA are included.
        /// </summary>
        [HttpPost("/api/Order/track")]
        public async Task<IActionResult> Track([FromBody] OrderTrackingRequest request, CancellationToken cancellationToken)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.OrderNumber))
            {
                return BadRequest("Order number is required.");
            }

            var result = await _orderTrackingService.TrackOrderAsync(request, cancellationToken);

            return result.Success ? Ok(result) : StatusCode(StatusCodes.Status502BadGateway, result);
        }
    }
}
